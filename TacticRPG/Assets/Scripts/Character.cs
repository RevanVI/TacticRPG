using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    public int Health;

    //weapon
    //armor

    public int Speed;
    public int Length;

    public Vector3Int Coords;
    private Vector3Int _targetCoords;

    public Vector3Int TargetCoords
    {
        get
        {
            return _targetCoords;
        }
        set
        {
            if (!_isMoving)
                _targetCoords = value;
        }
    }

    private Rigidbody2D _rb2d;
    private bool _isMoving;

    public UnityEvent OnMoveEnded;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnDie;

    private void Start()
    {
        _isMoving = false;
        _rb2d = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (_isMoving)
        {
            Vector3 targetWorldCoords = GridSystem.Instance.CurrentTilemap.GetCellCenterWorld(_targetCoords);
            Vector3 rotation = (targetWorldCoords - _rb2d.transform.position).normalized;
            float distance = (targetWorldCoords - _rb2d.transform.position).magnitude;
            Vector3 newWorldCoords = _rb2d.transform.position + rotation * /*distance* */Speed * Time.fixedDeltaTime;
            //check overstepping
            if ((newWorldCoords - _rb2d.transform.position).magnitude > distance)
            {
                newWorldCoords = targetWorldCoords;
            }

            //set new tilemap coordinations
            Vector3Int newCoords = GridSystem.Instance.GetTilemapCoordsFromWorld(GridSystem.Instance.CurrentTilemap, newWorldCoords);
            if (Coords != newCoords)
            {
                Debug.Log($"{gameObject.name} Old Coords: ({Coords.x},{Coords.y})");
                Debug.Log($"{gameObject.name} New Coords: ({newCoords.x},{newCoords.y})");
                Debug.Log($"{gameObject.name} Distance: {distance}");
            }
            //Debug.Log($"New coords: ({Coords.x},{Coords.y})");
            _rb2d.transform.position = newWorldCoords;

            //check end of moving 
            if (distance < 0.01f)
            {
                _isMoving = false;
                OnMoveEnded.Invoke();
            }
        }
    }
}
    /*
    public int MoveDistance;
    public int Health;
    public int MaxHealth;
    public int Damage;
    public float Speed;
    public int Length;

    public Vector3Int Coords;
    private Vector3Int _targetCoords;

    public UnityEvent OnMoveEnded;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnDie;

    
public Vector3Int TargetCoords
{
get { return _targetCoords; }

set { if (!_isMoving)
      {
        _targetCoords = value;
      };
}
}

private bool _isMoving;

private Rigidbody2D _rb2d;
private SpriteRenderer _spriteRenderer;
private Transform _hpBar;
//debug variables
static int turnCounter = 0;


// Start is called before the first frame update
protected void Start()
{
_rb2d = GetComponent<Rigidbody2D>();
_spriteRenderer = GetComponent<SpriteRenderer>();
Transform HealthBar = transform.Find("HealthBar");
_hpBar = HealthBar.Find("Hp");
_isMoving = false;
OnMoveEnded.AddListener(() => { GridSystem.Instance.TakeTile(Coords); });
}

// Update is called once per frame
void FixedUpdate()
{
if (_isMoving)
{
    Vector3 targetWorldCoords = GridSystem.Instance.CurrentTilemap.GetCellCenterWorld(_targetCoords);
    Vector3 rotation = (targetWorldCoords - _rb2d.transform.position).normalized;
    float distance = (targetWorldCoords - _rb2d.transform.position).magnitude;
    Vector3 newWorldCoords = _rb2d.transform.position + rotation * /*distance* */
    /* Speed * Time.fixedDeltaTime;
    //check overstepping
    if ((newWorldCoords - _rb2d.transform.position).magnitude > distance)
    {
        newWorldCoords = targetWorldCoords;
    }

    //set new tilemap coordinations
    Vector3Int newCoords = GridSystem.Instance.GetTilemapCoordsFromWorld(GridSystem.Instance.CurrentTilemap, newWorldCoords);
    ++turnCounter;
    if (Coords != newCoords)
    {
        Debug.Log($"{turnCounter}, {gameObject.name} Old Coords: ({Coords.x},{Coords.y})");
        Debug.Log($"{turnCounter}, {gameObject.name} New Coords: ({newCoords.x},{newCoords.y})");
        Debug.Log($"{turnCounter}, {gameObject.name} Distance: {distance}");
        int tileStatus = GridSystem.Instance.IsTileAvailable(newCoords);
        if (tileStatus == 0)
        {
            Debug.Log($"{turnCounter}, {gameObject.name} Successful turn");
            Debug.Log($"{turnCounter}, {gameObject.name} Tile will be realeased: ({Coords.x},{Coords.y})");
            GridSystem.Instance.ReleaseTile(Coords);
            Debug.Log($"{turnCounter}, {gameObject.name} Tile was realeased: ({Coords.x},{Coords.y})");
            Coords = newCoords;
            Debug.Log($"{turnCounter}, {gameObject.name} Tile will be taken: ({Coords.x},{Coords.y})");
            GridSystem.Instance.TakeTile(Coords);
            Debug.Log($"{turnCounter}, {gameObject.name} Tile was taken: ({Coords.x},{Coords.y})");
        }
        else if (tileStatus == 2)
        {
            Debug.Log($"{turnCounter}, {gameObject.name} Attack turn");
            Character character = GameController.Instance.FindCharacter(newCoords);
            character.Stop();
            Debug.Log($"{turnCounter}, {gameObject.name} Another character stopped");
            character.TakeDamage(Damage);
            Stop();
            Debug.Log($"{turnCounter}, {gameObject.name} Character stopped");
        }
    }
    //Debug.Log($"New coords: ({Coords.x},{Coords.y})");
    _rb2d.transform.position = newWorldCoords;

    //check end of moving 
    if (distance < 0.01f)
    {
        _isMoving = false;
        OnMoveEnded.Invoke();
    }
}
}

public void Move()
{
GridSystem.Instance.ReleaseTile(Coords);
_isMoving = true;
}

public void Stop()
{
_targetCoords = Coords;
}

public void TakeDamage(int damage)
{
Health -= damage;
OnDamageTaken.Invoke();
UpdateHPBar();
StartCoroutine(DamageAnimation());
if (Health <= 0)
{
    gameObject.SetActive(false);
    GridSystem.Instance.ReleaseTile(Coords);
    OnDie.Invoke();
}
}

private IEnumerator DamageAnimation()
{
Color color = _spriteRenderer.color;
_spriteRenderer.color = new Color(255, 0, 0);
yield return new WaitForSeconds(0.5f);
_spriteRenderer.color = color;
}

private void UpdateHPBar()
{
float hpPercent = (float)Health / MaxHealth;
_hpBar.localScale = new Vector3(hpPercent, _hpBar.localScale.y, _hpBar.localScale.z);
}
*/

