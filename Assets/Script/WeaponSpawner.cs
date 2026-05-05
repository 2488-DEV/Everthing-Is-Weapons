using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject itemPrefab; 
    public WeaponInfo[] weaponPool;    // ลาก "เบสบอล", "ไม้ที" ใส่แค่อย่างละอัน
    public WeaponRarity[] rarityPool;  // ลาก SO Rarity (Common-Mythical) ทั้ง 6 อันใส่ที่นี่

    void Start()
    {
        SpawnWeapon();
        Destroy(gameObject); 
    }

    void SpawnWeapon()
    {
        if (weaponPool.Length == 0 || rarityPool.Length == 0 || itemPrefab == null) return;

        WeaponRarity selectedRarity = GetWeightedRarity();

        WeaponInfo baseWeapon = weaponPool[Random.Range(0, weaponPool.Length)];

        if (baseWeapon != null && selectedRarity != null)
        {
            GameObject spawnedItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            ItemPickup pickupScript = spawnedItem.GetComponent<ItemPickup>();
            
            if (pickupScript != null)
            {
                pickupScript.Setup(baseWeapon, selectedRarity);
                Debug.Log($"<color=cyan>เสกสำเร็จ!</color> {baseWeapon.weaponName} [{selectedRarity.rarityName}]");
            }
        }
    }

    WeaponRarity GetWeightedRarity()
    {
        float totalChance = 0;

        foreach (WeaponRarity r in rarityPool) 
        {
            totalChance += r.rarityChance;
        }

        float randomPoint = Random.Range(0, totalChance);
        float currentTracker = 0;

        foreach (WeaponRarity r in rarityPool)
        {
            currentTracker += r.rarityChance;
            if (randomPoint <= currentTracker)
            {
                return r;
            }
        }
        return rarityPool[0]; // 
    }
}