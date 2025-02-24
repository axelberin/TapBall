using UnityEngine;

[CreateAssetMenu(fileName = "UISkins", menuName = "ScriptableObjects/UISkins")]
public class UISkins : ScriptableObject
{
    public string skinName;
    public Sprite sprite;
    public int price;
}
