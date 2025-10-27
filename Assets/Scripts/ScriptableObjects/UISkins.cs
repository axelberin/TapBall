using UnityEngine;

[CreateAssetMenu(fileName = "UISkins", menuName = "ScriptableObjects/UISkins")]
public class UISkins : ScriptableObject
{
    public string skinName;
    public Sprite sprite;
    public SkinPriceByGroup groupPrice;
    public int skinAddPrice;
    public Vector2 spriteSize;
    public bool unlockeable;
    public Sprite backgroundSprite;
    public RuntimeAnimatorController backgroundAnimator;

    public int GetPrice => unlockeable ? 0 : groupPrice.Price + skinAddPrice;
}
