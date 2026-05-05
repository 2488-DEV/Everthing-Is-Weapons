using UnityEngine;

[CreateAssetMenu(fileName = "WeaponRarity", menuName = "Scriptable Objects/WeaponRarity")]
public class WeaponRarity : ScriptableObject
{
    public string rarityName;
    public float rarityDamage;
    public float rarityDurability;
    public float rarityChance;
}
