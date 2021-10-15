using UnityEngine;

public class MovePiece : MonoBehaviour
{
    public static MovePiece instance;
    Match3 game;

    NodePiece moving;
    Point newIndex;
    Vector2 mouseStart;
    
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    
    void Start()
    {
        game = GetComponent<Match3>();
    }

    private void Update()
    {
        if(moving != null)
        {
            Vector2 dir = ((Vector2)Input.mousePosition - mouseStart);
            Vector2 nDir = dir.normalized;
            Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

            newIndex = Point.Clone(moving.index);
            Point add = Point.Zero;
            if(dir.magnitude > 32)
            {
                if(aDir.x > aDir.y)
                {
                    add = (new Point((nDir.x > 0) ? 1 : -1, 0));
                }else if(aDir.y > aDir.x)
                {
                    add = (new Point(0, (nDir.y > 0) ? -1 : 1));
                }
            }
            newIndex.Add(add);

            Vector2 pos = game.GetPositionFromPoint(moving.index);
            if (!newIndex.Equals(moving.index))
            {
                pos += Point.Multiply(new Point(add.x, -add.y), 16).ToVector();
            }
            moving.MovePositionTo(pos);
        }
    }

    public void MoveThePiece(NodePiece piece)
    {
        if (moving != null) return;
        moving = piece;
        mouseStart = Input.mousePosition;
    }

    public void DropThePiece()
    {
        if (moving == null) return;
        if (!newIndex.Equals(moving.index))
        {
            game.FlipPiece(moving.index, newIndex, true);
        }
        else
        {
            game.ResetPiece(moving);
        }

        game.ResetPiece(moving);
        moving = null;
    }
}
