using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "Scriptable Objects/WeaponInfo")]
public class WeaponInfo : ScriptableObject
{
    public string weaponName;
    public float baseDamage; 
    public float baseDurability; 
    public WeaponRarity weaponRarity;
    public Sprite itemModel;
    public enum WeaponType {Melee , Range}
    public WeaponType weaponType;

    public float TotalDamage => baseDamage + (weaponRarity != null ? weaponRarity.rarityDamage : 0);
    
    public float attackSpeed;
    public float attackReach;
    [Header("Ranged")]
    public float projectileSpeed = 15f;
    public float projectileLifetime = 3f;
    public float MaxDurability =>
        baseDurability + (weaponRarity != null ? weaponRarity.rarityDurability : 0);
}