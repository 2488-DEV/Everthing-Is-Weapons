using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public WeaponInfo weaponData;
    public WeaponRarity weaponRarity;
    public WeaponHandler playerWeapon;
    private SpriteRenderer sr;
    public float currentDurability;
    public float FinalDamage => weaponData.baseDamage + weaponRarity.rarityDamage;

    [Header("Visual Settings")]
    public float pulseSpeed = 2f;      // ความเร็วในการกระพริบ
    public float minAlpha = 0.1f;

    void Start()
    {
        if (currentDurability <= 0 && weaponData != null)
        {
            float bonus = (weaponRarity != null) ? weaponRarity.rarityDurability : 0;
            currentDurability = weaponData.baseDurability + bonus;
        }
    }
    void Update()
    {
        if (weaponRarity != null && sr != null)
        {
            ApplyPulsingColor();
        }
    }

    void ApplyPulsingColor()
    {
        // 1. ปรับความเร็ว (Speed)
        // ถ้าอยากให้ครบรอบ (ไป-กลับ) ในเวลาประมาณ 2 วินาที ให้ใช้ค่าประมาณ 0.5f
        // สูตร: 1 / (วินาทีที่ต้องการ) = pulseSpeed
        float pulseSpeed = 0.5f; 
        float lerp = Mathf.PingPong(Time.time * pulseSpeed, 1f);

        // 2. กำหนดสีขาวเป็นสีหลัก
        Color baseColor = Color.white;

        // 3. ดึงสี Rarity มา
        Color targetColor = weaponRarity.rarityColor;

        // 4. ผสมสีโดยจำกัดความเข้มสูงสุดที่ 0.2 (20%)
        // แทนที่จะผสมไปจนถึงสี Rarity 100% เราจะผสมไปแค่ 0.2f พอ
        float maxIntensity = 0.3f;
        Color finalColor = Color.Lerp(baseColor, targetColor, lerp * maxIntensity);

        // ล็อก Alpha ของ Sprite ให้ทึบ 100% ตลอดเวลา
        finalColor.a = 1f; 

        sr.color = finalColor;
    }

    public void SetWeapon(WeaponInfo newInfo, WeaponRarity newRarity, float durability) // รับมาทั้งคู่
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        weaponData = newInfo;
        weaponRarity = newRarity;
        currentDurability = durability;

        if (weaponData != null) sr.sprite = weaponData.itemModel;
    }
    public void Setup(WeaponInfo info, WeaponRarity rarity)
    {
        weaponData = info;
        weaponRarity = rarity;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null && weaponData != null)
        {
            sr.sprite = weaponData.itemModel;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            WeaponHandler playerCurrentWeapon = collision.gameObject.GetComponentInChildren<WeaponHandler>();

            playerWeapon = playerCurrentWeapon;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerWeapon = null;
        }
    }
}