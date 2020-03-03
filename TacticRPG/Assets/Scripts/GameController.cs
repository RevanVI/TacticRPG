using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Start,
    PlayerTurn,
    EnemyTurn,
    Endgame
};

public class GameController : MonoBehaviour
{
    /*
    public static GameController Instance;
    public GridSystem Grid;
    public GameState State;

    public PlayerController PlayerCharacter;
    public EnemyController[] Enemies;

    private bool _isInputBlocked;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _isInputBlocked = true;
        State = GameState.Start;

        //new WaitForSeconds(2f);

        StartPlayerTurn();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && State == GameState.PlayerTurn && !_isInputBlocked)
        {
            _isInputBlocked = true;
            Vector3Int cellPosition = GridSystem.Instance.GetTilemapCoordsFromScreen(GridSystem.Instance.CurrentTilemap, Input.mousePosition);
            bool isMovementEnable = GridSystem.Instance.IsMovementEnable(cellPosition);
            if (isMovementEnable)
            {
                GridSystem.Instance.Movemap.ClearAllTiles();
                PlayerCharacter.TargetCoords = cellPosition;
                PlayerCharacter.Move();
                StartCoroutine(StartEnemyTurn());
            }
            else
            {
                Debug.Log("Cell out of move map");
                _isInputBlocked = false;
            }
        }
    }

    private void StartPlayerTurn()
    {
        State = GameState.PlayerTurn;
        GridSystem.Instance.PrintCharacterMoveMap(PlayerCharacter);
        _isInputBlocked = false;
    }

    private IEnumerator StartEnemyTurn()
    {
        State = GameState.EnemyTurn;
        Debug.Log("Enemy turn");
        for (int i = 0; i < Enemies.Length; ++i)
        {
            Enemies[i].ChooseMovement();
            Enemies[i].Move();
        }
        yield return new WaitForSeconds(1f);
        StartPlayerTurn();
    }

    private IEnumerator Calculate()
    {

        return null;
    }

    public Character FindCharacter(Vector3Int tileCoords)
    {
        if (PlayerCharacter.Coords == tileCoords)
            return PlayerCharacter;
        for (int i = 0; i < Enemies.Length; ++i)
        {
            if (Enemies[i].Coords == tileCoords)
                return Enemies[i];
        }
        return null;
    }
    */
}
