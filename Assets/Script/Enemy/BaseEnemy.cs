using UnityEngine;
using System.Collections;

public class BaseEnemy : MonoBehaviour
{
    public EnemyInfo enemyInfo;
    private SpriteRenderer sr;
    public bool playerDetect;
    public DetectionArea DetectionArea;
    public bool isGettingHit;
    public bool hitArea;
    public float health;
    public AttackArea AttackArea;
    public GameObject Sense;
    public float waitTimer = 1.0f;
    public float Timer;
    public float attackTimer;
    private Player player;
    private Rigidbody2D rb;
    public float knockbackForce = 2000f;
    
    
    // 1. เพิ่มตัวแปรเพื่อเก็บข้อมูลตำแหน่งของ Player
    public Transform playerTransform; 

    void Start()
    {
        AttackArea.transform.localScale = new Vector3(enemyInfo.attackReach , enemyInfo.attackReach , enemyInfo.attackReach);
        UpdateWeaponVisual();
        health = enemyInfo.health;
        Timer = waitTimer;
        Sense.transform.localPosition = new Vector3(Sense.transform.localPosition.x, Sense.transform.localPosition.y + enemyInfo.senseLocate, Sense.transform.localPosition.z);
        
        attackTimer = enemyInfo.attackSpeed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
        }

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {   
        HealthUpdate();
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        playerDetect = DetectionArea.playerDetect;
        hitArea = AttackArea.hitArea;
        if (playerDetect && playerTransform != null)
        {
            if (Timer > 0) 
            {
                Timer -= Time.deltaTime;
                Sense.GetComponent<SpriteRenderer>().enabled = true;
            }
            if (Timer <= 0) 
            {
                Attack();
                MoveTowardsPlayer();
                Sense.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    void Attack()
    {
        if (hitArea && attackTimer <= 0)
        {
            Debug.Log("Hit Player");
            attackTimer = enemyInfo.attackSpeed;
            player.health -= enemyInfo.attackDamage;

            player.ApplyKnockback(transform.position, 2f, 0.1f);
        }
    }
    void HealthUpdate()
    {   
        if (health <= 0)
        {
            Debug.Log("dead");
            this.gameObject.SetActive(false);
        }
    }
    void MoveTowardsPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        float offset = 2.5f; 

        if (distanceToPlayer > offset)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                playerTransform.position, 
                enemyInfo.speed * Time.deltaTime
            );
        }

        // ส่วนการหันหน้า (Flip) ให้ไว้นอก if เพื่อให้มันหันมอง Player ตลอดเวลาแม้จะหยุดเดินแล้ว
        if (playerTransform.position.x < transform.position.x)
            sr.flipX = false;
        else
            sr.flipX = true;
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

    void ApplyKnockback(Vector3 attackerPos)
    {
        Vector2 direction = (transform.position - attackerPos).normalized;
        // กำหนดเลยว่าอยากให้ถอยไปกี่เมตร (เช่น 2 เมตร)
        Vector2 targetPos = (Vector2)transform.position + (direction * 2f); 

        StartCoroutine(KnockbackLerp(targetPos, 0.1f));
    }

    IEnumerator KnockbackLerp(Vector2 target, float duration)
    {
        float time = 0;
        Vector2 startPos = transform.position;

        while (time < duration)
        {
            // สั่งให้เลื่อนตำแหน่งไปหาเป้าหมายตามเวลาที่ผ่านไป
            transform.position = Vector2.Lerp(startPos, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = target; // มั่นใจว่าหยุดตรงจุดเป้าหมายเป๊ะ
    }
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("PlayerAttackHitBox"))
        {
            // ใช้ตัวแปรนี้เพื่อเช็กว่า "ของฝั่งเรา" ที่โดนชนคืออันไหน
            // เราจะดึง Collider ของตัวแม่ (BaseEnemy) มาเทียบ
            Collider2D myMainCollider = GetComponent<Collider2D>();

            // เช็กว่าอาวุธของผู้เล่น กำลังแตะอยู่กับ Collider หลักของตัวแม่หรือไม่
            if (collision.IsTouching(myMainCollider))
            {
                if (!isGettingHit)
                {
                    Debug.Log("<color=Red>โดนตัวเน้นๆ!</color>");
                    health -= player.attackDamage;
                    isGettingHit = true;
                    ApplyKnockback(collision.transform.position);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("PlayerAttackHitBox"))
        {
            isGettingHit = false;
        }
    }
}
