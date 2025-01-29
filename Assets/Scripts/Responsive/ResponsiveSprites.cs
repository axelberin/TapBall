using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveSprites : MonoBehaviour
{
    void Start()
    {
        ResizeToAspectRatio();
    }

    void ResizeToAspectRatio()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float targetWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
        float targetHeight = Camera.main.orthographicSize * 2.0f;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleFactor = Mathf.Min(targetWidth / spriteSize.x, targetHeight / spriteSize.y);
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    }
}
