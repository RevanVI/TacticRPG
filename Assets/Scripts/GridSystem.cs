
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

    public void DefineAvailableMeleeTargets(Movemap movemap, List<Character> characterList, Node.TileGameStatus characterFraction, int attackDistance)
    {
        foreach(var character in characterList)
        {
            Vector3Int offsetCoords = character.Coords;
            //check all directions
            for (int i = 0; i < 4; ++i)
            {
                offsetCoords = character.Coords + attackDistance * offsets[i];

                //if tile belongs to movemap and empty than character can attack target from it
                if (movemap.MoveCoords.IndexOf(offsetCoords) != -1)
                {
                    movemap.EnemyMeleeCoords.Add(character.Coords);
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
        foreach(var tilePosition in movemap.EnemyMeleeCoords)
            Movemap.SetTile(tilePosition, EnemyTile);
        foreach (var tilePosition in movemap.RangeCoords)
            Movemap.SetTile(tilePosition, EnemyTile);
    }

    public void PrintCharacterMoveMap(Character character, List<Character> characterList, int attackDistance)
    {
        Node.TileGameStatus fraction;
        fraction = GetTileStatusFromCharacter(character);
        _movemap = BuildMovemap(fraction, character.Properties.Speed, character.Coords);
        DefineAvailableMeleeTargets(_movemap, characterList, fraction, attackDistance);
        PrintMoveMap(_movemap, fraction);
    }

    public bool IsMovementEnable(Vector3Int targetPosition)
    {
        return _movemap.IsMovementEnable(targetPosition);
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
                    centralTileNode.Influences = new List<KeyValuePair<int, Node.InfluenceStatus>>();
                    _graph.NodeGraph.Add(nodeKey, centralTileNode);
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
                                offsetTileNode.Influences = new List<KeyValuePair<int, Node.InfluenceStatus>>();
                                _graph.NodeGraph.Add(offsetNodeKey, offsetTileNode);
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
        AddEffectToNode(coords, effect);
    }

    public void RemoveEffect(EffectTile effect)
    {
        Vector3Int coords = GetTilemapCoordsFromWorld(PathfindingMap, effect.transform.position);
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
                bool ok = gameobject.TryGetComponent<Character>(out oldCharacter);
                if (ok && oldCharacter == character)
                {
                    node.ObjectsOnTile.Remove(character.gameObject);
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

    public Path AStarPathfinding(Node start, Node end, Character character)
    {
        start.CostSoFar = 0;
        start.EstimatedCost = Heuristic(start, end);

        int steps = character.Properties.Speed;

        List<KeyValuePair<Node, int>> openList = new List<KeyValuePair<Node, int>>();
        openList.Add(new KeyValuePair<Node, int>(start, steps));

        //Node currentNode = null;
        KeyValuePair<Node, int> currentPair;
        while (openList.Count != 0)
        {
            currentPair = openList[0];

            //find node with smallest estimated cost in open list
            foreach (var nodePair in openList)
            {
                if (nodePair.Key.EstimatedCost < currentPair.Key.EstimatedCost)
                    currentPair = nodePair;
            }

            //if found node is goal node then stop searching and start build path
            if (currentPair.Key == end)
                break;
            //if there is not the last possible step then analyse near tiles
            if (currentPair.Value > 0)
            { 
                foreach (var connection in currentPair.Key.Connections)
                {
                    Node endNode = connection.EndNode;

                    //Characters cant go through another characters
                    //So we need to ignore nodes that taken by ally or enemy (if this is not goal node)
                    //also we need ignore tiles that beyond character's move range
                    if ((endNode.GameStatus == Node.TileGameStatus.Ally || endNode.GameStatus == Node.TileGameStatus.Enemy) && endNode != end)
                        continue;

                    //calculate the cost of movement
                    //this cost depend on tile and character properties
                    float cost = currentPair.Key.CostSoFar + _basicCost;
                    if (endNode.HasEffect)
                    {
                        EffectTile effect = endNode.GetEffect();
                        if (effect.Type == EffectType.Damage)
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
                        else if (effect.Type == EffectType.Heal)
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

                    float heuristic;
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
                        openList.Add(new KeyValuePair<Node, int>(endNode, currentPair.Value - 1));
                    }
                }
            }

            openList.Remove(currentPair);
            currentPair.Key.ProcessStatus = Node.NodeProcessStatus.InClosedList;
        }

        //if goal node wasn't found
        if (currentPair.Key != end)
            return null;
        //else build path
        Path path = new Path();
        Node currentNode = currentPair.Key;

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

    public Node.TileGameStatus GetTileStatusFromCharacter(Character character)
    {
        if (character.gameObject.CompareTag("Ally"))
            return Node.TileGameStatus.Ally;
        else if (character.gameObject.CompareTag("Enemy"))
            return Node.TileGameStatus.Enemy;
        else
            return Node.TileGameStatus.Empty;
    }

    public Character GetCharacterFromCoords(Vector3Int coords)
    {
        Node node = _graph.GetNode(coords);
        if (node != null)
            return node.GetCharacter();
        return null;
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

        Node.TileGameStatus fraction = GetTileStatusFromCharacter(character);
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
