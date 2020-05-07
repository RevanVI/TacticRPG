using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum GameState
{
    Start,
    PlayerTurn,
    EnemyTurn,
    EndBattle
};

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public GridSystem Grid;
    public GameState State;
    private bool _isUpdateStarted;

    public PlayerController PlayerController;
    public List<Character> TurnQueue;
    public TurnPanelController TurnPanelControllerRef;
    public Character _currentCharacter;

    public List<Character> CharacterList;
    public EnemyController[] Enemies;
    public List<Vector3Int> AvailableRangedTargets;
    public List<Vector3Int> AvailableMeleeTargets;


    private bool _isInputBlocked;

    public UnityEvent OnTurnStart;
    public UnityEvent OnTurnEnd;

    public int RoundCount;
    public int TurnCount;

    private int _battleIdCounter;

    public Camera CurrentCamera;
    public PointerHandler IngamePointer;
    /*
     * 0 - not needed
     * 1 - check ally characters
     * 2 - check enemy characters
     */
    private string _isEndgameCheckNeeded = "";


    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        TurnQueue = new List<Character>();
        CharacterList = new List<Character>();

        RoundCount = 0;
        TurnCount = 0;
        _battleIdCounter = 0;

        LogHandler.Initialise();
    }

    // Start is called before the first frame update
    void Start()
    {
        _isInputBlocked = true;
        State = GameState.Start;

        AvailableRangedTargets = new List<Vector3Int>();

        _isUpdateStarted = false;
        StartCoroutine(WaitStartGame());
    }

    // Update is called once per frame
    void Update()
    {
        _isUpdateStarted = true;

        //move pointer
        Ray ray = CurrentCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPosition = ray.GetPoint(-ray.origin.z / ray.direction.z);
        IngamePointer.transform.position = worldPosition;

        //check if cursor point on enemy
        Vector3Int tilePosition = GridSystem.Instance.GetTilemapCoordsFromWorld(GridSystem.Instance.PathfindingMap, worldPosition);
        Character targetCharacter = GridSystem.Instance.GetCharacterFromCoords(tilePosition);

        if (State == GameState.PlayerTurn &&
            targetCharacter != null &&
            targetCharacter.tag == _currentCharacter.GetOppositeFraction())
        {
            //cursor in melee attack
            if (AvailableMeleeTargets.Contains(tilePosition) &&
                (_currentCharacter.Properties.Class == CharacterClass.Warrior ||
                 _currentCharacter.Properties.Class == CharacterClass.Healer ||
                 AvailableRangedTargets.Count == 0 || //enemy nearby or no missiles 
                 !AvailableRangedTargets.Contains(tilePosition)))
            {
                Vector2 relativeMousePosition = GridSystem.Instance.GetRelativePointPositionInTile(GridSystem.Instance.PathfindingMap,
                                                                                                   tilePosition,
                                                                                                   worldPosition);
                List<Vector3Int> nearAvailableTiles = GridSystem.Instance.GetNearMovemapTilesList(tilePosition);

                //movemap doesn't returns tile there character stays, but current character can stay near enemy
                //so need to check this option
                if (IsCharactersStayNear(_currentCharacter, targetCharacter))
                    nearAvailableTiles.Add(_currentCharacter.Coords);

                List<PointerHandler.PointerStatus> directionsList = new List<PointerHandler.PointerStatus>();
                foreach (var nearTileCoords in nearAvailableTiles)
                {
                    Vector3Int offset = tilePosition - nearTileCoords;
                    if (offset.x < 0)
                        directionsList.Add(PointerHandler.PointerStatus.RightAttack);
                    else if (offset.x > 0)
                        directionsList.Add(PointerHandler.PointerStatus.LeftAttack);
                    else if (offset.y < 0)
                        directionsList.Add(PointerHandler.PointerStatus.TopAttack);
                    else
                        directionsList.Add(PointerHandler.PointerStatus.BottomAttack);
                }

                PointerHandler.PointerStatus defaultStatus = directionsList[0];
                float space = 0.25f;

                if (nearAvailableTiles.Count == 1)
                {
                    IngamePointer.SetSprite(directionsList[0]);
                }
                else
                {
                    //check all directions from right to top clockwise
                    if (relativeMousePosition.x > (1 - space) && directionsList.Contains(PointerHandler.PointerStatus.RightAttack))
                        IngamePointer.SetSprite(PointerHandler.PointerStatus.RightAttack);
                    else if (relativeMousePosition.y < space && directionsList.Contains(PointerHandler.PointerStatus.BottomAttack))
                        IngamePointer.SetSprite(PointerHandler.PointerStatus.BottomAttack);
                    else if (relativeMousePosition.x < space && directionsList.Contains(PointerHandler.PointerStatus.LeftAttack))
                        IngamePointer.SetSprite(PointerHandler.PointerStatus.LeftAttack);
                    else if (relativeMousePosition.y > (1 - space) && directionsList.Contains(PointerHandler.PointerStatus.TopAttack))
                        IngamePointer.SetSprite(PointerHandler.PointerStatus.TopAttack);
                    else
                        IngamePointer.SetSprite(defaultStatus);
                }
            }
            //cursor in range attack
            else if ((_currentCharacter.Properties.Class == CharacterClass.Archer ||
                      _currentCharacter.Properties.Class == CharacterClass.Mage) &&
                      AvailableRangedTargets.IndexOf(tilePosition) != -1)
            {
                IngamePointer.SetSprite(PointerHandler.PointerStatus.RangeAttack);
            }
        }
        else
            IngamePointer.SetSprite(PointerHandler.PointerStatus.Normal);


        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (State == GameState.PlayerTurn && !_isInputBlocked)
            {
                //GridSystem.Instance.PrintTileInfo(cellPosition);
                _isInputBlocked = true;

                //in updating pointer we have already calculate all data 
                PointerHandler.PointerStatus pointerStatus = IngamePointer.GetStatus();

                if (pointerStatus == PointerHandler.PointerStatus.RangeAttack)
                {
                    //animations and so on)
                    _currentCharacter.AttackAtRange(targetCharacter);
                }
                else
                {
                    if (GridSystem.Instance.IsMovementEnable(tilePosition))
                    {
                        GridSystem.Instance.ResetMovemap();

                        Vector3Int targetMoveCoords = tilePosition;
                        //if player attacks define tile where character should stay
                        if (pointerStatus != PointerHandler.PointerStatus.Normal)
                        {
                            if (pointerStatus == PointerHandler.PointerStatus.RightAttack)
                                targetMoveCoords += new Vector3Int(1, 0, 0);
                            else if (pointerStatus == PointerHandler.PointerStatus.BottomAttack)
                                targetMoveCoords += new Vector3Int(0, -1, 0);
                            else if (pointerStatus == PointerHandler.PointerStatus.LeftAttack)
                                targetMoveCoords += new Vector3Int(-1, 0, 0);
                            else
                                targetMoveCoords += new Vector3Int(0, 1, 0);
                        }
                        //Build path
                        Path path = GridSystem.Instance.BuildPath(_currentCharacter.Coords, targetMoveCoords, _currentCharacter);
                        if (pointerStatus != PointerHandler.PointerStatus.Normal)
                            path.NodePath.Add(GridSystem.Instance.GetNode(tilePosition));
                        GridSystem.Instance.PrintPath(path.NodePath);
                        List<Vector3Int> coordPath = path.ConvertToCoordPath();

                        //there is two options: this movements is melee attack or not
                        if (targetCharacter == null)
                            _currentCharacter.Move(coordPath);
                        else
                            _currentCharacter.Move(coordPath, targetCharacter);
                    }
                    else
                        _isInputBlocked = false;
                }
            }
        }
    }

    private void DefineTurnQueue()
    {
        foreach(var character in CharacterList)
        {
            if (character.Properties.Health > 0)
            {
                int indexToInsert = TurnQueue.FindLastIndex(delegate (Character otherChar)
                {
                    return otherChar.Properties.Speed >= character.Properties.Speed;
                });
                if (indexToInsert == -1)
                    TurnQueue.Add(character);
                else
                    TurnQueue.Insert(indexToInsert, character);   
            }
        }
        TurnQueue.Add(null);
    }

    private IEnumerator WaitStartGame()
    {
        while (!_isUpdateStarted)
        {
            yield return new WaitForSeconds(0.1f);
        }
        DefineTurnQueue();
        for (int i = 0; i < TurnQueue.Count; ++i)
        {
            if (TurnQueue[i] != null)
            { 
                TurnPanelControllerRef.AddIcon(TurnQueue[i], TurnQueue[i].Properties);
                //on this point character and his icon exist and we can connect it
                //TurnQueue[i].OnDamageTaken.AddListener(OnCharacterTakeDamage);
            }
            //else paste marker
            
        }
        RoundCount = 1;
        StartNextTurn();
    }

    private void StartNextTurn()
    {
        Debug.Log("Next turn");
        _currentCharacter = TurnQueue[0];
        TurnQueue.RemoveAt(0);
        if (_currentCharacter == null) // next round start
        {
            ++RoundCount;
            //set up new marker
            TurnQueue.Add(null);
            //get real next character
            _currentCharacter = TurnQueue[0];
            TurnQueue.RemoveAt(0);
        }
        ++TurnCount;
        OnTurnStart.Invoke();
        if (_currentCharacter.gameObject.CompareTag("Enemy"))
        {
            StartEnemyTurn();
           //StartCoroutine(StartEnemyTurn());
        }
        else
        {
            StartPlayerTurn();
        }
    }

    private void StartPlayerTurn()
    {
        State = GameState.PlayerTurn;
        GridSystem.Instance.PrintCharacterMoveMap(_currentCharacter);

        //GridSystem.Instance.UpdateInfluenceMap(CharacterList);

        DefineAvailableMeleeTargets(_currentCharacter);
        if ((_currentCharacter.Properties.Class == CharacterClass.Archer || 
            _currentCharacter.Properties.Class == CharacterClass.Mage) &&
            !IsThereEnemyNearby(_currentCharacter))
        {
            DefineAvailableRangedTargets(_currentCharacter);
            GridSystem.Instance.PrintMovemapTiles(AvailableRangedTargets, GridSystem.Instance.EnemyTile);
        }
        _isInputBlocked = false;
    }

    private void StartEnemyTurn()
    {
        State = GameState.EnemyTurn;
        GridSystem.Instance.PrintCharacterMoveMap(_currentCharacter);
        //GridSystem.Instance.UpdateInfluenceMap(CharacterList);

        DefineAvailableMeleeTargets(_currentCharacter);
        if ((_currentCharacter.Properties.Class == CharacterClass.Archer ||
            _currentCharacter.Properties.Class == CharacterClass.Mage) &&
            !IsThereEnemyNearby(_currentCharacter))
        {
            DefineAvailableRangedTargets(_currentCharacter);
            GridSystem.Instance.PrintMovemapTiles(AvailableRangedTargets, GridSystem.Instance.EnemyTile);
        }
        ((CharacterAI)_currentCharacter).MakeTurn();
        //yield return new WaitForSeconds(1f);
        //EndTurn();
    }

    public Character FindCharacter(Vector3Int tileCoords)
    {
        foreach (var character in TurnQueue)
        {
            if (character != null && character.Coords == tileCoords)
                return character;
        }
        return null;
    }

    public Character FindCharacter(int battleId)
    {
        foreach (var character in CharacterList)
            if (character.BattleId == battleId)
                return character;
        return null;
    }

    public void RegisterCharacter(Character character)
    {
        if (!CharacterList.Contains(character))
        {
            character.BattleId = _battleIdCounter;
            ++_battleIdCounter;
            CharacterList.Add(character);
            character.OnMoveEnded.AddListener(EndTurn);
            character.OnDamageTaken.AddListener(OnCharacterTakeDamage);
            character.OnDie.AddListener(OnCharacterDie);
        }
    }

    public void EndTurn()
    {
        GridSystem.Instance.ResetMovemap();
        AvailableRangedTargets.Clear();
        TurnQueue.Add(_currentCharacter);
        OnTurnEnd.Invoke();
        bool isGameEnded = false;
        if (_isEndgameCheckNeeded != "")
        {
            isGameEnded = CheckEndgame(_isEndgameCheckNeeded);
        }
        if (!isGameEnded)
        {
            _isEndgameCheckNeeded = "";
            StartNextTurn();
        }
        else
            Debug.Log("Game ended");
    }

    public void GetCurrentCharacterInfo(out int characterBattleId, out CharacterProperties properties)
    {
        characterBattleId = _currentCharacter.BattleId;
        properties = _currentCharacter.Properties;
    }

    public void OnCharacterTakeDamage(int characterBattleId)
    {
        Character character = FindCharacter(characterBattleId);
        TurnPanelControllerRef.UpdateTurnIcon(characterBattleId, character.Properties);
    }

    public void OnCharacterDie(int characterBattleId)
    {
        Character character = FindCharacter(characterBattleId);
        TurnPanelControllerRef.RemoveTurnIcon(characterBattleId);
        TurnQueue.RemoveAll(delegate (Character otherCharacter)
                            {
                                return otherCharacter == character;
                            });
        _isEndgameCheckNeeded = character.tag;
    }

    public void DefineAvailableRangedTargets(Character character)
    {
        AvailableRangedTargets.Clear();

        //character can't shoot when there is enemy nearby
        bool isEnemyNearby = IsThereEnemyNearby(character);
        if (isEnemyNearby || character.Properties.CurrentMissiles == 0)
            return;

        //go through all characters on map and define if they are visible from character's point
        string oppositeFraction = character.GetOppositeFraction();
        LayerMask layerMask = LayerMask.GetMask(oppositeFraction, "MapEdges");

        foreach(var otherCharacter in CharacterList)
        {
            //ignore ally and dead characters
            if (!otherCharacter.CompareTag(character.tag) && otherCharacter.Properties.CurrentHealth >= 0)
            {
                Vector3 direction = (otherCharacter.transform.position - character.transform.position).normalized;
                float distanceBetweenCharacters = (otherCharacter.transform.position - character.transform.position).magnitude;

                RaycastHit2D[] hits2D = Physics2D.RaycastAll(new Vector2(character.transform.position.x, character.transform.position.y),
                                     new Vector2(direction.x, direction.y),
                                     distanceBetweenCharacters,
                                     layerMask);

                //we need to define if edge was hit earlier than target character
                bool found = false;
                foreach(var hit in hits2D)
                {
                    //MapEdges layer no = 8
                    if (hit.transform.gameObject.layer == 8)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    AvailableRangedTargets.Add(otherCharacter.Coords);
            }
        }
    }

    public void DefineAvailableMeleeTargets(Character character)
    {
        AvailableMeleeTargets.Clear();

        Node.TileGameStatus gameStatus;
        if (character.tag == "Ally")
            gameStatus = Node.TileGameStatus.Enemy;
        else
            gameStatus = Node.TileGameStatus.Ally;
        AvailableMeleeTargets = GridSystem.Instance.GetTileCoordsFromMovemap(gameStatus);
    }

    public bool IsThereEnemyNearby(Character character)
    {
        //check all directions
        Vector3Int[] offsets = { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0) };
        int i;
        for (i = 0; i < 4; ++i)
        {
            Character characterNearby = GridSystem.Instance.GetCharacterFromCoords(character.Coords + offsets[i]);
            if (characterNearby != null && !characterNearby.CompareTag(character.tag))
                break;
        }

        if (i < 4)
            return true;
        return false;
    }

    public bool IsCharactersStayNear(Character character1, Character character2)
    {
        if ((character1.Coords - character2.Coords).magnitude == 1)
            return true;
        return false;
    }

    public bool CheckEndgame(string fraction)
    {
        bool isGameEnded = true;
        foreach(var character in TurnQueue)
        {
            if (character != null && character.tag == fraction)
            {
                isGameEnded = false;
                break;
            }
        }
        return isGameEnded;
    }
}
