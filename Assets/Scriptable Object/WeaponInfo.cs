using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "Scriptable Objects/WeaponInfo")]
public class WeaponInfo : ScriptableObject
{
    public string weaponName;
    public float baseDamage; 
    public float baseDurability; 
    public WeaponType weaponType;
    public WeaponRarity weaponRarity;
    public Sprite itemModel;

    public float TotalDamage => baseDamage + (weaponRarity != null ? weaponRarity.rarityDamage : 0);
    
    public float AttackSpeed => weaponType != null ? weaponType.baseAttackSpeed : 0;
    public float AttackReach => weaponType != null ? weaponType.baseReach : 0;
    public float MaxDurability => 
        baseDurability + (weaponRarity != null ? weaponRarity.rarityDurability : 0);
}