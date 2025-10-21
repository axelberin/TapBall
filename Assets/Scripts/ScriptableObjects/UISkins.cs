using UnityEngine;

[CreateAssetMenu(fileName = "UISkins", menuName = "ScriptableObjects/UISkins")]
public class UISkins : ScriptableObject
{
    public string skinName;
    public Sprite sprite;
    public int price;
    public Vector2 spriteSize;
    public bool unlockeable;
    public Sprite backgroundSprite;
    public RuntimeAnimatorController backgroundAnimator;
}
