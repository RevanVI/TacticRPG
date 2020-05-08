using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using System;


public enum CharacterClass
{
    Warrior = 0,
    Archer = 1,
    Healer = 2,
    Mage = 3,
}

[Serializable]
public struct CharacterProperties
{
    //Main properties
    public string Name;
    public CharacterClass Class;
    public int Level;
    public Sprite Icon;
    public int Health;
    public int CurrentHealth;
    public int Speed;

    //Attack properties
    public int RangedDamage;
    public int MeleeDamage;
    public int MaxMissiles;
    public int CurrentMissiles;
}



public class UnityIntEvent: UnityEvent<int>
{

}

public class Character : MonoBehaviour
{
    public CharacterProperties Properties;

    public int BattleId;
    //weapon
    //armor

    public Vector3Int Coords;
    private Vector3Int _targetCoords;

    public List<Vector3Int> TargetPath;

    private Rigidbody2D _rb2d;
    private bool _isMoving;
    private Character _attackedCharacter;

    public UnityEvent OnMoveEnded;
    public UnityIntEvent OnDamageTaken;
    public UnityIntEvent OnDie;

    private void Start()
    {
        _isMoving = false;
        _rb2d = GetComponent<Rigidbody2D>();

        OnDamageTaken = new UnityIntEvent();
        OnDie = new UnityIntEvent();
        //register in gameController
        GameController.Instance.RegisterCharacter(this);
        GridSystem.Instance.DefineCharacter(this);
    }

    void FixedUpdate()
    {
        if (_isMoving)
        {
            Vector3 targetWorldCoords = GridSystem.Instance.PathfindingMap.GetCellCenterWorld(_targetCoords);
            Vector3 rotation = (targetWorldCoords - _rb2d.transform.position).normalized;
            float distance = (targetWorldCoords - _rb2d.transform.position).magnitude;
            Vector3 newWorldCoords = _rb2d.transform.position + rotation * Properties.Speed * Time.fixedDeltaTime;
            //check overstepping
            if ((newWorldCoords - _rb2d.transform.position).magnitude > distance)
            {
                newWorldCoords = targetWorldCoords;
            }

            //set new tilemap coordinations
            Coords = GridSystem.Instance.GetTilemapCoordsFromWorld(GridSystem.Instance.PathfindingMap, newWorldCoords);
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
                    if (_attackedCharacter != null && TargetPath.Count == 0)
                    {
                        _isMoving = false;
                        GridSystem.Instance.AddCharacterToNode(Coords, this);
                        StartCoroutine(AnimateMeleeAttack());
                    }
                }
            }
        }
    }

    public void Move(List<Vector3Int> path, Character attackedCharacter = null)
    {
        _attackedCharacter = attackedCharacter;
        TargetPath = path;
        _targetCoords = TargetPath[0];
        TargetPath.RemoveAt(0);
        if (TargetPath.Count == 0 && attackedCharacter != null)
        {
            //attacking near enemy
            StartCoroutine(AnimateMeleeAttack());
        }
        else
        {
            GridSystem.Instance.RemoveCharacterFromNode(Coords, this);
            _isMoving = true;
        }
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
        OnDamageTaken.Invoke(BattleId);

        if (Properties.CurrentHealth <= 0)
        {
            gameObject.SetActive(false);
            GridSystem.Instance.RemoveCharacterFromNode(Coords, this);
            OnDie.Invoke(BattleId);
        }
    }

    public IEnumerator AnimateMeleeAttack()
    {

        float curTime = 0;
        bool end = false;
        int moveStatus = 0;

        Vector3 startWorldsCoords = transform.position;
        Vector3 targetWorldCoords = _attackedCharacter.transform.position;

        //move forward
        while (!end)
        {
            curTime += Time.deltaTime;
            if (curTime > 0.5f)
            {
                curTime = 0.5f;
                end = true;
                ++moveStatus;
            }

            float x = Mathf.Lerp(startWorldsCoords.x, targetWorldCoords.x, curTime / 0.5f);
            float y = Mathf.Lerp(startWorldsCoords.y, targetWorldCoords.y, curTime / 0.5f);
            transform.position = new Vector3(x, y, startWorldsCoords.z);
            yield return null;

            if (moveStatus == 1)
            {
                _attackedCharacter.TakeDamage(Properties.MeleeDamage);
                targetWorldCoords = startWorldsCoords;
                startWorldsCoords = transform.position;
                ++moveStatus;
                end = false;
                curTime = 0f;
            }
        }

        _attackedCharacter = null;
        OnMoveEnded.Invoke();
    }

    public void AttackAtRange(Character attackedCharacter)
    {
        StartCoroutine(AnimateAttackAtRange(attackedCharacter));
    }

    public IEnumerator AnimateAttackAtRange(Character attackedCharacter)
    {
        float curTime = 0f;
        bool end = false;

        GridSystem.Instance.ResetMovemap();
        List<Vector3Int> tilesCoords = new List<Vector3Int>();
        tilesCoords.Add(attackedCharacter.Coords);
        GridSystem.Instance.PrintMovemapTiles(tilesCoords, GridSystem.Instance.EnemyTile);
        while (!end)
        {
            curTime += Time.deltaTime;
            if (curTime >= 1.5f)
            {
                curTime = 1.5f;
                end = true;
            }
            yield return null;
        }

        GridSystem.Instance.ResetMovemap();
        attackedCharacter.TakeDamage(Properties.RangedDamage);
        --Properties.CurrentMissiles;
        OnMoveEnded.Invoke();
    }

    public static string GetStringClassName(CharacterClass characterClass)
    {
        string className;
        if (characterClass == CharacterClass.Warrior)
            className = "Warrior";
        else if (characterClass == CharacterClass.Archer)
            className = "Archer";
        else if (characterClass == CharacterClass.Mage)
            className = "Mage";
        else //if (characterClass == CharacterClass.Healer)
            className = "Healer";
        return className;
    }

    public string GetOppositeFraction()
    {
        if (tag == "Ally")
            return "Enemy";
        else
            return "Ally";
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

