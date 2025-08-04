using UnityEngine;

public class NeonBallController : MonoBehaviour, ISpecialSkin
{
    private SpriteRenderer _renderer;

    public void Initialize()
    {
        _renderer ??= GetComponent<SpriteRenderer>();
        _renderer.color = GetRandomColor(Random.Range(0, 7));
    }

    public void OnTap()
    {
        _renderer.color = GetRandomColor(Random.Range(0, 7));
    }

    private Color GetRandomColor(int index)
    {
        return index switch
        {
            0 => Color.red,
            1 => Color.green,
            2 => Color.blue,
            3 => Color.white,
            4 => Color.yellow,
            5 => Color.cyan,
            6 => Color.magenta,
            _ => Color.red,
        };
    }
}
