using System.Collections;
using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    [SerializeField] float _activeTime = 3f;
    private bool _isActive = false;

    public void Select()
    {
        if (!_isActive)
            Initialize();
    }

    public abstract void Initialize();

    public abstract void StopPowerUpEffect();

    public IEnumerator EffectActiveCoroutine()
    {
        _isActive = true;
        yield return new WaitForSecondsRealtime(_activeTime);
        StopPowerUpEffect();
        _isActive = false;
    }
}
