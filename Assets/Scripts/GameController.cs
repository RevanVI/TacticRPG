﻿using System.Collections;
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
    public Character _currentCharacter;

    public List<Character> CharacterList;
    public EnemyController[] Enemies;

    private bool _isInputBlocked;

    public UnityEvent OnTurnStart;
    public UnityEvent OnTurnEnd;
 
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        TurnQueue = new List<Character>();
        CharacterList = new List<Character>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _isInputBlocked = true;
        State = GameState.Start;

        _isUpdateStarted = false;
        StartCoroutine(WaitStartGame());
    }

    // Update is called once per frame
    void Update()
    {
        _isUpdateStarted = true;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int cellPosition = GridSystem.Instance.GetTilemapCoordsFromScreen(GridSystem.Instance.CurrentTilemap, Input.mousePosition);
            GridSystem.Instance.PrintTileInfo(cellPosition);

            if (State == GameState.PlayerTurn && !_isInputBlocked)
            {
                _isInputBlocked = true;
                cellPosition = GridSystem.Instance.GetTilemapCoordsFromScreen(GridSystem.Instance.CurrentTilemap, Input.mousePosition);
                bool isMovementEnable = GridSystem.Instance.IsMovementEnable(cellPosition);
                if (isMovementEnable)
                {
                    GridSystem.Instance.ResetMovemap();
                    //Build path
                    List<Node> path = GridSystem.Instance.BuildPath(_currentCharacter.Coords, cellPosition, _currentCharacter);
                    GridSystem.Instance.PrintPath(path);
                    List<Vector3Int> coordPath = GridSystem.Instance.ConvertFromGraphPath(path);
                    //move
                    _currentCharacter.Move(coordPath);
                }
                else
                {
                    Debug.Log("Cell out of move map");
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
    }

    private IEnumerator WaitStartGame()
    {
        while (!_isUpdateStarted)
        {
            yield return new WaitForSeconds(0.1f);
        }
        DefineTurnQueue();
        StartNextTurn();
    }

    private void StartNextTurn()
    {
        Debug.Log("Next turn");
        _currentCharacter = TurnQueue[0];
        TurnQueue.RemoveAt(0);
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

    public void RegisterCharacter(Character character)
    {
        if (!CharacterList.Contains(character))
        {
            CharacterList.Add(character);
            character.OnMoveEnded.AddListener(EndTurn);
        }
    }

    public void EndTurn()
    {
        GridSystem.Instance.ResetMovemap();
        TurnQueue.Add(_currentCharacter);
        OnTurnEnd.Invoke();
        StartNextTurn();
    }

    public CharacterProperties GetCurrentCharacterProperties()
    {
        return _currentCharacter.Properties;
    }
}
