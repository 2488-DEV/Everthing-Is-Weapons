using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public WeaponInfo currentWeapon;
    public WeaponRarity currentRarity;
    private SpriteRenderer sr;
    
    public float currentDurability;

    // สร้างเป็น Property เพื่อให้เรียกใช้ง่ายและคำนวณค่าใหม่เสมอ
    public float MaxDurability => (currentWeapon != null) 
        ? currentWeapon.baseDurability + (currentRarity != null ? currentRarity.rarityDurability : 0) 
        : 0;

    public void SetWeapon(WeaponInfo newWeapon, WeaponRarity newRarity, float durability)
    {
        currentWeapon = newWeapon;
        currentRarity = newRarity; 
        currentDurability = durability;
        
        UpdateWeaponVisual();
    }

    // ฟังก์ชันสำหรับลดความทนทานตอนโจมตี
    public void DecreaseDurability(float amount)
    {
        if (currentWeapon == null) return;
    
        currentDurability -= amount;
        
        if (currentDurability <= 0)
        {
            currentDurability = 0;
            BreakWeapon(); // 👈 เรียกฟังก์ชันพังอาวุธ
        }
    }
    
    void BreakWeapon()
    {
        Debug.Log($"{currentWeapon.weaponName} พังแล้ว!");

        currentWeapon = null;
        currentRarity = null;

        // สั่งให้มือหยุดเหวี่ยง (ไปดึง Script Player มา Reset)
        // ถ้าสคริปต์นี้อยู่ที่เดียวกับ Player ใช้ GetComponent ได้เลย
        Player player = GetComponentInParent<Player>(); 
        if(player != null) {
            player.ResetAttack(); 
        }

        UpdateWeaponVisual();
    }
    void UpdateWeaponVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (currentWeapon == null) 
        {
            sr.sprite = null; 
            return;
        }

        sr.sprite = currentWeapon.itemModel;
    }
    
}