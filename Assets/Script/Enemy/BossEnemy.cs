using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Collections;

public class BossEnemy : MonoBehaviour
{
    public Transform playerTransform; 

    public EnemyInfo enemyInfo;
    public Transform weaponHandler;
    private SpriteRenderer sr;
    public bool playerDetect;
    public DetectionArea DetectionArea;
    public bool isGettingHit;
    public bool hitArea;
    public float health;
    public float maxHealth;
    public float attackDamage;
    public AttackArea AttackArea;
    public GameObject Sense;
    public float waitTimer = 1.0f;
    public float Timer;
    public float attackTimer;
    public float expGiven;
    private Player player;
    private Rigidbody2D rb;
    public float knockbackForce = 2000f;
    public WeaponHandler weaponHandlerScript;
    
    [Header("BossMove")]
    public GameObject[] hitBox;
    public GameObject[] VFX;

    // 1. เพิ่มตัวแปรเพื่อเก็บข้อมูลตำแหน่งของ Player
    
    void Start()
    {
        
        weaponHandlerScript = weaponHandler.GetComponent<WeaponHandler>();
        AttackArea.transform.localScale = new Vector3(enemyInfo.attackReach , enemyInfo.attackReach , enemyInfo.attackReach);
        UpdateWeaponVisual();
        health = enemyInfo.health;
        attackDamage = enemyInfo.attackDamage;
        expGiven = enemyInfo.expGiven;
        if (InGameScript.currentStage > 0)
        {
            health = enemyInfo.health * Mathf.Pow(1.025f, InGameScript.currentStage - 1);
            attackDamage += 0.5f * (InGameScript.currentStage - 1);
            expGiven = enemyInfo.expGiven * Mathf.Pow(1.05f, InGameScript.currentStage - 1);
        }
        maxHealth = health;
        Timer = waitTimer;
        Sense.transform.localPosition = new Vector3(Sense.transform.localPosition.x, Sense.transform.localPosition.y + enemyInfo.senseLocate, Sense.transform.localPosition.z);
        
        attackTimer = enemyInfo.attackSpeed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
            playerTransform = playerObj.transform;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {   
        if (player.attackTimer <= 0)
        {
            isGettingHit = false;
        }
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
                Sense.GetComponent<SpriteRenderer>().enabled = false;
                LookAtPlayer();
                if (attackTimer <= 0)
                {
                    BossFirstMove();
                }
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
            player.currentEXP += enemyInfo.expGiven;
        }
    }

    void MoveTowardsPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        float offset = enemyInfo.offset; 

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
    void LookAtPlayer()
    {
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
                    health -= player.attackDamage + (player.attackDamage * (player.bonusAttackDamage/100));
                    Debug.Log("เสียเลือดไป : " + (player.attackDamage + (player.attackDamage * (player.bonusAttackDamage/100))));
                    isGettingHit = true;
                    ApplyKnockback(collision.transform.position);
                    weaponHandlerScript.DecreaseDurability(enemyInfo.durabilityCost);
                }
            }
        }
    }
    
    void BossFirstMove()
    {
        if (playerTransform == null) return;

        GameObject paperPrefab = null;

        foreach (GameObject go in VFX)
        {
            // --- เพิ่มบรรทัดนี้เพื่อกัน Error ครับเจมส์ ---
            if (go == null) continue; 
            // ---------------------------------------
    
            if (go.name == "Paper") 
            {
                paperPrefab = go;
                break;
            }
        }

        if (paperPrefab == null)
        {
            Debug.LogError("หา VFX ชื่อ Paper ไม่เจอจ้าเจมส์");
            return;
        }

        // 2. เสกกระดาษออกมาจาก "กลางตัวบอส" (transform.position)
        GameObject projectile = Instantiate(paperPrefab, transform.position, Quaternion.identity);

        // 3. คำนวณทิศทางจาก "กลางตัวบอส" ไปหา Player
        Vector2 shootDirection = (Vector2)playerTransform.position - (Vector2)transform.position;

        // 4. ส่งทิศทางไปให้กระดาษพุ่ง
        PaperProjectile paperScript = projectile.GetComponent<PaperProjectile>();
        if (paperScript != null)
        {
            paperScript.SetDirection(shootDirection);
        }

        attackTimer = 2;
    }
}
