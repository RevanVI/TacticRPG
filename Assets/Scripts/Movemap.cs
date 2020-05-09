using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movemap
{
    public List<Vector3Int> MoveCoords;
    public List<Vector3Int> EnemyMeleeCoords;
    public List<Vector3Int> RangeCoords;

    public Movemap()
    {
        MoveCoords = new List<Vector3Int>();
        EnemyMeleeCoords = new List<Vector3Int>();
        RangeCoords = new List<Vector3Int>();
    }

    public bool IsMovementEnable(Vector3Int targetPosition)
    {
        if (MoveCoords.IndexOf(targetPosition) != -1)
            return true;
        return false;
    }

    public bool IsCoordsInMovemap(Vector3Int targetPosition)
    {
        if (MoveCoords.IndexOf(targetPosition) != -1 ||
            EnemyMeleeCoords.IndexOf(targetPosition) != -1 ||
            RangeCoords.IndexOf(targetPosition) != -1)
            return true;
        return false;
    }

    public void Clear()
    {
        MoveCoords.Clear();
        EnemyMeleeCoords.Clear();
        RangeCoords.Clear();
    }

    public List<Vector3Int> GetMeleeTileCoordsFromMovemap(Node.TileGameStatus gameStatus)
    {
        if (gameStatus == Node.TileGameStatus.Empty)
        {
            return MoveCoords;
        }
        else 
        {
            return EnemyMeleeCoords;
        }
        return null;
    }

    public Movemap Copy()
    {
        Movemap newMovemap = new Movemap();
        newMovemap.MoveCoords.AddRange(MoveCoords);
        newMovemap.EnemyMeleeCoords.AddRange(EnemyMeleeCoords);
        newMovemap.RangeCoords.AddRange(RangeCoords);
        return newMovemap;
    }

}
