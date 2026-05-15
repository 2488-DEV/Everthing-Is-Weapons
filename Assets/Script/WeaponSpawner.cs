using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject itemPrefab;
    public WeaponInfo[] weaponPool;    // รายชื่ออาวุธ
    public WeaponRarity[] rarityPool;  // SO Rarity (Common-Mythical)

    [Header("Drop Rate")]
    [Range(0f, 1f)]
    public float emptyChance = 0.3f;   // โอกาสที่จะไม่เกิดของ (30%)

    void Start()
    {
        SpawnWeapon();
        // ทำลายตัว Spawner ทิ้งทันทีเพื่อประหยัด RAM
        Destroy(gameObject);
    }

    void SpawnWeapon()
    {
        // 1. เช็กดวงเบื้องต้น (70% ที่จะผ่าน)
        if (Random.value <= emptyChance)
        {
            Debug.Log("<color=#808080>[Spawner]</color> <color=red>เกลือ!</color> จุดนี้ไม่มีของดรอป");
            return;
        }

        // 2. ตรวจสอบความพร้อมของไฟล์ (Validation)
        if (itemPrefab == null || weaponPool == null || weaponPool.Length == 0 || rarityPool == null || rarityPool.Length == 0)
        {
            Debug.LogWarning("[Spawner] ขาดการตั้งค่า Prefab หรือ Pool ใน Inspector!");
            return;
        }

        // 3. สุ่มระดับความหายาก และ ตัวอาวุธ
        WeaponRarity selectedRarity = GetWeightedRarity();
        WeaponInfo baseWeapon = weaponPool[Random.Range(0, weaponPool.Length)];

        // 4. ทำการสร้างไอเทม (Instantiate)
        if (baseWeapon != null && selectedRarity != null)
        {
            GameObject spawnedItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            ItemPickup pickupScript = spawnedItem.GetComponent<ItemPickup>();

            if (pickupScript != null)
            {
                pickupScript.Setup(baseWeapon, selectedRarity);
                // แสดงผลใน Console ด้วยสีตามระดับความหายาก (ถ้าตั้งสีไว้ใน SO)
                Debug.Log($"<color=white>เสกสำเร็จ!</color> <color=yellow>{baseWeapon.weaponName}</color> [<color=cyan>{selectedRarity.rarityName}</color>]");
            }
        }
    }

    // ระบบสุ่มแบบถ่วงน้ำหนัก (Weighted Random)
    WeaponRarity GetWeightedRarity()
    {
        float totalWeight = 0;

        // คำนวณผลรวมของโอกาสทั้งหมดที่มีใน Pool
        foreach (WeaponRarity r in rarityPool)
        {
            if (r != null) totalWeight += r.rarityChance;
        }

        // ถ้าลืมตั้งค่าโอกาสดรอปเลย ให้คืนค่าอันแรกสุดไปก่อน
        if (totalWeight <= 0) return rarityPool[0];

        float randomPoint = Random.Range(0, totalWeight);
        float currentTracker = 0;

        foreach (WeaponRarity r in rarityPool)
        {
            if (r == null) continue;

            currentTracker += r.rarityChance;
            if (randomPoint <= currentTracker)
            {
                return r;
            }
        }

        return rarityPool[0];
    }
}