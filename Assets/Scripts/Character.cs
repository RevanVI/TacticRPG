﻿using System.Collections;
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
    public string Name;
    public int Level;
    public Sprite Icon;
    public int Health;
    public int CurrentHealth;
    public int Speed;
    public int BaseDamage;
    public CharacterClass Class;

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
    public UnityEvent OnDie;

    private void Start()
    {
        _isMoving = false;
        _rb2d = GetComponent<Rigidbody2D>();

        OnDamageTaken = new UnityIntEvent();
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
                        StartCoroutine(AnimateAttack());
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
            StartCoroutine(AnimateAttack());
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

    public IEnumerator AnimateAttack()
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
                _attackedCharacter.TakeDamage(Properties.BaseDamage);
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

