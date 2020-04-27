using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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


    private bool _isInputBlocked;

    public UnityEvent OnTurnStart;
    public UnityEvent OnTurnEnd;

    public int RoundCount;
    public int TurnCount;

    private int _battleIdCounter;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        TurnQueue = new List<Character>();
        CharacterList = new List<Character>();

        RoundCount = 0;
        TurnCount = 0;
        _battleIdCounter = 0;
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
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int cellPosition = GridSystem.Instance.GetTilemapCoordsFromScreen(GridSystem.Instance.PathfindingMap, Input.mousePosition);
            GridSystem.Instance.PrintTileInfo(cellPosition);

            if (State == GameState.PlayerTurn && !_isInputBlocked)
            {
                _isInputBlocked = true;

                if (AvailableRangedTargets.IndexOf(cellPosition) != -1)
                {
                    Character targetCharacter = GridSystem.Instance.GetCharacterFromCoords(cellPosition);
                    //animations and so on)
                    targetCharacter.TakeDamage(_currentCharacter.Properties.BaseDamage);
                    EndTurn();
                }
                else
                { 
                    bool isMovementEnable = GridSystem.Instance.IsMovementEnable(cellPosition);
                    if (isMovementEnable)
                    {
                        GridSystem.Instance.ResetMovemap();
                        //Build path
                        List<Node> path = GridSystem.Instance.BuildPath(_currentCharacter.Coords, cellPosition, _currentCharacter);
                        GridSystem.Instance.PrintPath(path);
                        List<Vector3Int> coordPath = GridSystem.Instance.ConvertFromGraphPath(path);

                        //there is two options: this movements is melee attack or not
                        Character targetCharacter = GridSystem.Instance.GetCharacterFromCoords(cellPosition);
                        if (targetCharacter == null)
                        {
                            //move
                            _currentCharacter.Move(coordPath);
                        }
                        else
                        {
                            //attack
                            _currentCharacter.Move(coordPath, targetCharacter);
                        }
                    }
                    else
                    {
                        Debug.Log("Cell out of move map");
                        _isInputBlocked = false;
                    }
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
            StartPlayerTurn();
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

        if (_currentCharacter.Properties.Class == CharacterClass.Archer || _currentCharacter.Properties.Class == CharacterClass.Mage)
        {
            DefineAvailableRangedTargets(_currentCharacter);
            GridSystem.Instance.PrintMovemapTiles(AvailableRangedTargets, GridSystem.Instance.EnemyTile);
        }

        _isInputBlocked = false;
    }

    private IEnumerator StartEnemyTurn()
    {
        State = GameState.EnemyTurn;
        GridSystem.Instance.PrintCharacterMoveMap(_currentCharacter);
        Debug.Log("Enemy turn");
        yield return new WaitForSeconds(1f);
        EndTurn();
    }

    public Character FindCharacter(Vector3Int tileCoords)
    {
        foreach (var character in TurnQueue)
        {
            if (character.Coords == tileCoords)
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
        }
    }

    public void EndTurn()
    {
        GridSystem.Instance.ResetMovemap();
        AvailableRangedTargets.Clear();
        TurnQueue.Add(_currentCharacter);
        OnTurnEnd.Invoke();
        StartNextTurn();
    }

    public void GetCurrentCharacterInfo(out int characterBattleId, out CharacterProperties properties)
    {
        characterBattleId = _currentCharacter.BattleId;
        properties = _currentCharacter.Properties;
    }

    public void OnCharacterTakeDamage(int characterbattleId)
    {
        Character character = FindCharacter(characterbattleId);
        TurnPanelControllerRef.UpdateTurnIcon(characterbattleId, character.Properties);
    }

    public void DefineAvailableRangedTargets(Character character)
    {
        AvailableRangedTargets.Clear();

        //go through all characters on map and define if they are visible from character's point
        string oppositeFraction;
        if (character.tag == "Ally")
            oppositeFraction = "Enemy";
        else
            oppositeFraction = "Ally";
        LayerMask layerMask = LayerMask.GetMask(oppositeFraction, "MapEdges");

        foreach(var otherCharacter in CharacterList)
        {
            //ignore ally and dead characters and characters that stand next to current character
            if (!otherCharacter.CompareTag(character.tag) && otherCharacter.Properties.CurrentHealth >= 0)
            {
                float distanceBetweenCharacters = (otherCharacter.transform.position - character.transform.position).magnitude;
                RaycastHit[] hits = Physics.RaycastAll(character.transform.position, otherCharacter.transform.position, distanceBetweenCharacters, layerMask);

                //we need to define if edge was hit earlier than target character
                bool found = false;
                foreach(var hit in hits)
                {
                    //MapEdges layer no = 8
                    if (hit.transform.gameObject.layer == 8)
                    {
                        found = true;
                    }
                }

                if (!found)
                    AvailableRangedTargets.Add(otherCharacter.Coords);
            }
        }

    }
}
