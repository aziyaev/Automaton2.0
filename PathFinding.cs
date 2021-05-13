using System;
using System.Collections.Generic;
using System.Drawing;

public class Pathfinding
{
    public static int SIZEX;
    public static int SIZEY;
    public static int CELLSIZE;

    private static Node[,] nodes;

    public static void init(int sizeX, int sizeY, int cellSize)
    {
        SIZEX = sizeX;
        SIZEY = sizeY;
        CELLSIZE = cellSize;

        nodes = new Node[SIZEX, SIZEY];

        for (int i = 0; i < SIZEX; i++)
        {
            for (int ii = 0; ii < SIZEY; ii++)
            {
                nodes[i, ii] = new Node(i, ii);
            }
        }
    }

    public static List<Point> getPositions(Point startPos, Point endPos)
    {
        Point startPoint = new Point((int)(startPos.X / CELLSIZE), (int)(startPos.Y / CELLSIZE));
        Point endPoint = new Point((int)(endPos.X / CELLSIZE), (int)(endPos.Y / CELLSIZE));

        List<Point> result = new List<Point>();

        foreach (var item in getPoints(startPoint, endPoint))
        {
            result.Add(new Point(item.X * CELLSIZE, item.Y * CELLSIZE));
        }

        return result;
    }

    public static List<Point> getPoints(Point startPos, Point targetPos)
    {
        updateNodesData();

        Node startNode = nodes[startPos.X, startPos.Y];
        Node targetNode = nodes[targetPos.X, targetPos.Y];

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node node = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].weight < node.weight || openSet[i].weight == node.weight)
                {
                    if (openSet[i].distance < node.distance)
                        node = openSet[i];
                }
            }

            openSet.Remove(node);
            closedSet.Add(node);

            if (node == targetNode)
            {
                return retracePath(startNode, targetNode);
            }

            foreach (Node neighbour in node.getNeighbours())
            {
                if (neighbour.isBlocked || closedSet.Contains(neighbour) || neighbour.isBlockedAround() )
                {
                    continue;
                }

                int newCostToNeighbour = node.moveCost + getDistance(node, neighbour);

                if (newCostToNeighbour < neighbour.moveCost || !openSet.Contains(neighbour))
                {
                    neighbour.moveCost = newCostToNeighbour;
                    neighbour.distance = getDistance(neighbour, targetNode);
                    neighbour.weight = neighbour.moveCost + neighbour.distance;
                    neighbour.lastNode = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null;
    }

    private static List<Point> retracePath(Node startNode, Node endNode)
    {
        List<Point> path = new List<Point>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(new Point(currentNode.X, currentNode.Y));
            currentNode = currentNode.lastNode;
        }

        path.Reverse();

        return path;
    }

    private static int getDistance(Node nodeA, Node nodeB)
    {
        int dstX = Math.Abs(nodeA.X - nodeB.X);
        int dstY = Math.Abs(nodeA.Y - nodeB.Y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        else
            return 14 * dstX + 10 * (dstY - dstX);
    }

    public static Node getNode(int X, int Y)
    {
        return nodes[X, Y];
    }

    public static void setNodeStatus(int X, int Y, bool isBlocked)
    {
        nodes[X, Y].isBlocked = isBlocked;
    }

    public static void updateNodesData()
    {
        for (int i = 0; i < SIZEX; i++)
        {
            for (int ii = 0; ii < SIZEY; ii++)
            {
                nodes[i, ii].distance = 0;
                nodes[i, ii].moveCost = 0;
                nodes[i, ii].weight = 0;
                nodes[i, ii].lastNode = null;
                nodes[i, ii].isBlocked = Main.cells[i, ii].isBlocked;
            }
        }
    }

    public static void clearNodesData()
    {
        for (int i = 0; i < SIZEX; i++)
        {
            for (int ii = 0; ii < SIZEY; ii++)
            {
                nodes[i, ii].distance = 0;
                nodes[i, ii].moveCost = 0;
                nodes[i, ii].weight = 0;
                nodes[i, ii].lastNode = null;
            }
        }
    }

    public class Node
    {
        public int X, Y;

        public int distance;
        public int moveCost;
        public int weight;

        public bool isBlocked;

        public Node lastNode;

        public Node(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public List<Node> getNeighbours()
        {
            List<Node> result = new List<Node>();

            if (check(0, 1))
                result.Add(Pathfinding.getNode(X, Y + 1));
            if (check(1, 1))
                result.Add(Pathfinding.getNode(X + 1, Y + 1));
            if (check(1, 0))
                result.Add(Pathfinding.getNode(X + 1, Y));
            if (check(1, -1))
                result.Add(Pathfinding.getNode(X + 1, Y - 1));
            if (check(0, -1))
                result.Add(Pathfinding.getNode(X, Y - 1));
            if (check(-1, -1))
                result.Add(Pathfinding.getNode(X - 1, Y - 1));
            if (check(-1, 0))
                result.Add(Pathfinding.getNode(X - 1, Y));
            if (check(-1, 1))
                result.Add(Pathfinding.getNode(X - 1, Y + 1));

            return result;
        }

        public bool isBlockedAround()
        {
            bool result = false;

            if (check(0, 1) && getNode(X, Y + 1).isBlocked && check(1, 0) && getNode(X + 1, Y).isBlocked)
                result = true;
            if (check(0, 1) && getNode(X, Y + 1).isBlocked && check(-1, 0) && getNode(X - 1, Y).isBlocked)
                result = true;
            if (check(0, -1) && getNode(X, Y - 1).isBlocked && check(-1, 0) && getNode(X - 1, Y).isBlocked)
                result = true;
            if (check(0, -1) && getNode(X, Y - 1).isBlocked && check(1, 0) && getNode(X + 1, Y).isBlocked)
                result = true;

            return result;
        }

        private bool check(int shiftX, int shiftY)
        {
            return X + shiftX < Pathfinding.SIZEX && X + shiftX > 0 && Y + shiftY < Pathfinding.SIZEY && Y + shiftY > 0;
        }
    }
}
