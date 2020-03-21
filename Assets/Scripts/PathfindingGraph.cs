using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    //general variables
    public Vector3Int Coords = new Vector3Int();
    public List<Connection> Connections = new List<Connection>();

    //gameplay variables
    public enum TileGameStatus
    {
        Empty = 0,
        Block = 1,
        Taken = 2,
    }
    public TileGameStatus GameStatus;

    //processing variables
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
    public float EstimatedCost;

    public Connection connection;

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

    public Node GetNode(Vector3Int coords)
    {
        string key = CreateNodeKeyFromCoordinates(coords.x, coords.y);
        if (NodeGraph.ContainsKey(key))
            return NodeGraph[key];
        return null;
    }

    public void RestoreProcessStatus()
    {
        foreach (var node in NodeGraph.Values)
        {
            node.ProcessStatus = Node.NodeProcessStatus.NotVisited;
            node.ProcessValue = 0;
        }
    }

    public List<Node> AStarPathfinding(Node start, Node end)
    {
        start.CostSoFar = 0;
        start.EstimatedCost = Heuristic(start, end);

        List<Node> openList = new List<Node>();
        openList.Add(start);

        Node currentNode = null;
        while (openList.Count != 0)
        {
            currentNode = openList[0];

            //find node with smallest estimated cost in open list
            foreach(var node in openList)
            {
                if (node.EstimatedCost < currentNode.EstimatedCost)
                    currentNode = node;
            }

            //if reach goal - stop searching and start build path
            if (currentNode == end)
                break;

            foreach(var connection in currentNode.Connections)
            {
                Node endNode = connection.EndNode;
                float cost = currentNode.CostSoFar + connection.Cost;
                float heuristic;

                if (endNode.ProcessStatus == Node.NodeProcessStatus.InClosedList)
                {
                    if (endNode.CostSoFar < cost)
                        continue;
                    heuristic = endNode.EstimatedCost - endNode.CostSoFar;
                }
                else if (endNode.ProcessStatus == Node.NodeProcessStatus.InOpenList)
                {
                    if (endNode.CostSoFar < cost)
                        continue;
                    heuristic = endNode.EstimatedCost - endNode.CostSoFar;
                }
                else
                    heuristic = Heuristic(endNode, end);

                endNode.connection = connection;
                endNode.CostSoFar = cost;
                endNode.EstimatedCost = cost + heuristic;
                if (endNode.ProcessStatus != Node.NodeProcessStatus.InOpenList)
                {
                    endNode.ProcessStatus = Node.NodeProcessStatus.InOpenList;
                    openList.Add(endNode);
                }
            }
            openList.Remove(currentNode);
            currentNode.ProcessStatus = Node.NodeProcessStatus.InClosedList;
        }

        //if goal node did not found
        if (currentNode != end)
            return null;
        //else build path
        List<Node> path = new List<Node>();
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.connection.StartNode;
        }
        path.Reverse();
        return path;
    }

    public float Heuristic(Node start, Node end)
    {
        float x = start.Coords.x - end.Coords.x;
        float y = start.Coords.y - end.Coords.y;

        return Mathf.Sqrt(x*x+y*y);
    }
}
