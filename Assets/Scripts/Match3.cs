using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;

    [Header("UI Elements")]
    public Sprite[] pieces;
    public RectTransform gameBoard;
    public RectTransform killBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;
    public GameObject killPiece;

    int columns = 9;
    int rows = 14;
    int[] fills;
    Node[,] board;

    List<NodePiece> update;
    List<FlippedPiece> flipped;
    List<NodePiece> dead;
    List<KillPiece> killed;

    System.Random random;

    void Start()
    {
        StartGame();
    }

    private void Update()
    {
        List<NodePiece> finishUpdating = new List<NodePiece>();
        for (int i = 0; i < update.Count; i++)
        {
            NodePiece piece = update[i];
            if (!piece.UpdatePiece())
            {
                finishUpdating.Add(piece);
            }
        }
        for (int i = 0; i < finishUpdating.Count; i++)
        {
            NodePiece piece = finishUpdating[i];
            FlippedPiece flip = GetFlipped(piece);
            NodePiece flippedPiece = null;

            int x = (int)piece.index.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, columns);

            List<Point> connected = IsConnected(piece.index, true);
            bool wasFlipped = (flip != null);

            if (wasFlipped)
            {
                flippedPiece = flip.GetOtherPiece(piece);
                AddPoints(ref connected, IsConnected(flippedPiece.index, true));
            }
            if(connected.Count == 0)
            {
                if (wasFlipped)
                {
                    FlipPiece(piece.index, flippedPiece.index, false);
                }
            }
            else
            {
                foreach(Point p in connected)
                {
                    KilledPiece(p);
                    Node node = GetNodeAtPoint(p);
                    NodePiece nodePiece = node.GetPiece();
                    if(nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                        dead.Add(nodePiece);
                    }
                    node.SetPiece(null);
                }

                ApplyGravityToBoard();
            }
            flipped.Remove(flip);
            update.Remove(piece);
        }
    }

    void ApplyGravityToBoard()
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = rows-1; j >= 0; j--)
            {
                Point p = new Point(i, j);
                Node node = GetNodeAtPoint(p);
                int val = GetValueAtPoint(p);
                if (val != 0) continue;
                for (int nj = (j-1); nj >= -1; nj--)
                {
                    Point next = new Point(i, nj);
                    int nextVal = GetValueAtPoint(next);
                    if (nextVal == 0) continue;
                    if(nextVal != -1)
                    {
                        Node got = GetNodeAtPoint(next);
                        NodePiece piece = got.GetPiece();

                        node.SetPiece(piece);
                        update.Add(piece);

                        got.SetPiece(null);
                    }
                    else
                    {
                        int newVal = FillPiece();
                        NodePiece piece;
                        Point fallP = new Point(i, -1 - fills[i]);
                        if(dead.Count > 0)
                        {
                            NodePiece revived = dead[0];
                            revived.gameObject.SetActive(true);
                            piece = revived;
                            
                            dead.RemoveAt(0);
                        }
                        else
                        {
                            GameObject obj = Instantiate(nodePiece, gameBoard);
                            NodePiece n = obj.GetComponent<NodePiece>();
                            piece = n;
                        }
                        piece.Initialize(newVal, p, pieces[newVal - 1]);
                        piece.rect.anchoredPosition = GetPositionFromPoint(fallP);

                        Node hole = GetNodeAtPoint(p);
                        hole.SetPiece(piece);
                        ResetPiece(piece);
                        fills[i]++;
                    }
                    break;
                }
            }
        }
    }

    FlippedPiece GetFlipped(NodePiece p)
    {
        FlippedPiece flip = null;
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].GetOtherPiece(p) != null)
            {
                flip = flipped[i];
                break;
            }
        }
        return flip;
    }

    void StartGame()
    {
        fills = new int[columns];
        string seed = GetRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPiece>();
        dead = new List<NodePiece>();
        killed = new List<KillPiece>();

        InitializedBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    void InitializedBoard()
    {
        board = new Node[columns, rows];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                board[j, i] = new Node((boardLayout.rows[i].row[j]) ? -1 : FillPiece(), new Point(j, i));
            }
        }
    }

    void VerifyBoard()
    {
        List<int> remove;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Point p = new Point(j, i);
                int val = GetValueAtPoint(p);
                if (val <= 0) continue;

                remove = new List<int>();
                while(IsConnected(p, true).Count > 0)
                {
                    val = GetValueAtPoint(p);
                    if (!remove.Contains(val))
                    {
                        remove.Add(val);
                    }
                    SetValuePoint(p, NewValue(ref remove));
                }
            }
        }
    }

    void InstantiateBoard()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Node node = GetNodeAtPoint(new Point(j, i));
                int val = node.value;
                if (val <= 0) continue;
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * j), -32 - (64 * i));
                piece.Initialize(val, new Point(j, i), pieces[val - 1]);
                node.SetPiece(piece);
            }
        }
    }

    public void ResetPiece(NodePiece piece)
    {
        piece.Resetposition();
        update.Add(piece);
    }

    public void FlipPiece(Point one, Point two, bool main)
    {
        if (GetValueAtPoint(one) < 0) return;
        Node nodeOne = GetNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.GetPiece();
        if(GetValueAtPoint(two) > 0)
        {
            Node nodeTwo = GetNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.GetPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            if (main)
            {
                flipped.Add(new FlippedPiece(pieceOne, pieceTwo));
            }

            update.Add(pieceOne);
            update.Add(pieceTwo);
        }
        else
        {
            ResetPiece(pieceOne);
        }
    }

    void KilledPiece(Point p)
    {
        List<KillPiece> available = new List<KillPiece>();
        for (int i = 0; i < killed.Count; i++)
        {
            if (!killed[i].falling)
            {
                available.Add(killed[i]);
            }
        }
        KillPiece set = null;
        if(available.Count > 0)
        {
            set = available[0];
        }
        else
        {
            GameObject kill = GameObject.Instantiate(killPiece, killBoard);
            KillPiece kp = kill.GetComponent<KillPiece>();
            set = kp;
            killed.Add(kp);
        }

        int val = GetValueAtPoint(p) - 1;
        if (set != null && val >= 0 && val < pieces.Length)
        {
            set.Initialized(pieces[val], GetPositionFromPoint(p));
        }
    }

    List<Point> IsConnected(Point p, bool main)
    {
        List<Point> connected = new List<Point>();
        int val = GetValueAtPoint(p);
        Point[] directions =
        {
            Point.Up,
            Point.Right,
            Point.Down,
            Point.Left
        };

        foreach(Point dir in directions)//判定珠寶的種類 在同一方向是否2個以上相同
        {
            List<Point> line = new List<Point>();

            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(p, Point.Multiply(dir, i));
                if (GetValueAtPoint(check) == val)
                {
                    line.Add(check);
                    same++;
                }
            }
            if (same > 1)
            {
                AddPoints(ref connected, line);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            List<Point> line = new List<Point>();

            int same = 0;
            Point[] check =
            {
                Point.Add(p, directions[i]),
                Point.Add(p, directions[i + 2])
            };
            foreach(Point next in check)
            {
                if(GetValueAtPoint(next) == val)
                {
                        line.Add(next);
                        same++;
                }
            }

            if (same > 1)
            {
                AddPoints(ref connected, line);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            List<Point> square = new List<Point>();

            int same = 0;
            int next = i + 1;
            if(next >= 4)
            {
                next -= 4;
            }

            Point[] check =
            {
                Point.Add(p, directions[i]),
                Point.Add(p, directions[next]),
                Point.Add(p, Point.Add(directions[i], directions[next]))
            };
            foreach (Point sc in check)
            {
                if (GetValueAtPoint(sc) == val)
                {
                    square.Add(sc);
                    same++;
                }
            }

            if(same > 2)
            {
                AddPoints(ref connected, square);
            }
        }

        if (main)
        {
            for (int i = 0; i < connected.Count; i++)
            {
                AddPoints(ref connected, IsConnected(connected[i], false));
            }
        }

        /* UNNESSASARY
        if(connected.Count > 0)
        {
            connected.Add(p);
        }
        */

        return connected;
    }

    void AddPoints(ref List<Point> points, List<Point> add)
    {
        foreach(Point p in add)
        {
            bool doAdd = true;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }

            if (doAdd)
            {
                points.Add(p);
            }
        }
    }

    int FillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length)) + 1;
        return val;
    }

    int GetValueAtPoint(Point p )
    {
        if(p.x < 0 || p.x >= columns || p.y < 0 || p.y >= rows)
        {
            return -1;
        }
        return board[p.x, p.y].value;
    }

    void SetValuePoint(Point p, int v)
    {
        board[p.x, p.y].value = v;
    }

    Node GetNodeAtPoint(Point p)
    {
        return board[p.x, p.y];
    }

    int NewValue(ref List<int> _remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < pieces.Length; i++)
        {
            available.Add(i + 1);
        }
        foreach(int i in _remove)
        {
            available.Remove(i);
        }

        return available.Count <= 0 ? 0 : available[random.Next(0, available.Count)];
    }

    string GetRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456790!@#$%^&*()";
        for (int i = 0; i < 20; i++)
        {
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        }
        return seed;
    }

    public Vector2 GetPositionFromPoint(Point p)
    {
        return new Vector2(32 + (64 * p.x), -32 - (64 * p.y));
    }
}

[System.Serializable]
public class Node
{
    public int value;//珠寶的編號
    public Point index;
    public NodePiece piece;

    public Node(int v, Point p)
    {
        value = v;
        index = p;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (!piece) return;
        piece.SetIndex(index);
    }

    public NodePiece GetPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class FlippedPiece
{
    public NodePiece one;
    public NodePiece two;

    public FlippedPiece(NodePiece o, NodePiece t)
    {
        one = o;
        two = t;
    }

    public NodePiece GetOtherPiece(NodePiece p)
    {
        if(p == one)
        {
            return two;
        }else if(p == two)
        {
            return one;
        }
        else
        {
            return null;
        }
    }
}
