using UnityEngine;
public class BaseEnemy : MonoBehaviour
{
    public EnemyInfo enemyInfo;
    private SpriteRenderer sr;
    public bool playerDetect;
    
    // 1. เพิ่มตัวแปรเพื่อเก็บข้อมูลตำแหน่งของ Player
    public Transform playerTransform; 

    void Start()
    {
        UpdateWeaponVisual();
    }

    void Update()
    {
        // 2. ถ้าเจอ Player ให้เดินไปหา
        if (playerDetect && playerTransform != null)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        // คำนวณทิศทาง และขยับตำแหน่งโดยอิงจาก speed ใน enemyInfo
        // ใช้ Vector2.MoveTowards เพื่อให้การเคลื่อนที่นุ่มนวล
        transform.position = Vector2.MoveTowards(
            transform.position, 
            playerTransform.position, 
            enemyInfo.speed * Time.deltaTime
        );

        // (แถม) เจมส์สามารถเช็กให้ศัตรูหันหน้าไปทาง Player ได้ด้วยนะ
        if (playerTransform.position.x < transform.position.x)
            sr.flipX = false; // หันซ้าย
        else
            sr.flipX = true; // หันขวา
    }
    void UpdateWeaponVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (enemyInfo == null) 
        {
            sr.sprite = null; 
            return;
        }

        sr.sprite = enemyInfo.enemyModel;
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=Red>Player entered detectionArea!</color>");
            playerDetect = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=Red>Player exited detectionArea!</color>");
            playerDetect = false;
        }
    }
}
