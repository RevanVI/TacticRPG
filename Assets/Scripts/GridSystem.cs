
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;
    public Camera CurrentCamera;
    public Tilemap PathfindingMap;

    private PathfindingGraph _graph;

    public Tilemap Movemap;
    public Tile MoveTile;
    public Tile PathTile;
    public Tile AllyTile;
    public Tile EnemyTile;
    public Tile PositionTile;

    private Movemap _movemap;

    public Vector3Int[] offsets = { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0) };

    private void Awake()
    {
        if (Instance == null)
        { 
            Instance = this;
        }
        InitializeGraph();
        _graph.RestoreProcessStatus();
    }

    private void Start()
    {
        /*
        Debug.Log($"Tilemap data:\n ");
        Debug.Log($"Bounds: ({PathfindingMap.cellBounds.x}, {PathfindingMap.cellBounds.x})\n");
        Debug.Log($"Origin: ({PathfindingMap.origin.x}, {PathfindingMap.origin.x})\n");
        Debug.Log($"Size : {PathfindingMap.size})");
        */
    }

    private void Update()
    {
       
    }

    /*
     * Movemap section
     */

    public Movemap BuildMovemap(Node.TileGameStatus fraction, int moveDistance, Vector3Int position)
    {
        Movemap map = new Movemap();
        List<Node> nodesToProcess = new List<Node>();

        Node currentNode = _graph.GetNode(position);
        if (currentNode.GameStatus == Node.TileGameStatus.Block)
            return map;
        currentNode.ProcessValue = moveDistance;
        currentNode.ProcessStatus = Node.NodeProcessStatus.InOpenList;
        nodesToProcess.Add(currentNode);
        map.MoveCoords.Add(currentNode.Coords);

        while (nodesToProcess.Count != 0)
        {
            currentNode = nodesToProcess[0];
            foreach (var connection in currentNode.Connections)
            {
                Node endNode = connection.EndNode;

                if ((endNode.ProcessStatus == Node.NodeProcessStatus.NotVisited ||
                    endNode.ProcessValue < currentNode.ProcessValue - 1) && 
                    endNode.GameStatus == Node.TileGameStatus.Empty)
                {
                    //first entry in open list and not taken by characters
                    if (endNode.ProcessStatus == Node.NodeProcessStatus.NotVisited)
                        map.MoveCoords.Add(endNode.Coords);

                    endNode.ProcessValue = currentNode.ProcessValue - 1;

                    //if node has been already waiting processing in open list then change order
                    if (endNode.ProcessStatus == Node.NodeProcessStatus.InOpenList)
                        nodesToProcess.Remove(endNode);

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
                        endNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
                }
            }
            nodesToProcess.Remove(currentNode);
            currentNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
        }
        _graph.RestoreProcessStatus();
        return map;
    }

    public void DefineAvailableMeleeTargets(Movemap movemap, Character currentCharacter, List<Character> characterList, List<Node.TileGameStatus> targetFractions, int attackDistance)
    {
        foreach(var character in characterList)
        {
            if (character.Properties.CurrentHealth <= 0 ||
                character == currentCharacter ||
                !targetFractions.Contains(Node.GetTileStatusFromCharacter(character)) )
                continue;

            Vector3Int offsetCoords = character.Coords;
            //check all directions
            for (int i = 0; i < 4; ++i)
            {
                offsetCoords = character.Coords + attackDistance * offsets[i];

                //if tile belongs to movemap and empty than character can attack target from it
                if (movemap.MoveCoords.IndexOf(offsetCoords) != -1)
                {
                    movemap.MeleeCoords.Add(character.Coords);
                    break;
                }
            }
        }
    }

    public List<Vector3Int> DefinePositionsToAttackTarget(Movemap movemap, Character target, int attackDistance)
    {
        Vector3Int offsetCoords = target.Coords;
        List<Vector3Int> positions = new List<Vector3Int>();
        //check all directions
        for (int i = 0; i < 4; ++i)
        {
            offsetCoords = target.Coords + attackDistance * offsets[i];

            //if tile belongs to movemap and empty than character can attack from it
            if (movemap.MoveCoords.IndexOf(offsetCoords) != -1)
                positions.Add(offsetCoords);
        }
        return positions;
    }

    public void PrintMoveMap(Movemap movemap, Node.TileGameStatus characterFraction)
    {
        foreach (var tilePosition in movemap.MoveCoords)
            Movemap.SetTile(tilePosition, MoveTile);
        foreach (var tilePosition in movemap.MeleeCoords)
        {
            if (GetNode(tilePosition).GameStatus == characterFraction)
                Movemap.SetTile(tilePosition, AllyTile);
            else
                Movemap.SetTile(tilePosition, EnemyTile);
        }
        foreach (var tilePosition in movemap.RangeCoords)
        {
            if (GetNode(tilePosition).GameStatus == characterFraction)
                Movemap.SetTile(tilePosition, AllyTile);
            else
                Movemap.SetTile(tilePosition, EnemyTile);
        }
    }

    public void PrintCharacterMoveMap(Character character, List<Character> characterList, int attackDistance)
    {
        Node.TileGameStatus fraction;
        fraction = Node.GetTileStatusFromCharacter(character);
        List<Node.TileGameStatus> fractionList = new List<Node.TileGameStatus>();
        if (fraction == Node.TileGameStatus.Ally)
            fractionList.Add(Node.TileGameStatus.Enemy);
        else
            fractionList.Add(Node.TileGameStatus.Ally);
        _movemap = BuildMovemap(fraction, character.Properties.Speed, character.Coords);
        DefineAvailableMeleeTargets(_movemap, character, characterList, fractionList, attackDistance);
        PrintMoveMap(_movemap, fraction);
    }

    public void ResetMovemap()
    {
        _movemap.Clear();
        Movemap.ClearAllTiles();
    }

    public void PrintMovemapTiles(List<Vector3Int> coords, Tile printTile)
    {
        foreach(var coord in coords)
        {
            Movemap.SetTile(coord, printTile);
        }
    }

    public List<Vector3Int> GetNearMovemapTilesList(Vector3Int coords)
    {
        List<Vector3Int> nearTiles = new List<Vector3Int>();
        for (int i = 0; i < 4; ++i)
        {
            Vector3Int offsetTile = coords + offsets[i];
            if (Movemap.GetTile(offsetTile) != null)
                nearTiles.Add(offsetTile);
        }
        return nearTiles;
    }

    public Movemap GetCurrentMovemap()
    {
        return _movemap;
    }

    public void SetMovemap(Movemap movemap)
    {
        _movemap = movemap;
    }
/*
 * Graph section
 */
    public void InitializeGraph()
    {
        _graph = new PathfindingGraph();
        PathfindingMap.CompressBounds();

        //analyze pathfinding map and build pathfinding graph
        foreach(Vector3Int pos in PathfindingMap.cellBounds.allPositionsWithin)
        {
            BattleTile tile = PathfindingMap.GetTile<BattleTile>(pos);
            if (tile != null)
            {
                Node centralTileNode = _graph.GetNode(pos);
                if (centralTileNode == null)
                {
                    centralTileNode = new Node();
                    centralTileNode.Coords = pos;
                    centralTileNode.ProcessStatus = Node.NodeProcessStatus.NotVisited;
                    if (tile.IsBlocked)
                        centralTileNode.GameStatus = Node.TileGameStatus.Block;
                    else
                        centralTileNode.GameStatus = Node.TileGameStatus.Empty;
                    centralTileNode.Influences = new List<KeyValuePair<int, Node.InfluenceStatus>>();
                    _graph.AddNode(centralTileNode);
                }

                //adding connections
                if (centralTileNode.GameStatus == Node.TileGameStatus.Empty)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        Vector3Int currentTileLocation = pos + offsets[i];
                        BattleTile offsetTile = PathfindingMap.GetTile<BattleTile>(currentTileLocation);
                        if (offsetTile != null)
                        {
                            Node offsetTileNode = _graph.GetNode(currentTileLocation);
                            if (offsetTileNode == null)
                            {
                                offsetTileNode = new Node();
                                offsetTileNode.Coords = currentTileLocation;
                                offsetTileNode.ProcessStatus = Node.NodeProcessStatus.NotVisited;
                                if (offsetTile.IsBlocked)
                                    offsetTileNode.GameStatus = Node.TileGameStatus.Block;
                                else
                                    offsetTileNode.GameStatus = Node.TileGameStatus.Empty;
                                offsetTileNode.Influences = new List<KeyValuePair<int, Node.InfluenceStatus>>();
                                _graph.AddNode(offsetTileNode);
                            }
                            if (offsetTileNode.GameStatus == Node.TileGameStatus.Empty)
                            {
                                centralTileNode.AddConnection(offsetTileNode);
                            }
                        }
                    }
                }
            }
        }
    }

    public Node GetNode(Vector3Int coords)
    {
        return _graph.GetNode(coords);
    }

    /*
     * Tilemap section
     */

    public void PrintTileInfo(Vector3Int cellPosition)
    {
        BattleTile tile = PathfindingMap.GetTile(cellPosition) as BattleTile;
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
        Ray ray = CurrentCamera.ScreenPointToRay(screenCoords);
        Vector3 worldPosition = ray.GetPoint(-ray.origin.z / ray.direction.z);
        return tilemap.WorldToCell(worldPosition);
    }

    public Vector3 GetWorldCoordsFromTilemap(Tilemap tilemap, Vector3Int tilemapCoords)
    {
        return tilemap.CellToWorld(tilemapCoords);
    }

    public Vector2 GetRelativePointPositionInTile(Tilemap tilemap, Vector3Int cellCoords, Vector3 pointCoords)
    {
        Vector3 cellWorldCenter = tilemap.GetCellCenterWorld(cellCoords);
        Vector3 cellSize = tilemap.cellSize;
        //offset center to left bottom corner of tile
        cellWorldCenter -= cellSize / 2;

        float x = (pointCoords.x - cellWorldCenter.x) / cellSize.x;
        float y = (pointCoords.y - cellWorldCenter.y) / cellSize.y;
        return new Vector2(x, y);
    }

    //Uses for initial character registration 
    public void DefineCharacter(Character character)
    {
        character.Coords = GetTilemapCoordsFromWorld(PathfindingMap, character.transform.position);
        AddCharacterToNode(character.Coords, character);
    }

    public void DefineEffect(EffectTile effect)
    {
        Vector3Int coords = GetTilemapCoordsFromWorld(PathfindingMap, effect.gameObject.transform.position);
        _graph.GetNode(coords).AddEffect(effect);
    }

    public void RemoveEffect(EffectTile effect)
    {
        Vector3Int coords = GetTilemapCoordsFromWorld(PathfindingMap, effect.transform.position);
        _graph.GetNode(coords).RemoveEffect(effect);
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
        float x = Mathf.Abs(start.Coords.x - end.Coords.x);
        float y = Mathf.Abs(start.Coords.y - end.Coords.y);

        return (x + y) * _basicCost;
    }

    public Path AStarPathfinding(Node start, Node end, Character character)
    {
        start.CostSoFar = 0;
        start.EstimatedCost = Heuristic(start, end);

        int steps = character.GetSteps();

        List<Node> openList = new List<Node>();
        openList.Add(start);
        openList[0].ProcessValue = steps;

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

            //if found node is goal node then stop searching and start build path
            if (currentNode == end)
                break;
            //if there is not the last possible step then analyse near tiles
            if (currentNode.ProcessValue > 0)
            { 
                foreach (var connection in currentNode.Connections)
                {
                    Node endNode = connection.EndNode;

                    //Characters cant go through another characters
                    //So we need to ignore nodes that taken by ally or enemy (if this is not goal node)
                    //also we need ignore tiles that beyond character's move range
                    if ((endNode.GameStatus == Node.TileGameStatus.Ally || endNode.GameStatus == Node.TileGameStatus.Enemy) && endNode != end)
                        continue;

                    //calculate the cost of movement
                    //this cost depend on tile and character properties
                    float cost = CalculateCost(currentNode, endNode, character);
                    float heuristic;
                    if ((endNode.ProcessStatus == Node.NodeProcessStatus.InClosedList ||
                         endNode.ProcessStatus == Node.NodeProcessStatus.InOpenList) &&
                        endNode.CostSoFar < cost)
                    {
                        continue;
                    }
                    else
                        heuristic = Heuristic(endNode, end);

                    endNode.connection = connection;
                    endNode.CostSoFar = cost;
                    endNode.EstimatedCost = cost + heuristic;
                    if (endNode.ProcessStatus != Node.NodeProcessStatus.InOpenList)
                    {
                        endNode.ProcessStatus = Node.NodeProcessStatus.InOpenList;
                        endNode.ProcessValue = currentNode.ProcessValue - 1;
                        openList.Add(endNode);
                    }
                }
            }
            openList.Remove(currentNode);
            currentNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
        }

        //if goal node wasn't found
        if (currentNode != end)
            return null;
        //else build path
        Path path = new Path();
        while (currentNode != start)
        {
            path.NodePath.Add(currentNode);
            if (currentNode.HasEffect)
                path.IsEffectOnPath = true;
            currentNode = currentNode.connection.StartNode;

        }
        path.NodePath.Reverse();
        return path;
    }

    public float CalculateCost(Node currentNode, Node targetNode, Character character)
    {
        float cost = currentNode.CostSoFar + _basicCost;
        if (targetNode.HasEffect)
        {
            EffectTile effect = targetNode.GetEffect();
            if (effect.Type == EffectTileType.Damage)
            {
                /*
                 * maxHealth - damage
                 * > 80% = +30
                 * 50-80% = +50
                 * 0-50% = +100
                 */
                float hpPercent = (character.Properties.CurrentHealth - effect.Value) / character.Properties.Health;
                if (hpPercent > 0.8f)
                    cost += 30;
                else if (hpPercent < 0.5f)
                    cost += 50;
                else
                    cost += 100;
            }
            else if (effect.Type == EffectTileType.Heal)
            {
                /*
                 * cost reduce based on current character's health
                 * 100% = 0
                 * >80% = -2
                 * 50-80% = -4
                 * <50% = -6
                 */
                if (character.Properties.CurrentHealth != character.Properties.Health)
                {
                    float hpPercent = character.Properties.CurrentHealth / character.Properties.Health;
                    if (hpPercent > 0.8f)
                        cost -= 2;
                    else if (hpPercent < 0.5f)
                        cost -= 6;
                    else
                        cost -= 4;
                }
            }
        }
        return cost;
    }

    public Path BuildPath(Vector3Int start, Vector3Int end, Character character)
    {
        Node startNode = _graph.GetNode(start);
        Node endNode = _graph.GetNode(end);

        Path path = AStarPathfinding(startNode, endNode, character);
        _graph.RestoreProcessStatus();

        return path;
    }

    public void PrintPath(List<Node> path)
    {
        for (int i = 0; i < path.Count; ++i)
        {
            Movemap.SetTile(path[i].Coords, PathTile);
        }
    }

    public static List<Node.TileGameStatus> ConvertFractionsFromStringToNode(List<string> fractionsStringList)
    {
        List<Node.TileGameStatus> fractionsNodeList = new List<Node.TileGameStatus>();
        foreach(var fraction in fractionsStringList)
        {
            if (fraction == "Ally")
                fractionsNodeList.Add(Node.TileGameStatus.Ally);
            else if (fraction == "Enemy")
                fractionsNodeList.Add(Node.TileGameStatus.Enemy);
        }
        return fractionsNodeList;
    }

    public Character GetCharacterFromCoords(Vector3Int coords)
    {
        Node node = _graph.GetNode(coords);
        if (node != null)
            return node.GetCharacter();
        return null;
    }

    public bool AddCharacterToNode(Vector3Int coords, Character character)
    {
        return _graph.GetNode(coords).AddCharacter(character);
    }

    public bool RemoveCharacterFromNode(Vector3Int coords, Character character)
    {
        return _graph.GetNode(coords).RemoveCharacter(character);
    }

    /*
     * Influnce map section
     */

    //build influence map going through all characters
    //can take much time
    public void UpdateInfluenceMap(List<Character> characters)
    {
        _graph.ClearInfluenceData();
        foreach (var character in characters)
        {
            if (character.Properties.CurrentHealth <= 0)
                continue;
            //Node.TileGameStatus fraction = GetTileStatusFromCharacter(character);
            List<Vector3Int> moveInfluence;
            List<Vector3Int> attackInfluence;
            GetInfluenceMap(character, out moveInfluence, out attackInfluence);

            AddInfluence(character.BattleId, Node.InfluenceStatus.Move, moveInfluence);
            moveInfluence.AddRange(attackInfluence);
            AddInfluence(character.BattleId, Node.InfluenceStatus.MeleeAttack, moveInfluence);

            _graph.RestoreProcessStatus();
        }
    }

    //Build movemap and additional tiles (attack influence) that character can attack
    public void GetInfluenceMap(Character character, out List<Vector3Int> moveInfluence, out List<Vector3Int> attackInfluence)
    {
        moveInfluence = new List<Vector3Int>();
        attackInfluence = new List<Vector3Int>();
        List<Node> nodesToProcess = new List<Node>();

        Node currentNode = _graph.GetNode(character.Coords);
        currentNode.ProcessValue = character.Properties.Speed;
        currentNode.ProcessStatus = Node.NodeProcessStatus.InOpenList;
        nodesToProcess.Add(currentNode);
        moveInfluence.Add(currentNode.Coords);

        Node.TileGameStatus fraction = Node.GetTileStatusFromCharacter(character);
        Node.TileGameStatus oppositeFraction;
        if (fraction == Node.TileGameStatus.Ally)
            oppositeFraction = Node.TileGameStatus.Enemy;
        else
            oppositeFraction = Node.TileGameStatus.Ally;

        while (nodesToProcess.Count != 0)
        {
            currentNode = nodesToProcess[0];
            foreach (var connection in currentNode.Connections)
            {
                Node endNode = connection.EndNode;

                if ((endNode.ProcessStatus == Node.NodeProcessStatus.NotVisited ||
                    endNode.ProcessValue < currentNode.ProcessValue - 1) && endNode.GameStatus != fraction)
                {
                    //first entry in open list and not taken by ally
                    if (endNode.ProcessStatus == Node.NodeProcessStatus.NotVisited)
                        moveInfluence.Add(endNode.Coords);

                    //hit to enemy ends turn
                    if (endNode.GameStatus == oppositeFraction)
                    {
                        endNode.ProcessValue = 0;
                        endNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
                    }
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
                        //check all near tiles and find these that beyond move influence on 1 step
                        for (int i = 0; i < 4; ++i)
                        {
                            Vector3Int offsetPosition = endNode.Coords + offsets[i];
                            Node offsetNode = _graph.GetNode(offsetPosition);
                            if (offsetNode != null &&
                                offsetNode.ProcessStatus == Node.NodeProcessStatus.NotVisited)
                            {
                                attackInfluence.Add(offsetNode.Coords);
                                offsetNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
                                offsetNode.ProcessValue = 0;
                            }
                        }
                        endNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
                    }
                }
            }
            nodesToProcess.Remove(currentNode);
            currentNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
        }
    }

    public void AddInfluence(int characterId, Node.InfluenceStatus influenceStatus, List<Vector3Int> coords)
    {
        foreach(var coord in coords)
        {
            _graph.GetNode(coord).Influences.Add(new KeyValuePair<int, Node.InfluenceStatus>(characterId, influenceStatus));
        }
    }

    public List<KeyValuePair<int, Node.InfluenceStatus>> GetInfluenceData(Vector3Int coords)
    {
        return _graph.GetNode(coords).Influences;
    }
}
