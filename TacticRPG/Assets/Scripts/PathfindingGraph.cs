using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Node
{
    List<Connection> connections;

    int GameStatus;

    int ProcessStatus;

    float CostSoFar;
}

public struct Connection
{
    float Cost;
    Node startNode;
    ref Node 
}

public class PathfindingGraph
{
    
}
