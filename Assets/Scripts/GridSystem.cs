﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;
    public Camera CurrentCamera;
    public Tilemap CurrentTilemap;

    private PathfindingGraph _graph;

    public Tilemap Movemap;
    public Tile MoveTile;
    public Tile PathTile;
    public Tile AllyTile;
    public Tile EnemyTile;

    private List<Vector3Int> _moveMap;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GridSystem object already exists");
        }
        else
        {
            Instance = this;
        }
        InitializeGraph();
        _graph.RestoreProcessStatus();
    }

    private void Start()
    {
        Debug.Log($"Tilemap data:\n ");
        Debug.Log($"Bounds: ({CurrentTilemap.cellBounds.x}, {CurrentTilemap.cellBounds.x})\n");
        Debug.Log($"Origin: ({CurrentTilemap.origin.x}, {CurrentTilemap.origin.x})\n");
        Debug.Log($"Size : {CurrentTilemap.size})");
    }

    private void Update()
    {
       
    }


    /*
     * Movemap section
     */

    public List<Vector3Int> GetMoveMap(Node.TileGameStatus fraction, int moveDistance, Vector3Int position)
    {
        List<Vector3Int> map = new List<Vector3Int>();
        List<Node> nodesToProcess = new List<Node>();

        Node currentNode = _graph.GetNode(position);
        if (currentNode.GameStatus == Node.TileGameStatus.Block)
            return map;
        currentNode.ProcessValue = moveDistance;
        currentNode.ProcessStatus = Node.NodeProcessStatus.InOpenList;
        nodesToProcess.Add(currentNode);

        Node.TileGameStatus oppositeFraction;
        if (fraction == Node.TileGameStatus.Ally)
            oppositeFraction = Node.TileGameStatus.Enemy;
        else
            oppositeFraction = Node.TileGameStatus.Ally;

        while(nodesToProcess.Count != 0)
        {
            currentNode = nodesToProcess[0];
            foreach(var connection in currentNode.Connections)
            {
                Node endNode = connection.EndNode;

                if ((endNode.ProcessStatus == Node.NodeProcessStatus.NotVisited ||
                    endNode.ProcessValue < currentNode.ProcessValue - 1) && endNode.GameStatus != fraction)
                {
                    //first entry in open list and not taken by ally
                    if (endNode.ProcessStatus == Node.NodeProcessStatus.NotVisited)
                        map.Add(endNode.Coords);

                    //hit to enemy ends turn
                    if (endNode.GameStatus == oppositeFraction)
                        endNode.ProcessValue = 0;
                    else
                        endNode.ProcessValue = currentNode.ProcessValue - 1;

                    //if node has been already waiting processing in open list then change order
                    if (endNode.ProcessStatus == Node.NodeProcessStatus.InOpenList)
                    {
                        nodesToProcess.Remove(endNode);
                    }

                    if (endNode.ProcessValue > 0)
                    {
                        int indexToInsert = nodesToProcess.FindLastIndex(delegate (Node node)
                                                    {
                                                        return node.ProcessValue >= endNode.ProcessValue;
                                                    });
                        nodesToProcess.Insert(indexToInsert, endNode);
                        endNode.ProcessStatus = Node.NodeProcessStatus.InOpenList;
                    }
                    else
                    {
                        endNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
                    }
                }
            }
            nodesToProcess.Remove(currentNode);
            currentNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
        }

        return map;
    }

    private void PrintMoveMap()
    {
        foreach (var tilePosition in _moveMap)
        {
            Node node = _graph.NodeGraph[_graph.CreateNodeKeyFromCoordinates(tilePosition.x, tilePosition.y)];
            if (node.GameStatus == Node.TileGameStatus.Empty)
                Movemap.SetTile(tilePosition, MoveTile);
            else if (node.GameStatus == Node.TileGameStatus.Enemy)
                Movemap.SetTile(tilePosition, EnemyTile);
            else if (node.GameStatus == Node.TileGameStatus.Ally)
                Movemap.SetTile(tilePosition, AllyTile);
        }
    }

    public void PrintCharacterMoveMap(Character character)
    {
        Node.TileGameStatus fraction;
        fraction = GetTileStatusFromCharacter(character);
        _moveMap = GetMoveMap(fraction, character.Length, character.Coords);
        PrintMoveMap();
    }

    public bool IsMovementEnable(Vector3Int targetPosition)
    {
        if (_moveMap.IndexOf(targetPosition) != -1)
            return true;
        return false;
    }

    public void ResetMovemap()
    {
        _moveMap.Clear();
        Movemap.ClearAllTiles();
        _graph.RestoreProcessStatus();
    }


    /*
     * Graph section
     */
    public void InitializeGraph()
    {
        _graph = new PathfindingGraph();
        CurrentTilemap.CompressBounds();
        foreach(Vector3Int pos in CurrentTilemap.cellBounds.allPositionsWithin)
        {
            BattleTile tile = CurrentTilemap.GetTile<BattleTile>(pos);
            if (tile != null)
            {
                Node centralTileNode;
                string nodeKey = _graph.CreateNodeKeyFromCoordinates(pos.x, pos.y);
                if (_graph.NodeGraph.ContainsKey(nodeKey))
                {
                    centralTileNode = _graph.NodeGraph[nodeKey];
                }
                else
                {
                    centralTileNode = new Node();
                    centralTileNode.Coords = pos;
                    centralTileNode.ProcessStatus = Node.NodeProcessStatus.NotVisited;
                    if (tile.IsBlocked)
                        centralTileNode.GameStatus = Node.TileGameStatus.Block;
                    else
                        centralTileNode.GameStatus = Node.TileGameStatus.Empty;
                    _graph.NodeGraph.Add(nodeKey, centralTileNode);
                }

                //adding connections
                if (centralTileNode.GameStatus == Node.TileGameStatus.Empty)
                {
                    Vector3Int[] offsets = { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0) };
                    for (int i = 0; i < 4; ++i)
                    {
                        Vector3Int currentTileLocation = pos + offsets[i];
                        BattleTile offsetTile = CurrentTilemap.GetTile<BattleTile>(currentTileLocation);
                        if (offsetTile != null)
                        {
                            Node offsetTileNode;
                            string offsetNodeKey = _graph.CreateNodeKeyFromCoordinates(currentTileLocation.x, currentTileLocation.y);
                            if (_graph.NodeGraph.ContainsKey(offsetNodeKey))
                            {
                                offsetTileNode = _graph.NodeGraph[offsetNodeKey];
                            }
                            else
                            {
                                offsetTileNode = new Node();
                                offsetTileNode.Coords = currentTileLocation;
                                offsetTileNode.ProcessStatus = Node.NodeProcessStatus.NotVisited;
                                if (offsetTile.IsBlocked)
                                    offsetTileNode.GameStatus = Node.TileGameStatus.Block;
                                else
                                    offsetTileNode.GameStatus = Node.TileGameStatus.Empty;
                                _graph.NodeGraph.Add(offsetNodeKey, offsetTileNode);
                            }

                            if (offsetTileNode.GameStatus == Node.TileGameStatus.Empty)
                            {
                                centralTileNode.AddConnection(1f, offsetTileNode);
                            }
                        }
                    }
                }
            }
        }
    }


    /*
     * Tilemap section
     */

    public void PrintTileInfo(Vector3Int cellPosition)
    {
        BattleTile tile = CurrentTilemap.GetTile(cellPosition) as BattleTile;
        if (tile != null)
        {
            Debug.Log($"Tile at position ({cellPosition.x}, {cellPosition.y}) exists\n Is blocked: {tile.IsBlocked}");
            Node node = _graph.GetNode(cellPosition);
            Debug.Log($"Connections count: {node.Connections.Count}");
            if (node.ProcessStatus == Node.NodeProcessStatus.InClosedList)
                Debug.Log($"In closed list");
            else if (node.ProcessStatus == Node.NodeProcessStatus.InOpenList)
                Debug.Log($"In open list");
            else if (node.ProcessStatus == Node.NodeProcessStatus.NotVisited)
                Debug.Log($"Not visited");
        }
        else
            Debug.Log($"Tile at position ({cellPosition.x}, {cellPosition.y}) does not exist");
    }

    public Vector3Int GetTilemapCoordsFromWorld(Tilemap tilemap, Vector3 worldCoords)
    {
        return tilemap.WorldToCell(worldCoords);
    }

    public Vector3Int GetTilemapCoordsFromScreen(Tilemap tilemap, Vector3 screenCoords)
    {
        Ray ray = CurrentCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPosition = ray.GetPoint(-ray.origin.z / ray.direction.z);
        return tilemap.WorldToCell(worldPosition);
    }
    


    //Uses for initial character registration 
    public void DefineCharacter(Character character)
    {
        character.Coords = GetTilemapCoordsFromWorld(CurrentTilemap, character.transform.position);
        AddCharacterToNode(character.Coords, character);
    }

    public void DefineEffect(EffectTile effect)
    {
        Vector3Int coords = GetTilemapCoordsFromWorld(CurrentTilemap, effect.gameObject.transform.position);
        AddEffectToNode(coords, effect);
    }

    public void RemoveEffect(EffectTile effect)
    {
        Vector3Int coords = GetTilemapCoordsFromWorld(CurrentTilemap, effect.transform.position);
        RemoveEffectFromNode(coords, effect);
    }

    //Now one effect rewrite another
    public bool AddEffectToNode(Vector3Int coords, EffectTile effect)
    {
        Node node = _graph.GetNode(coords);
        //if tile already has effect when find it and rewrite
        if (node.HasEffect)
        {
            foreach (var gameobject in node.ObjectsOnTile)
            {
                EffectTile oldEffect;
                bool ok = gameobject.TryGetComponent<EffectTile>(out oldEffect);
                if (ok)
                {
                    node.ObjectsOnTile.Remove(oldEffect.gameObject);
                    oldEffect.EndEffect();
                }
            }
        }

        node.ObjectsOnTile.Add(effect.gameObject);
        node.HasEffect = true;
        effect.StartEffect();
        return true;
    }

    public bool RemoveEffectFromNode(Vector3Int coords, EffectTile effect)
    {
        Node node = _graph.GetNode(coords);
        if (node.HasEffect)
        {
            foreach (var gameobject in node.ObjectsOnTile)
            {
                EffectTile oldEffect;
                bool ok = gameobject.TryGetComponent<EffectTile>(out oldEffect);
                if (ok && oldEffect == effect)
                {
                    node.ObjectsOnTile.Remove(oldEffect.gameObject);
                    oldEffect.EndEffect();
                }
            }
            node.HasEffect = false;
            return true;
        }
        return false;
    }

    //Now only one character can be on tile
    public bool AddCharacterToNode(Vector3Int coords, Character character)
    {
        Node node = _graph.GetNode(coords);
        if (node.GameStatus == Node.TileGameStatus.Empty)
        {
            node.GameStatus = GetTileStatusFromCharacter(character);
            node.ObjectsOnTile.Add(character.gameObject);
        }
        return false;
    }

    public bool RemoveCharacterFromNode(Vector3Int coords, Character character)
    {
        Node node = _graph.GetNode(coords);
        if (node.GameStatus == Node.TileGameStatus.Ally || node.GameStatus == Node.TileGameStatus.Enemy)
        {
            foreach(var gameobject in node.ObjectsOnTile)
            {
                Character oldCharacter;
                bool ok = gameObject.TryGetComponent<Character>(out oldCharacter);
                if (ok)
                {
                    node.ObjectsOnTile.Remove(gameObject);
                    node.GameStatus = Node.TileGameStatus.Empty;
                    return true;
                }
            }
        }
        return false;
    }

    public void SetTileGameplayStatus(Vector3Int coords, Node.TileGameStatus status)
    {
        _graph.GetNode(coords).GameStatus = status;
    }


    /*
     * Pathfining section
     */
    private float _basicCost = 10;

    public float Heuristic(Node start, Node end)
    {
        float x = (start.Coords.x - end.Coords.x) * _basicCost;
        float y = (start.Coords.y - end.Coords.y) * _basicCost;

        return Mathf.Sqrt(x * x + y * y);
    }

    public List<Node> AStarPathfinding(Node start, Node end, Character character)
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
            foreach (var node in openList)
            {
                if (node.EstimatedCost < currentNode.EstimatedCost)
                    currentNode = node;
            }

            //if reach goal - stop searching and start build path
            if (currentNode == end)
                break;

            foreach (var connection in currentNode.Connections)
            {
                Node endNode = connection.EndNode;
                //calculate the cost of movement
                //this cost depend on tile and character properties
                float cost = currentNode.CostSoFar + _basicCost;
                if (endNode.HasEffect)
                {
                    EffectTile effect = endNode.GetEffect();
                    if (effect.Type == EffectType.Damage)
                    {
                        /*
                         * maxHealth - damage
                         * > 80% = +2
                         * 50-80% = +4
                         * 0-50% = +6
                         */
                        float hpPercent = (character.CurHealth - effect.Value) / character.Health;
                        if (hpPercent > 0.8f)
                            cost += 2;
                        else if (hpPercent < 0.5f)
                            cost += 6;
                        else
                            cost += 4;
                    }
                    else if (effect.Type == EffectType.Heal)
                    {
                        /*
                         * cost reduce based on current character's health
                         * 100% = 0
                         * >80% = -2
                         * 50-80% = -4
                         * <50% = -6
                         */
                        if (character.CurHealth != character.Health)
                        {
                            float hpPercent = character.CurHealth / character.Health;
                            if (hpPercent > 0.8f)
                                cost -= 2;
                            else if (hpPercent < 0.5f)
                                cost -= 6;
                            else
                                cost -= 4;
                        }
                    }
                }

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



    public List<Node> BuildPath(Vector3Int start, Vector3Int end, Character character)
    {
        Node startNode = _graph.GetNode(start);
        Node endNode = _graph.GetNode(end);

        return AStarPathfinding(startNode, endNode, character);
    }

    public void PrintPath(List<Node> path)
    {
        for (int i = 0; i < path.Count; ++i)
        {
            Movemap.SetTile(path[i].Coords, PathTile);
        }
    }

    public List<Vector3Int> ConvertFromGraphPath(List<Node> path)
    {
        List<Vector3Int> coordPath = new List<Vector3Int>();
        for (int i = 0; i < path.Count; ++i)
            coordPath.Add(path[i].Coords);
        return coordPath;
    }


    public Node.TileGameStatus GetTileStatusFromCharacter(Character character)
    {
        if (character.gameObject.CompareTag("Player"))
            return Node.TileGameStatus.Ally;
        else if (character.gameObject.CompareTag("Enemy"))
            return Node.TileGameStatus.Enemy;
        else
            return Node.TileGameStatus.Empty;
    }
}
