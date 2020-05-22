using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    //general variables
    public Vector3Int Coords = new Vector3Int();
    public List<Connection> Connections = new List<Connection>();

    //List of objects that stay on tile (characters, effects and so on)
    public List<GameObject> ObjectsOnTile = new List<GameObject>();

    //gameplay variables
    public enum TileGameStatus
    {
        Empty = 0,
        Block = 1,
        Ally = 2,
        Enemy = 3,
    }
    public TileGameStatus GameStatus;
    public bool HasEffect;

    public enum InfluenceStatus
    {
        Move = 0,
        MeleeAttack = 1,
    }
    //some kinf of influence map
    //contains pair of character's battle id and influence that it has on this tile
    public List<KeyValuePair<int, InfluenceStatus>> Influences;

    //processing variables
    public enum NodeProcessStatus
    {
        NotVisited = 0,
        InOpenList = 1,
        InClosedList = 2,
    }
    public NodeProcessStatus ProcessStatus;
    public int ProcessValue;

    public float CostSoFar;
    public float EstimatedCost;

    //How we went here in pathfinding
    public Connection connection;

    public bool AddConnection(Connection connection)
    {
        foreach (var oldConnection in Connections)
        {
            if (oldConnection.StartNode == connection.StartNode && oldConnection.EndNode == connection.EndNode)
                return false;
        }
        Connections.Add(connection);
        return true;
    }

    public bool AddConnection(Node to)
    {
        foreach(var connection in Connections)
        {
            if (connection.StartNode == this && connection.EndNode == to)
                return false;
        }
        Connection newConnection = new Connection(this, to);
        Connections.Add(newConnection);
        return true;
    }

    public void RemoveConnection(Node to)
    {
        for(int i = 0; i < Connections.Count; ++i)
        {
            var connection = Connections[i];
            if (connection.EndNode == to)
            {
                Connections.Remove(connection);
                --i;
            }
        }
    }

    public Character GetCharacter()
    {
        foreach(var gameobject in ObjectsOnTile)
        {
            Character character;
            bool ok = gameobject.TryGetComponent<Character>(out character);
            if (ok)
                return character;
        }
        return null;
    }

    public bool AddCharacter(Character character)
    {
        if (GameStatus == Node.TileGameStatus.Empty)
        {
            GameStatus = GetTileStatusFromCharacter(character);
            ObjectsOnTile.Add(character.gameObject);
        }
        return false;
    }

    public bool RemoveCharacter(Character character)
    {
        if (GameStatus == Node.TileGameStatus.Ally || GameStatus == Node.TileGameStatus.Enemy)
        {
            foreach (var gameobject in ObjectsOnTile)
            {
                Character oldCharacter;
                bool ok = gameobject.TryGetComponent<Character>(out oldCharacter);
                if (ok && oldCharacter == character)
                {
                    ObjectsOnTile.Remove(character.gameObject);
                    GameStatus = Node.TileGameStatus.Empty;
                    return true;
                }
            }
        }
        return false;
    }

    public EffectTile GetEffect()
    {
        if (HasEffect)
        {
            foreach(var gameonject in ObjectsOnTile)
            {
                EffectTile effect;
                bool ok = gameonject.TryGetComponent(out effect);
                if (ok)
                    return effect;
            }
        }
        return null;
    }

    public bool AddEffect(EffectTile effect)
    {
        //if tile already has effect when find it and rewrite
        if (HasEffect)
        {
            foreach (var gameobject in ObjectsOnTile)
            {
                EffectTile oldEffect;
                bool ok = gameobject.TryGetComponent<EffectTile>(out oldEffect);
                if (ok)
                {
                    ObjectsOnTile.Remove(oldEffect.gameObject);
                    oldEffect.EndEffect();
                }
            }
        }

        ObjectsOnTile.Add(effect.gameObject);
        HasEffect = true;
        effect.StartEffect();
        return true;
    }

    public bool RemoveEffect(EffectTile effect)
    {
        if (HasEffect)
        {
            foreach (var gameobject in ObjectsOnTile)
            {
                EffectTile oldEffect;
                bool ok = gameobject.TryGetComponent<EffectTile>(out oldEffect);
                if (ok && oldEffect == effect)
                {
                    ObjectsOnTile.Remove(oldEffect.gameObject);
                    oldEffect.EndEffect();
                    break;
                }
            }
            HasEffect = false;
            return true;
        }
        return false;
    }

    static public TileGameStatus GetTileStatusFromCharacter(Character character)
    {
        if (character.gameObject.CompareTag("Ally"))
            return TileGameStatus.Ally;
        else if (character.gameObject.CompareTag("Enemy"))
            return TileGameStatus.Enemy;
        else
            return TileGameStatus.Empty;
    }
}

public class Connection
{
    //public float Cost;
    public Node StartNode;
    public Node EndNode;

    public Connection()
    {
        //Cost = 0;
        StartNode = null;
        EndNode = null;
    }

    public Connection(Node from, Node to)
    {
        //Cost = cost;
        StartNode = from;
        EndNode = to;
    }
}

public class PathfindingGraph
{
    //key - coordinates in form (x,y)
    private Dictionary<string, Node> _nodeGraph;

    public PathfindingGraph()
    {
        _nodeGraph = new Dictionary<string, Node>();
    }

    public void GetNodeCoordinates(string nodeKey, out int x, out int y)
    {
        char[] splitters = { '(', ',', ')' };
        string[] coords = nodeKey.Split(splitters);
        x = int.Parse(coords[0]);
        y = int.Parse(coords[1]);
    }

    public string CreateNodeKeyFromCoordinates(int x, int y)
    {
        return $"({x},{y})";
    }

    public Node GetNode(Vector3Int coords)
    {
        string key = CreateNodeKeyFromCoordinates(coords.x, coords.y);
        if (_nodeGraph.ContainsKey(key))
            return _nodeGraph[key];
        return null;
    }

    public Node GetNode(string coords)
    {
        if (_nodeGraph.ContainsKey(coords))
            return _nodeGraph[coords];
        return null;
    }

    public void RestoreProcessStatus()
    {
        foreach (var node in _nodeGraph.Values)
        {
            node.ProcessStatus = Node.NodeProcessStatus.NotVisited;
            node.ProcessValue = 0;
        }
    }

    public void ClearInfluenceData()
    {
        foreach (var node in _nodeGraph.Values)
        {
            node.Influences.Clear();
        }
    }

    public bool AddNode(Node node)
    {
        string key = CreateNodeKeyFromCoordinates(node.Coords.x, node.Coords.y);
        if (_nodeGraph.ContainsKey(key))
            return false;
        _nodeGraph.Add(key, node);
        return true;
    }

    public bool Contains(string coords)
    {
        return _nodeGraph.ContainsKey(coords);
    }

    public bool Contains(Node node)
    {
        string key = CreateNodeKeyFromCoordinates(node.Coords.x, node.Coords.y);
        return _nodeGraph.ContainsKey(key);
    }
}
