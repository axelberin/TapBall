using UnityEngine;

public class ResponsiveSprites : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool maintainAspectRatio = true;
    [SerializeField] private float percentageOfScreenHeight = 0.2f; // 20% de la altura de la pantalla
    [SerializeField] private Vector2 padding = Vector2.zero; // Padding en unidades de mundo

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        AdjustSprite();
    }

    private void AdjustSprite()
    {
        // Obtener la cámara principal
        Camera camera = Camera.main;
        if (camera == null) 
            return;

        // Calcular altura deseada en unidades de mundo
        float targetWorldHeight = camera.orthographicSize * 2f * percentageOfScreenHeight;

        // Obtener dimensiones originales del sprite
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // Calcular escala necesaria
        float scale;
        if (maintainAspectRatio)
        {
            scale = (targetWorldHeight - padding.y) / spriteSize.y;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            // Calcular ancho en unidades de mundo basado en el aspect ratio de la pantalla
            float targetWorldWidth = targetWorldHeight * (Screen.width / (float)Screen.height);
            transform.localScale = new Vector3(
                (targetWorldWidth - padding.x) / spriteSize.x,
                (targetWorldHeight - padding.y) / spriteSize.y,
                1f
            );
        }
    }

    // Opcional: Ajustar cuando la orientación cambie
    private void OnRectTransformDimensionsChange()
    {
        AdjustSprite();
    }
}
