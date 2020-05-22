using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public List<Node> NodePath;
    public bool IsEffectOnPath;

    public Path()
    {
        NodePath = new List<Node>();
        IsEffectOnPath = false;
    }

    public List<Vector3Int> ConvertToCoordPath()
    {
        List<Vector3Int> coordPath = new List<Vector3Int>();
        for (int i = 0; i < NodePath.Count; ++i)
            coordPath.Add(NodePath[i].Coords);
        return coordPath;
    }

}
