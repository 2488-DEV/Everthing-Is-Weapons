using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponInfo weaponData;
    public WeaponRarity weaponRarity;
    public float currentDurability;

    [Header("Settings")]
    public float pulseSpeed = 0.5f;
    public float maxIntensity = 0.3f;

    private SpriteRenderer sr;
    private WeaponHandler playerWeapon; // สำหรับอ้างอิงเฉยๆ (Optional)

    // อัปเกรด: เพิ่ม Null Check ให้ Property เพื่อความปลอดภัย
    public float FinalDamage => (weaponData != null && weaponRarity != null)
        ? weaponData.baseDamage + weaponRarity.rarityDamage
        : 0;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // ถ้าค่าความทนทานยังไม่ได้ถูกตั้งมาจาก Spawner ให้คำนวณใหม่ตาม Stat
        if (currentDurability <= 0 && weaponData != null)
        {
            float bonus = (weaponRarity != null) ? weaponRarity.rarityDurability : 0;
            currentDurability = weaponData.baseDurability + bonus;
        }

        // อัปเดตภาพทันทีเมื่อเริ่มเกม
        if (weaponData != null && sr != null)
        {
            sr.sprite = weaponData.itemModel;
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
        // 1. คำนวณจังหวะการกระพริบ
        float lerp = Mathf.PingPong(Time.time * pulseSpeed, 1f);

        // 2. ผสมสีขาว (Base) เข้ากับสี Rarity ตามความเข้มที่กำหนด
        Color baseColor = Color.white;
        Color targetColor = weaponRarity.rarityColor;

        // ผสมสีโดยใช้ maxIntensity เพื่อไม่ให้สี Rarity กลบรายละเอียดของ Sprite จนมิด
        Color finalColor = Color.Lerp(baseColor, targetColor, lerp * maxIntensity);

        // ล็อก Alpha ให้ชัดเจนเสมอ
        finalColor.a = 1f;
        sr.color = finalColor;
    }

    // ฟังก์ชันสำหรับ "เปลี่ยนข้อมูลไอเทม" (เช่น ตอนโยนของจากมือลงพื้น)
    public void SetWeapon(WeaponInfo newInfo, WeaponRarity newRarity, float durability)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        weaponData = newInfo;
        weaponRarity = newRarity;
        currentDurability = durability;

        if (weaponData != null) sr.sprite = weaponData.itemModel;
    }

    // ฟังก์ชันสำหรับ "ตั้งค่าครั้งแรก" (เรียกจาก Spawner)
    public void Setup(WeaponInfo info, WeaponRarity rarity)
    {
        weaponData = info;
        weaponRarity = rarity;

        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (sr != null && weaponData != null)
        {
            sr.sprite = weaponData.itemModel;
        }

        // ตั้งค่าความทนทานเริ่มต้น
        float bonus = (weaponRarity != null) ? weaponRarity.rarityDurability : 0;
        currentDurability = weaponData.baseDurability + bonus;
    }

    // ระบบ Trigger เพื่อบอกให้ Player รู้ว่า "เก็บบวกสลับ" ของชิ้นนี้ได้
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // แค่เก็บอ้างอิงไว้เฉยๆ การเก็บของจริงจะถูกคุมโดยสคริปต์ Player
            playerWeapon = collision.GetComponentInChildren<WeaponHandler>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerWeapon = null;
        }
    }
}