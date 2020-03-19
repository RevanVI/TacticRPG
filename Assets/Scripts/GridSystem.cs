
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;
    public Tilemap CurrentTilemap;
    public Tilemap Movemap;
    public Camera CurrentCamera;
    public Tile MoveTile;

    PathfindingGraph _graph;

    //public RoadTile TakenTile;
    //public RoadTile CommonTile;

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
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int cellPosition = GetTilemapCoordsFromScreen(GridSystem.Instance.CurrentTilemap, Input.mousePosition);
            PrintTileInfo(cellPosition);
        }
    }

    /*
    public List<Vector3Int> GetMoveMap(int moveDistance, Vector3Int position)
    {
        List<Vector3Int> map = new List<Vector3Int>();
        Vector3Int curPosition = position;

        for (int rotation = 0; rotation < 4; ++rotation)
        {
            Vector3Int moveVector;
            TileBase tile;
            if (rotation == 0)
                moveVector = new Vector3Int(0, 1, 0);
            else if (rotation == 1)
                moveVector = new Vector3Int(1, 0, 0);
            else if (rotation == 2)
                moveVector = new Vector3Int(0, -1, 0);
            else
                moveVector = new Vector3Int(-1, 0, 0);
            for (int step = 1; step <= moveDistance; ++step)
            {
                curPosition += moveVector;
                tile = CurrentTilemap.GetTile(curPosition);
                if (tile is RoadTile)
                {
                    RoadTile roadTile = tile as RoadTile;
                    if (roadTile.isBlock)
                        break;
                    if (roadTile.isTaken)
                        break;
                }
                map.Add(curPosition);
            }
            curPosition = position;
        }
        return map;
    }

    private void PrintMoveMap(int moveDistance, Vector3Int position)
    {
        List<Vector3Int> mapToPaint = GetMoveMap(moveDistance, position);
        foreach (var tilePosition in mapToPaint)
        {
            Movemap.SetTile(tilePosition, MoveTile);
        }
    }
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
                                if (tile.IsBlocked)
                                    offsetTileNode.GameStatus = Node.TileGameStatus.Block;
                                else
                                    offsetTileNode.GameStatus = Node.TileGameStatus.Empty;
                                _graph.NodeGraph.Add(offsetNodeKey, offsetTileNode);
                            }

                            if (offsetTileNode.GameStatus == Node.TileGameStatus.Empty)
                            {
                                centralTileNode.AddConnection(1f, centralTileNode, offsetTileNode);
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
    
    /*
    public void PrintCharacterMoveMap(Character character)
    {
        _moveMap = GetMoveMap(character.Length, character.Coords);
        PrintMoveMap(character.Length, character.Coords);
    }
    */

    public bool IsMovementEnable(Vector3Int targetPosition)
    {
        if (_moveMap.IndexOf(targetPosition) != -1)
            return true;
        return false;
    }
    /*
    public void TakeTile(Vector3Int coords)
    {
        CurrentTilemap.SetTile(coords, TakenTile);
    }

    public void ReleaseTile(Vector3Int coords)
    {
        CurrentTilemap.SetTile(coords, CommonTile);
    }
    */
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
