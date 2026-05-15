using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool isBoss;
    [Header("Settings")]
    public GameObject[] stageIToVEnemy;   // ด่าน 1-5
    public GameObject[] stageVIToXEnemy;  // ด่าน 6-10
    public GameObject[] stageXToEnemy;    // ด่าน 11 ขึ้นไป
    public Transform[] spawnPoints;      // จุดที่อยากให้มอนสเตอร์ไปโผล่ (ถ้ามี)
    
    [Header("Boss")]
    public GameObject VStageBoss;
    public GameObject XStageBoss;
    public GameObject LastStageBoss;
    public Transform bossSpawnPoint;

    void Start()
    {
        SpawnEnemiesForStage();
    }

    public void SpawnEnemiesForStage()
    {
        int stage = InGameScript.currentStage;
        GameObject bossToSpawn = null;

        // --- 1. เช็กเงื่อนไขด่านแบบเจาะจงเลข ---
        if (stage == 5 || stage == 15) 
        {
            bossToSpawn = VStageBoss;
        }
        else if (stage == 10 || stage == 20) 
        {
            bossToSpawn = XStageBoss;
        }
        else if (stage == 25) 
        {
            bossToSpawn = LastStageBoss;
        }

        // --- 2. ถ้าเจอเลขด่านที่กำหนด ให้เสก Boss ออกมา ---
        if (bossToSpawn != null)
        {
            if (bossSpawnPoint == null) 
            {
                Debug.LogError("ลืมลากจุดเกิดบอสใส่ช่องใน Inspector!");
                return;
            }

            Vector3 bossPos = bossSpawnPoint.position; 
            Debug.Log("<color=Yellow>พิกัดที่สั่งให้บอสเกิด: </color>" + bossPos); // เช็กเลขตรงนี้ใน Console

            Instantiate(bossToSpawn, bossPos, Quaternion.identity);
            isBoss = true;
            return; 
        }

        // --- 3. เสกลูกน้องตามจำนวนปกติ (Code เดิม) ---
        GameObject[] currentEnemyPool = GetCurrentEnemyPool();
        if (currentEnemyPool == null || currentEnemyPool.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < InGameScript.enemyCount; i++)
        {
            GameObject prefabToSpawn = currentEnemyPool[Random.Range(0, currentEnemyPool.Length)];
            int randomPointIndex = Random.Range(0, spawnPoints.Length);
            Vector3 selectedSpawnPos = spawnPoints[randomPointIndex].position;

            float spawnRadius = 2.5f; 
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 finalPos = new Vector3(selectedSpawnPos.x + randomOffset.x, selectedSpawnPos.y + randomOffset.y, 0);

            Instantiate(prefabToSpawn, finalPos, Quaternion.identity);
        }
    }
    // ฟังก์ชันช่วยเลือกกลุ่มศัตรูตามด่าน
    private GameObject[] GetCurrentEnemyPool()
    {
        int stage = InGameScript.currentStage;

        if (stage >= 1 && stage <= 5) return stageIToVEnemy;
        if (stage >= 6 && stage <= 10) return stageVIToXEnemy;
        if (stage > 10) return stageXToEnemy;

        return stageIToVEnemy; // default
    }
}