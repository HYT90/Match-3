using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillPiece : MonoBehaviour
{
    public bool falling;

    float speed = 32f;
    float gravity = 64f;
    Vector2 moveDir;
    RectTransform rect;
    Image img;

    public void Initialized(Sprite piece, Vector2 start)
    {
        falling = true;

        moveDir = Vector2.up;
        moveDir.x = Random.Range(-1.0f, 1.0f);
        moveDir *= speed / 2;

        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        img.sprite = piece;
        rect.anchoredPosition = start;
    }

    private void Update()
    {
        if (!falling) return;
        moveDir.y -= Time.deltaTime * gravity;
        moveDir.x = Mathf.Lerp(moveDir.x, 0, Time.deltaTime);
        rect.anchoredPosition += moveDir * Time.deltaTime * speed;
        if(rect.position.x < -64f || rect.position.x > Screen.width + 64f || rect.position.y < -64f || rect.position.y > Screen.height + 64f)
        {
            falling = false;
        }
    }
}
