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

    public bool HasEffect;

    public TileGameStatus GameStatus;

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

    public bool AddConnection(float cost, Node to)
    {
        foreach(var connection in Connections)
        {
            if (connection.StartNode == this && connection.EndNode == to)
                return false;
        }
        Connection newConnection = new Connection(cost, this, to);
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

    //Now one effect rewrite another
    public bool AddEffect(EffectTile effect)
    {
        //if tile already has effect when find it and rewrite
        if (HasEffect)
        {
            foreach(var gameobject in ObjectsOnTile)
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
                }
            }
            HasEffect = false;
            return true;
        }
        return false;
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
}

public class Connection
{
    public float Cost;
    public Node StartNode;
    public Node EndNode;

    public Connection()
    {
        Cost = 0;
        StartNode = null;
        EndNode = null;
    }

    public Connection(float cost, Node from, Node to)
    {
        Cost = cost;
        StartNode = from;
        EndNode = to;
    }
}

public class PathfindingGraph
{
    //key - coordinates in form (x,y)
    public Dictionary<string, Node> NodeGraph;

    public PathfindingGraph()
    {
        NodeGraph = new Dictionary<string, Node>();
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
        if (NodeGraph.ContainsKey(key))
            return NodeGraph[key];
        return null;
    }

    public void RestoreProcessStatus()
    {
        foreach (var node in NodeGraph.Values)
        {
            node.ProcessStatus = Node.NodeProcessStatus.NotVisited;
            node.ProcessValue = 0;
        }
    }

    public List<Node> AStarPathfinding(Node start, Node end)
    {
        start.CostSoFar = 0;
        start.EstimatedCost = Heuristic(start, end);

        List<Node> openList = new List<Node>();
        openList.Add(start);

        Node currentNode = null;
        while (openList.Count != 0)
        {
            currentNode = openList[0];

            //find node with smallest estimated cost in open list
            foreach(var node in openList)
            {
                if (node.EstimatedCost < currentNode.EstimatedCost)
                    currentNode = node;
            }

            //if reach goal - stop searching and start build path
            if (currentNode == end)
                break;

            foreach(var connection in currentNode.Connections)
            {
                Node endNode = connection.EndNode;
                float cost = currentNode.CostSoFar + connection.Cost;
                float heuristic;

                //Characters cant go through another characters
                //So we need to ignore nodes that taken by ally or enemy (if this is not goal node)
                if ((endNode.GameStatus == Node.TileGameStatus.Ally || endNode.GameStatus == Node.TileGameStatus.Enemy) && endNode != end)
                    continue;

                if (endNode.ProcessStatus == Node.NodeProcessStatus.InClosedList)
                {
                    if (endNode.CostSoFar < cost)
                        continue;
                    heuristic = endNode.EstimatedCost - endNode.CostSoFar;
                }
                else if (endNode.ProcessStatus == Node.NodeProcessStatus.InOpenList)
                {
                    if (endNode.CostSoFar < cost)
                        continue;
                    heuristic = endNode.EstimatedCost - endNode.CostSoFar;
                }
                else
                    heuristic = Heuristic(endNode, end);

                endNode.connection = connection;
                endNode.CostSoFar = cost;
                endNode.EstimatedCost = cost + heuristic;
                if (endNode.ProcessStatus != Node.NodeProcessStatus.InOpenList)
                {
                    endNode.ProcessStatus = Node.NodeProcessStatus.InOpenList;
                    openList.Add(endNode);
                }
            }
            openList.Remove(currentNode);
            currentNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
        }

        //if goal node did not found
        if (currentNode != end)
            return null;
        //else build path
        List<Node> path = new List<Node>();
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.connection.StartNode;
        }
        path.Reverse();
        return path;
    }

    public float Heuristic(Node start, Node end)
    {
        float x = start.Coords.x - end.Coords.x;
        float y = start.Coords.y - end.Coords.y;

        return Mathf.Sqrt(x*x+y*y);
    }

    public void SetNodeGameplayStatus(Vector3Int coords, Node.TileGameStatus status)
    {
        NodeGraph[CreateNodeKeyFromCoordinates(coords.x, coords.y)].GameStatus = status;
    }
}
