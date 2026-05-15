using UnityEngine;

public class LevelInitializer : MonoBehaviour
{
    [Header("Settings")]
    public Transform spawnPoint; // ลาก Object จุดเกิดมาใส่ช่องนี้

    void Start()
    {
        // ค้นหา Player ตัวที่เป็น Singleton (ตัวที่เหลือรอดมาได้)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null && spawnPoint != null)
        {
            // 1. วาร์ปตัวละครไปที่จุดเกิดทันที
            playerObj.transform.position = spawnPoint.position;

            // 2. หยุดแรงเฉื่อยเผื่อตัวละครพุ่งมาแรงๆ จากด่านที่แล้ว
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            Debug.Log($"<color=cyan>Level Initialized:</color> Player warped to {spawnPoint.name}");
        }
        else
        {
            if (spawnPoint == null) Debug.LogWarning("อย่าลืมลากจุดเกิดมาใส่ใน LevelInitializer ด้วยนะเพื่อน!");
        }
    }
}