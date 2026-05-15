using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("Current Weapon Data")]
    public WeaponInfo currentWeapon;
    public WeaponRarity currentRarity;
    public float currentDurability;

    [Header("Visuals")]
    private SpriteRenderer sr;

    // Property คำนวณค่าความทนทานสูงสุดเสมอ
    public float MaxDurability => (currentWeapon != null)
        ? currentWeapon.baseDurability + (currentRarity != null ? currentRarity.rarityDurability : 0)
        : 0;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetWeapon(WeaponInfo newWeapon, WeaponRarity newRarity, float durability)
    {
        currentWeapon = newWeapon;
        currentRarity = newRarity;

        // คุมค่าความทนทานไม่ให้เกิน Max
        currentDurability = Mathf.Clamp(durability, 0, MaxDurability);

        UpdateWeaponVisual();
    }

    // ฟังก์ชันลดความทนทาน (เรียกใช้จาก Boss หรือ Enemy เวลาโดนตี)
    public void DecreaseDurability(float amount)
    {
        if (currentWeapon == null) return;

        currentDurability -= amount;

        if (currentDurability <= 0)
        {
            currentDurability = 0;
            BreakWeapon();
        }
    }

    public void ThrowWeapon()
    {
        if (currentWeapon == null) return;

        Debug.Log($"<color=orange>[System]</color> Threw <color=yellow>{currentWeapon.weaponName}</color>!");

        currentWeapon = null;
        currentRarity = null;
        currentDurability = 0;

        Player player = GetComponentInParent<Player>();
        if (player != null)
            player.ResetAttack();

        UpdateWeaponVisual();
    }

    void BreakWeapon()
    {
        if (currentWeapon == null) return;

        Debug.Log($"<color=orange>[System]</color> <color=red>{currentWeapon.weaponName} พังยับเยิน!</color>");

        currentWeapon = null;
        currentRarity = null;

        Player player = GetComponentInParent<Player>();
        if (player != null)
        {
            player.ResetAttack();
        }

        UpdateWeaponVisual();
    }

    public void UpdateWeaponVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (currentWeapon == null)
        {
            sr.sprite = null;
            // ล้างสี (ถ้ามีการใส่สีตาม Rarity ไว้ก่อนหน้า)
            sr.color = Color.white;
            return;
        }

        sr.sprite = currentWeapon.itemModel;

        // --- อัปเกรด: ใส่สีตาม Rarity (ถ้ามีข้อมูลสีใน SO) ---
        /*
        if (currentRarity != null) {
            sr.color = currentRarity.rarityColor;
        }
        */
    }
}