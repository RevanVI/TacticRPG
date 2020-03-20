﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3Int Coords = new Vector3Int();
    public List<Connection> Connections = new List<Connection>();

    /*
     * gameplay status of tile
     */
    public enum TileGameStatus
    {
        Empty = 0,
        Block = 1,
        Taken = 2,
    }
    public TileGameStatus GameStatus;

    /*
     * Status of node in pathfinding process
     */
    public enum NodeProcessStatus
    {
        NotVisited = 0,
        InOpenList = 1,
        InClosedList = 2,
    }
    public NodeProcessStatus ProcessStatus;
    // using in move map building to store the minimum step value
    public int ProcessValue;


    public float CostSoFar;

    public bool AddConnection(Connection connection)
    {
        foreach (var oldConnection in Connections)
        {
            if (oldConnection.StartNode == connection.StartNode && oldConnection.EndNode == connection.EndNode)
                return false;
        }
        Connections.Add(connection);
        return true;
    }

    public bool AddConnection(float cost, Node from, Node to)
    {
        foreach(var connection in Connections)
        {
            if (connection.StartNode == from && connection.EndNode == to)
                return false;
        }
        Connection newConnection = new Connection(cost, from, to);
        Connections.Add(newConnection);
        return true;
    }

    public void RemoveConnection()
    {

    }
}

public class Connection
{
    public float Cost;
    public Node StartNode;
    public Node EndNode;

    public Connection()
    {
        Cost = 0;
        StartNode = null;
        EndNode = null;
    }

    public Connection(float cost, Node from, Node to)
    {
        Cost = cost;
        StartNode = from;
        EndNode = to;
    }
}

public class PathfindingGraph
{
    //key - coordinates in form (x,y)
    public Dictionary<string, Node> NodeGraph;

    public PathfindingGraph()
    {
        NodeGraph = new Dictionary<string, Node>();
    }

    public void GetNodeCoordinates(string nodeKey, out int x, out int y)
    {
        char[] splitters = { '(', ',', ')' };
        string[] coords = nodeKey.Split(splitters);
        x = int.Parse(coords[0]);
        y = int.Parse(coords[1]);
    }

    public string CreateNodeKeyFromCoordinates(int x, int y)
    {
        return $"({x},{y})";
    }

    public void RestoreProcessStatus()
    {
        foreach (var node in NodeGraph.Values)
        {
            node.ProcessStatus = Node.NodeProcessStatus.NotVisited;
            node.ProcessValue = 0;
        }
    }
}
