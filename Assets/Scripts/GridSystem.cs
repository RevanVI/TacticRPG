
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;
    public Camera CurrentCamera;
    public Tilemap CurrentTilemap;

    PathfindingGraph _graph;

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

    public List<Vector3Int> GetMoveMap(Node.TileGameStatus fraction, int moveDistance, Vector3Int position)
    {
        List<Vector3Int> map = new List<Vector3Int>();
        List<Node> nodesToProcess = new List<Node>();

        Node currentNode = _graph.NodeGraph[_graph.CreateNodeKeyFromCoordinates(position.x, position.y)];
        if (currentNode.GameStatus == Node.TileGameStatus.Block)
            return map;
        currentNode.ProcessValue = moveDistance;
        currentNode.ProcessStatus = Node.NodeProcessStatus.InOpenList;
        nodesToProcess.Add(currentNode);
        //not add start tile
        //map.Add(currentNode.Coords);

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

    public void PrintTileInfo(Vector3Int cellPosition)
    {
        BattleTile tile = CurrentTilemap.GetTile(cellPosition) as BattleTile;
        if (tile != null)
        {
            Debug.Log($"Tile at position ({cellPosition.x}, {cellPosition.y}) exists\n Is blocked: {tile.IsBlocked}");
            Node node = _graph.NodeGraph[_graph.CreateNodeKeyFromCoordinates(cellPosition.x, cellPosition.y)];
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
    
    public void PrintCharacterMoveMap(Character character)
    {
        Node.TileGameStatus fraction;
        if (character.CompareTag("Player"))
            fraction = Node.TileGameStatus.Ally;
        else
            fraction = Node.TileGameStatus.Enemy;
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

    //Uses for initial character registration 
    public void DefineCharacterCoords(Character character)
    {
        character.Coords = GetTilemapCoordsFromWorld(CurrentTilemap, character.transform.position);
        Node node = _graph.NodeGraph[_graph.CreateNodeKeyFromCoordinates(character.Coords.x, character.Coords.y)];
        if (character.gameObject.CompareTag("Player"))
            node.GameStatus = Node.TileGameStatus.Ally;
        else if (character.gameObject.CompareTag("Enemy"))
            node.GameStatus = Node.TileGameStatus.Enemy;
    }

    public void DefineEffect(EffectTile effect)
    {
        Vector3Int coords = GetTilemapCoordsFromWorld(CurrentTilemap, effect.gameObject.transform.position);
        Node node = _graph.NodeGraph[_graph.CreateNodeKeyFromCoordinates(coords.x, coords.y)];
        node.AddEffect(effect);
    }

    public void RemoveEffect(EffectTile effect)
    {
        Vector3Int coords = GetTilemapCoordsFromWorld(CurrentTilemap, effect.gameObject.transform.position);
        Node node = _graph.NodeGraph[_graph.CreateNodeKeyFromCoordinates(coords.x, coords.y)];
        node.AddEffect(effect);
    }

    public List<Node> BuildPath(Vector3Int start, Vector3Int end)
    {
        Node startNode = _graph.GetNode(start);
        Node endNode = _graph.GetNode(end);

        return _graph.AStarPathfinding(startNode, endNode);
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

    public void SetTileGameplayStatus(Vector3Int coords, Node.TileGameStatus status)
    {
        _graph.SetNodeGameplayStatus(coords, status);
    }


    /*
    public int IsTileAvailable(Vector3Int coords)
    {
        TileBase tile = CurrentTilemap.GetTile(coords);
        if (tile is RoadTile)
        {
            RoadTile roadTile = tile as RoadTile;
            if (roadTile.isBlock)
                return 1;
            if (roadTile.isTaken)
                return 2;
        }
        return 0;
    }
    */

}
