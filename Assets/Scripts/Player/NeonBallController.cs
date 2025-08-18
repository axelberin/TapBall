using UnityEngine;
using System.Collections;

public class NeonBallController : SpecialSkin
{
    private SpriteRenderer _renderer;
    private Coroutine _idleCoroutine;
    private float _idleDelay = 3f;
    private float _colorChangeInterval = 0.5f;
    private bool _isIdleChangingColor;

    public override void Initialize()
    {
        _renderer ??= GetComponent<SpriteRenderer>();
        _renderer.color = GetRandomColor(Random.Range(0, 7));

        StartCoroutine(IdleTimer());
    }

    public override void OnTap()
    {
        _renderer.color = GetRandomColor(Random.Range(0, 7));

        // Se reinicia la espera de inactividad
        if (_idleCoroutine != null)
            StopCoroutine(_idleCoroutine);

        _idleCoroutine = StartCoroutine(IdleTimer());

        // Si estaba cambiando colores por inactividad, se detiene
        if (_isIdleChangingColor)
        {
            StopCoroutine(nameof(IdleColorChangeLoop));
            _isIdleChangingColor = false;
        }
    }

    private IEnumerator IdleTimer()
    {
        yield return new WaitForSeconds(_idleDelay);
        _isIdleChangingColor = true;
        StartCoroutine(IdleColorChangeLoop());
    }

    private IEnumerator IdleColorChangeLoop()
    {
        while (_isIdleChangingColor)
        {
            _renderer.color = GetRandomColor(Random.Range(0, 7));
            yield return new WaitForSeconds(_colorChangeInterval);
        }
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
