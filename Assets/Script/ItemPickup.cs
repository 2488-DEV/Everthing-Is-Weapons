using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public WeaponInfo weaponData;
    public WeaponRarity weaponRarity;
    private SpriteRenderer sr;

    public float FinalDamage => weaponData.baseDamage + weaponRarity.rarityDamage;

    public void Setup(WeaponInfo info, WeaponRarity rarity)
    {
        weaponData = info;
        weaponRarity = rarity;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null && weaponData != null)
        {
            sr.sprite = weaponData.itemModel;
        }
        Debug.Log($"ไอเทมเซตอัปเรียบร้อย: {weaponData.weaponName} ระดับ {weaponRarity.rarityName}");
    }
}