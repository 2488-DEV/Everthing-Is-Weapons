using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool isBoss;

    [Header("Enemy Pools")]
    public GameObject[] stageIToVEnemy;   // ด่าน 1-5
    public GameObject[] stageVIToXEnemy;  // ด่าน 6-10
    public GameObject[] stageXToEnemy;    // ด่าน 11 ขึ้นไป
    public Transform[] spawnPoints;

    [Header("Boss Settings")]
    public GameObject VStageBoss;     // บอสตัวที่ 1 (ด่าน 5, 15)
    public GameObject XStageBoss;     // บอสตัวที่ 2 (ด่าน 10, 20)
    public GameObject LastStageBoss;  // บอสใหญ่ (ด่าน 25)
    public Transform bossSpawnPoint;

    void Start()
    {
        SpawnEnemiesForStage();
    }

    public void SpawnEnemiesForStage()
    {
        int stage = InGameScript.currentStage;
        GameObject bossToSpawn = null;

        // 1. ตรวจสอบเงื่อนไขด่านบอส (Logic 5, 10, 15, 20, 25)
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

        // 2. ถ้าเป็นด่านบอส
        if (bossToSpawn != null)
        {
            SpawnBoss(bossToSpawn);
            isBoss = true;
            return; // หยุดการทำงาน ไม่ต้องเสกลูกน้องปกติ
        }

        // 3. ถ้าเป็นด่านปกติ ให้เสกลูกน้อง
        isBoss = false;
        SpawnNormalEnemies();
    }

    void SpawnBoss(GameObject bossPrefab)
    {
        if (bossSpawnPoint == null)
        {
            Debug.LogError("[Spawner] ลืมใส่ Boss Spawn Point!");
            return;
        }

        Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
        Debug.Log($"<color=red>BOSS APPEARED!</color> Stage: {InGameScript.currentStage}");
    }

    void SpawnNormalEnemies()
    {
        GameObject[] currentEnemyPool = GetCurrentEnemyPool();

        // เช็กความพร้อมของข้อมูล
        if (currentEnemyPool == null || currentEnemyPool.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[Spawner] ข้อมูล Pool หรือ SpawnPoints ไม่ครบ!");
            return;
        }

        for (int i = 0; i < InGameScript.enemyCount; i++)
        {
            // สุ่มเลือกชนิดศัตรูและจุดเกิด
            GameObject prefabToSpawn = currentEnemyPool[Random.Range(0, currentEnemyPool.Length)];
            Transform selectedPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // ปรับระยะห่างในการเกิดรอบๆ จุด (Random Offset)
            Vector2 randomCircle = Random.insideUnitCircle * 2.0f;
            Vector3 finalPos = selectedPoint.position + new Vector3(randomCircle.x, randomCircle.y, 0);

            Instantiate(prefabToSpawn, finalPos, Quaternion.identity);
        }

        Debug.Log($"<color=cyan>Enemies Spawned:</color> {InGameScript.enemyCount} units");
    }

    private GameObject[] GetCurrentEnemyPool()
    {
        int stage = InGameScript.currentStage;

        if (stage <= 5) return stageIToVEnemy;
        if (stage <= 10) return stageVIToXEnemy;
        return stageXToEnemy;
    }
}