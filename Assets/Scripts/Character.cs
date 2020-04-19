using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using System;

[Serializable]
public struct CharacterProperties
{
    public string Name;
    public int Level;
    public Sprite Icon;
    public int Health;
    public int CurrentHealth;
    public int Speed;
}

public class Character : MonoBehaviour
{
    public CharacterProperties Properties;

    //weapon
    //armor

    public Vector3Int Coords;
    private Vector3Int _targetCoords;

    public List<Vector3Int> TargetPath;

    private Rigidbody2D _rb2d;
    private bool _isMoving;

    public UnityEvent OnMoveEnded;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnDie;

    private void Start()
    {
        _isMoving = false;
        _rb2d = GetComponent<Rigidbody2D>();

        //register in gameController
        GameController.Instance.RegisterCharacter(this);
        GridSystem.Instance.DefineCharacter(this);
    }

    void FixedUpdate()
    {
        if (_isMoving)
        {
            Vector3 targetWorldCoords = GridSystem.Instance.CurrentTilemap.GetCellCenterWorld(_targetCoords);
            Vector3 rotation = (targetWorldCoords - _rb2d.transform.position).normalized;
            float distance = (targetWorldCoords - _rb2d.transform.position).magnitude;
            Vector3 newWorldCoords = _rb2d.transform.position + rotation * Properties.Speed * Time.fixedDeltaTime;
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
                Coords = newCoords;
            }
            //Debug.Log($"New coords: ({Coords.x},{Coords.y})");
            _rb2d.transform.position = newWorldCoords;

            //check end of moving 
            if (distance < 0.01f)
            {
                if (TargetPath.Count == 0)
                {
                    _isMoving = false;
                    GridSystem.Instance.AddCharacterToNode(Coords, this);
                    OnMoveEnded.Invoke();
                }
                else
                {
                    _targetCoords = TargetPath[0];
                    TargetPath.RemoveAt(0);
                }
            }
        }
    }

    public void Move(List<Vector3Int> path)
    {
        //Release current tile
        bool deb = GridSystem.Instance.RemoveCharacterFromNode(Coords, this);
        Debug.Log($"Remove from node: {deb}");
        TargetPath = path;
        _targetCoords = TargetPath[0];
        TargetPath.RemoveAt(0);
        _isMoving = true;
    }

    //Stop movement
    //Character ends current step (this target in _targetCoords) and stop
    public void Stop()
    {
        TargetPath.Clear();
    }


    public void TakeDamage(int damage)
    {
        Properties.CurrentHealth -= damage;
        OnDamageTaken.Invoke();
        /*
        //UpdateHPBar();
        //StartCoroutine(DamageAnimation());
    /if (Health <= 0)
    {
        gameObject.SetActive(false);
        GridSystem.Instance.ReleaseTile(Coords);
        OnDie.Invoke();
    }
    */
    }
}

/*
private IEnumerator DamageAnimation()
{
Color color = _spriteRenderer.color;
_spriteRenderer.color = new Color(255, 0, 0);
yield return new WaitForSeconds(0.5f);
_spriteRenderer.color = color;
}
*/
/*
private void UpdateHPBar()
{
float hpPercent = (float)Health / MaxHealth;
_hpBar.localScale = new Vector3(hpPercent, _hpBar.localScale.y, _hpBar.localScale.z);
}
*/

