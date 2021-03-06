﻿using System.Collections;
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
    public SkillPanel SkillPanelRef;
    public Character _currentCharacter;

    public List<Character> CharacterList;
    public EnemyController[] Enemies;
    public List<Vector3Int> AvailableRangedTargets;

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

    //--------------------
    private int _skillNo;
    private Skill _skillData;
    //--------------------------

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

        if (_skillData != null)
            return;
        //check if cursor point on enemy
        Vector3Int tilePosition = GridSystem.Instance.GetTilemapCoordsFromWorld(GridSystem.Instance.PathfindingMap, worldPosition);
        Character targetCharacter = GridSystem.Instance.GetCharacterFromCoords(tilePosition);

        if (State == GameState.PlayerTurn &&
            targetCharacter != null &&
            targetCharacter.tag == _currentCharacter.GetOppositeFraction())
        {
            //cursor in melee attack
            if (GridSystem.Instance.GetCurrentMovemap().MeleeCoords.Contains(tilePosition) &&
                (_currentCharacter.Properties.Class == CharacterClass.Warrior ||
                 _currentCharacter.Properties.Class == CharacterClass.Healer ||
                 AvailableRangedTargets.Count == 0 || //enemy nearby or no missiles 
                 !AvailableRangedTargets.Contains(tilePosition)))
            {
                Vector2 relativeMousePosition = GridSystem.Instance.GetRelativePointPositionInTile(GridSystem.Instance.PathfindingMap,
                                                                                                   tilePosition,
                                                                                                   worldPosition);
                List<Vector3Int> nearAvailableTiles = GridSystem.Instance.GetNearMovemapTilesList(tilePosition);

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

                int directionIndex = -1;
                IngamePointer.DefineSprite(relativeMousePosition, directionsList, out directionIndex);         
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
                    if (GridSystem.Instance.GetCurrentMovemap().IsCoordsInMovemap(tilePosition) && tilePosition != _currentCharacter.Coords)
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
                        GridSystem.Instance.PrintPath(path.NodePath);
                        List<Vector3Int> coordPath = path.ConvertToCoordPath();

                        //there is two options: attack or movement
                        if (targetCharacter == null)
                            _currentCharacter.Move(coordPath);
                        else
                            _currentCharacter.AttackMelee(targetCharacter, coordPath);
                    }
                    else
                        _isInputBlocked = false;
                }
            }
        }
    }

    private void DefineTurnQueue()
    {
        foreach (var character in CharacterList)
        {
            if (character.Properties.Health > 0)
            {
                int indexToInsert = -1;
                for (int i = 0; i < TurnQueue.Count; ++i)
                {
                    if (TurnQueue[i].Properties.Speed < character.Properties.Speed)
                    {
                        indexToInsert = i;
                        break;
                    }
                }
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

        _currentCharacter.ProcessEffects();
        _currentCharacter.ProcessSkills();
        SkillPanelRef.SetSkills(_currentCharacter);
        if (_currentCharacter.IsStunned())
            EndTurn();

        Node.TileGameStatus fraction = Node.GetTileStatusFromCharacter(_currentCharacter);
        List<Node.TileGameStatus> fractionList = new List<Node.TileGameStatus>();
        if (fraction == Node.TileGameStatus.Ally)
            fractionList.Add(Node.TileGameStatus.Enemy);
        else
            fractionList.Add(Node.TileGameStatus.Ally);
        Movemap movemap = GridSystem.Instance.BuildMovemap(fraction, _currentCharacter.Properties.Speed, _currentCharacter.Coords);
        GridSystem.Instance.DefineAvailableMeleeTargets(movemap, _currentCharacter, CharacterList, fractionList, 1);

        if ((_currentCharacter.Properties.Class == CharacterClass.Archer ||
            _currentCharacter.Properties.Class == CharacterClass.Mage) &&
            !IsThereEnemyNearby(_currentCharacter))
        {
            AvailableRangedTargets.Clear();
            AvailableRangedTargets.AddRange(DefineAvailableRangedTargets(_currentCharacter, _currentCharacter.GetOppositeFraction()));
           movemap.RangeCoords.AddRange(AvailableRangedTargets);
        }
        GridSystem.Instance.SetMovemap(movemap);
        GridSystem.Instance.PrintMoveMap(movemap, fraction);
        GridSystem.Instance.UpdateInfluenceMap(CharacterList);

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
        _isInputBlocked = false;
    }

    private void StartEnemyTurn()
    {
        State = GameState.EnemyTurn;
        ((CharacterAI)_currentCharacter).MakeTurn();
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
            character.OnActionsEnded.AddListener(EndTurn);
            character.OnDamageTaken.AddListener(OnCharacterTakeDamage);
            character.OnDie.AddListener(OnCharacterDie);
        }
    }

    public void EndTurn()
    {
        GridSystem.Instance.ResetMovemap();
        AvailableRangedTargets.Clear();
        if (_currentCharacter.Properties.CurrentHealth > 0)
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
        if (_currentCharacter == character)
            EndTurn();
    }

    public List<Vector3Int> DefineAvailableRangedTargets(Character character, string fraction)
    {
        List<Vector3Int> availableTargets = new List<Vector3Int>(); 

        LayerMask layerMask = LayerMask.GetMask(fraction, "MapEdges");

        foreach (var otherCharacter in CharacterList)
        {
            //ignore ally and dead characters
            if (otherCharacter.CompareTag(fraction) && 
                otherCharacter.Properties.CurrentHealth > 0 &&
                otherCharacter != _currentCharacter)
            { 
                Vector3 direction = (otherCharacter.transform.position - character.transform.position).normalized;
                float distanceBetweenCharacters = (otherCharacter.transform.position - character.transform.position).magnitude;

                RaycastHit2D[] hits2D = Physics2D.RaycastAll(new Vector2(character.transform.position.x, character.transform.position.y),
                                     new Vector2(direction.x, direction.y),
                                     distanceBetweenCharacters,
                                     layerMask);

                //we need to define if edge was hit earlier than target character
                bool found = false;
                foreach (var hit in hits2D)
                {
                    //MapEdges layer no = 8
                    if (hit.transform.gameObject.layer == 8)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    availableTargets.Add(otherCharacter.Coords);
            }
        }
        return availableTargets;
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
        foreach (var character in TurnQueue)
        {
            if (character != null && character.tag == fraction)
            {
                isGameEnded = false;
                break;
            }
        }
        return isGameEnded;
    }

    public void SkillUsed(int skillNo)
    {
        _skillNo = skillNo;
        _skillData = _currentCharacter.Skills[skillNo];
        if (_skillData.TypeTarget == Skill.TargetType.Self)
            StartCoroutine(SelfSkillProcess());
        else if (_skillData.TypeUse == Skill.UseType.Melee)
        {
            StartCoroutine(MeleeSkillProcess());
        }
        else if (_skillData.TypeUse == Skill.UseType.Randged)
        {
            StartCoroutine(RangeSkillProcess());
        }
    }

    public IEnumerator MeleeSkillProcess()
    {
        bool end = false;

        //skill can be denied so we need to save the data
        Movemap oldMovemap = GridSystem.Instance.GetCurrentMovemap().Copy();

        //define list of target fractions
        List<string> fractionList = new List<string>();
        if (_skillData.FractionTarget == Skill.TargetFraction.Enemy)
            fractionList.Add(_currentCharacter.GetOppositeFraction());
        else if (_skillData.FractionTarget == Skill.TargetFraction.Ally)
            fractionList.Add(_currentCharacter.tag);
        else
        {
            fractionList.Add(_currentCharacter.GetOppositeFraction());
            fractionList.Add(_currentCharacter.tag);
        }

        GridSystem.Instance.ResetMovemap();
        //define all data
        Movemap skillMovemap = new Movemap();                                                                            
        skillMovemap.MoveCoords.AddRange(oldMovemap.MoveCoords);   
        //define all target that character can reach 
        GridSystem.Instance.DefineAvailableMeleeTargets(skillMovemap, _currentCharacter, CharacterList, GridSystem.ConvertFractionsFromStringToNode(fractionList), _skillData.Distance);

        //define all positions to attack for all targets
        List<List<Vector3Int>> possiblePositions = new List<List<Vector3Int>>();
        for (int i = 0; i < skillMovemap.MeleeCoords.Count; ++i)
        {
            Character targetCharacter = GridSystem.Instance.GetCharacterFromCoords(skillMovemap.MeleeCoords[i]);
            //get possible positions to attack
            possiblePositions.Add(GridSystem.Instance.DefinePositionsToAttackTarget(skillMovemap, targetCharacter, _skillData.Distance));
            //additional special checks
            _skillData.AdditionalChecks(targetCharacter, possiblePositions[i]);
            //if there no possible positions to attack than delete this target from lists
            if (possiblePositions[i].Count == 0)
            {
                skillMovemap.MeleeCoords.RemoveAt(i);
                possiblePositions.RemoveAt(i);
                --i;
            }
        }

        GridSystem.Instance.PrintMoveMap(skillMovemap, Node.GetTileStatusFromCharacter(_currentCharacter));

        List<Vector3Int> choosedPosition = new List<Vector3Int>();
        bool isNeedToRepaintMovemap = false;

        while (!end)
        {
            //define where player points
            Vector3 worldPosition = IngamePointer.transform.position;
            Vector3Int tilePosition = GridSystem.Instance.GetTilemapCoordsFromWorld(GridSystem.Instance.PathfindingMap, worldPosition);
            Character targetCharacter = GridSystem.Instance.GetCharacterFromCoords(tilePosition);

            if (targetCharacter != null &&
                targetCharacter != _currentCharacter &&
                skillMovemap.MeleeCoords.Contains(tilePosition))
            {
                Vector2 relativeMousePosition = GridSystem.Instance.GetRelativePointPositionInTile(GridSystem.Instance.PathfindingMap,
                                                                                                   tilePosition,
                                                                                                   worldPosition);
                int targetIndex = skillMovemap.MeleeCoords.IndexOf(targetCharacter.Coords);
                List<PointerHandler.PointerStatus> directionsList = new List<PointerHandler.PointerStatus>();
                foreach (Vector3Int nearTileCoords in possiblePositions[targetIndex])
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

                int currentDirectionIndex = -1;
                IngamePointer.DefineSprite(relativeMousePosition, directionsList, out currentDirectionIndex);

                //print current character attack position
                if (choosedPosition.Count != 0)
                    GridSystem.Instance.PrintMovemapTiles(choosedPosition, GridSystem.Instance.MoveTile);
                choosedPosition.Clear();
                choosedPosition.Add(possiblePositions[targetIndex][currentDirectionIndex]);
                isNeedToRepaintMovemap = true;
                GridSystem.Instance.PrintMovemapTiles(choosedPosition, GridSystem.Instance.PositionTile);
            }
            else
            {
                IngamePointer.SetSprite(PointerHandler.PointerStatus.Normal);
                if (isNeedToRepaintMovemap)
                {
                    GridSystem.Instance.PrintMovemapTiles(choosedPosition, GridSystem.Instance.MoveTile);
                    isNeedToRepaintMovemap = false;
                    choosedPosition.Clear();
                }
            }

            if (Input.GetMouseButtonDown(0) &&
                !EventSystem.current.IsPointerOverGameObject() &&
                IngamePointer.GetStatus() != PointerHandler.PointerStatus.Normal)
            {
                if (!_isInputBlocked)
                {
                    _isInputBlocked = true;

                    GridSystem.Instance.ResetMovemap();
                    //Build path
                    Path path = GridSystem.Instance.BuildPath(_currentCharacter.Coords, choosedPosition[0], _currentCharacter);
                    GridSystem.Instance.PrintPath(path.NodePath);
                    List<Vector3Int> coordPath = path.ConvertToCoordPath();

                    _skillData.Target = targetCharacter;
                    _currentCharacter.ExecuteSkill(_skillNo, coordPath);
                    end = true;
                }
            }

            if (Input.GetMouseButtonDown(1) &&
                !EventSystem.current.IsPointerOverGameObject())
            {
                if (!_isInputBlocked)
                {
                    _isInputBlocked = true;

                    //return old movemap
                    GridSystem.Instance.ResetMovemap();
                    GridSystem.Instance.SetMovemap(oldMovemap);
                    GridSystem.Instance.PrintMoveMap(oldMovemap, Node.GetTileStatusFromCharacter(_currentCharacter));
                    SkillPanelRef.SkillDenied(_skillNo);
                    end = true;

                    _isInputBlocked = false;
                }
            }
            yield return null;
        }
        _skillData = null;
    }

    public IEnumerator SelfSkillProcess()
    {
        bool end = false;

        //skill can be denied so we need to save the data
        Movemap oldMovemap = GridSystem.Instance.GetCurrentMovemap().Copy();

        //print new skill movemap                 
        //in this case we need to print only current character tile
        GridSystem.Instance.ResetMovemap();
        List<Vector3Int> coordsList = new List<Vector3Int>();
        coordsList.Add(_currentCharacter.Coords);
        GridSystem.Instance.PrintMovemapTiles(coordsList, GridSystem.Instance.AllyTile);
        while (!end)
        {
            if (Input.GetMouseButtonDown(0) &&
                !EventSystem.current.IsPointerOverGameObject())
            {
                if (!_isInputBlocked)
                {
                    _isInputBlocked = true;

                    GridSystem.Instance.ResetMovemap();
                    _skillData.Target = _currentCharacter;
                    _currentCharacter.ExecuteSkill(_skillNo, null);
                    end = true;
                }
            }

            if (Input.GetMouseButtonDown(1) &&
                !EventSystem.current.IsPointerOverGameObject())
            {
                if (!_isInputBlocked)
                {
                    _isInputBlocked = true;

                    //return old movemap
                    GridSystem.Instance.ResetMovemap();
                    GridSystem.Instance.SetMovemap(oldMovemap);
                    GridSystem.Instance.PrintMoveMap(oldMovemap, Node.GetTileStatusFromCharacter(_currentCharacter));
                    SkillPanelRef.SkillDenied(_skillNo);
                    end = true;

                    _isInputBlocked = false;
                }
            }
            yield return null;
        }
        _skillData = null;
    }

    public IEnumerator RangeSkillProcess()
    {
        bool end = false;

        //skill can be denied so we need to save the data
        Movemap oldMovemap = GridSystem.Instance.GetCurrentMovemap().Copy();

        //define list of target fractions
        List<string> fractionList = new List<string>();
        if (_skillData.FractionTarget == Skill.TargetFraction.Enemy)
            fractionList.Add(_currentCharacter.GetOppositeFraction());
        else if (_skillData.FractionTarget == Skill.TargetFraction.Ally)
            fractionList.Add(_currentCharacter.tag);
        else
        {
            fractionList.Add(_currentCharacter.GetOppositeFraction());
            fractionList.Add(_currentCharacter.tag);
        }

        //print new skill movemap                                                               
        GridSystem.Instance.ResetMovemap();
        Movemap skillMovemap = new Movemap();

        bool isThereEnemyNearby = IsThereEnemyNearby(_currentCharacter);

        if ((isThereEnemyNearby && _skillData.UseNearEnemy) ||
            (!isThereEnemyNearby && _currentCharacter.Properties.Class != CharacterClass.Archer))
        {
            foreach (var fraction in fractionList)
                skillMovemap.RangeCoords.AddRange(DefineAvailableRangedTargets(_currentCharacter, fraction));
        }
        else if (!isThereEnemyNearby && _currentCharacter.Properties.Class == CharacterClass.Archer) //this can reduce amount of raycasts
        {
            skillMovemap.RangeCoords.AddRange(oldMovemap.RangeCoords);
            if (fractionList.Contains(_currentCharacter.tag))
                skillMovemap.RangeCoords.AddRange(DefineAvailableRangedTargets(_currentCharacter, _currentCharacter.tag));
        }

        GridSystem.Instance.PrintMoveMap(skillMovemap, Node.GetTileStatusFromCharacter(_currentCharacter));
        while (!end)
        {
            //define where player points
            Vector3 worldPosition = IngamePointer.transform.position;
            Vector3Int tilePosition = GridSystem.Instance.GetTilemapCoordsFromWorld(GridSystem.Instance.PathfindingMap, worldPosition);
            Character targetCharacter = GridSystem.Instance.GetCharacterFromCoords(tilePosition);

            if (targetCharacter != null &&
                targetCharacter != _currentCharacter &&
                //fractionList.Contains(targetCharacter.tag) &&
                skillMovemap.RangeCoords.Contains(tilePosition))
            {
                IngamePointer.SetSprite(PointerHandler.PointerStatus.RangeAttack);
            }
            else
            {
                IngamePointer.SetSprite(PointerHandler.PointerStatus.Normal);
            }

            if (Input.GetMouseButtonDown(0) &&
                !EventSystem.current.IsPointerOverGameObject() &&
                IngamePointer.GetStatus() != PointerHandler.PointerStatus.Normal)
            {
                if (!_isInputBlocked)
                {
                    _isInputBlocked = true;

                    GridSystem.Instance.ResetMovemap();
                    _skillData.Target = targetCharacter;
                    _currentCharacter.ExecuteSkill(_skillNo, null);
                    end = true;
                }
            }

            if (Input.GetMouseButtonDown(1) &&
                !EventSystem.current.IsPointerOverGameObject())
            {
                if (!_isInputBlocked)
                {
                    _isInputBlocked = true;

                    //return old movemap
                    GridSystem.Instance.ResetMovemap();
                    GridSystem.Instance.SetMovemap(oldMovemap);
                    GridSystem.Instance.PrintMoveMap(oldMovemap, Node.GetTileStatusFromCharacter(_currentCharacter));
                    SkillPanelRef.SkillDenied(_skillNo);
                    end = true;

                    _isInputBlocked = false;
                }
            }
            yield return null;
        }
        _skillData = null;
    }

}


