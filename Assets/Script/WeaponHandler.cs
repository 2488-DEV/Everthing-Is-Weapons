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
    
        // 1. (Optional) เล่น Effect หรือเสียงอาวุธแตกตรงนี้
        // AudioSource.PlayClipAtPoint(breakSound, transform.position);
    
        // 2. เคลียร์ค่าอาวุธในมือให้เป็น Null (กลายเป็นมือเปล่า)
        currentWeapon = null;
        currentRarity = null;
    
        // 3. อัปเดตภาพให้เป็นมือเปล่า
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