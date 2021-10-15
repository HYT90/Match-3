using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point
{
    public int x;
    public int y;

    public Point(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public void Multiply(int m)
    {
        x *= m;
        y *= m;
    }

    public void Add(Point a)
    {
        x += a.x;
        y += a.y;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public bool Equals(Point p)
    {
        return (x == p.x && y == p.y);
    }

    public static Point FromVector(Vector2 p)
    {
        return new Point((int)p.x, (int)p.y);
    }

    public static Point FromVector(Vector3 p)
    {
        return new Point((int)p.x, (int)p.y);
    }

    public static Point Multiply(Point p, int m)
    {
        return new Point(p.x * m, p.y * m);
    }

    public static Point Add(Point p, Point a)
    {
        return new Point(p.x + a.x, p.y + a.y);
    }

    public static Point Clone(Point p)
    {
        return new Point(p.x, p.y);
    }

    public static Point Zero
    {
        get { return new Point(0, 0); }
    }
    public static Point One
    {
        get { return new Point(1, 1); }
    }
    public static Point Up
    {
        get { return new Point(0, 1); }
    }
    public static Point Down
    {
        get { return new Point(0, -1); }
    }
    public static Point Right
    {
        get { return new Point(1, 0); }
    }
    public static Point Left
    {
        get { return new Point(-1, 0); }
    }
}
