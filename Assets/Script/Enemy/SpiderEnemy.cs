using UnityEngine;
using System.Collections;

public class SpiderEnemy : MonoBehaviour
{
    [Header("References")]
    public EnemyInfo enemyInfo;
    public Transform weaponHandler;
    public DetectionArea DetectionArea;
    public AttackArea AttackArea;
    public GameObject Sense;

    [Header("Settings")]
    public float waitTimer = 1.0f;
    public float knockbackForce = 2000f;
    public float offsetY = 1f;

    [Header("Web Shooting")]
    public GameObject webPrefab;
    public float webShootCooldown = 2f;

    // Internal Variables
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Player player;
    private Transform playerTransform;
    private WeaponHandler weaponHandlerScript;

    [Header("Status (Read Only)")]
    public float health;
    public float maxHealth;
    public float attackDamage;
    public float expGiven;
    public float Timer;
    public float attackTimer;
    public bool playerDetect;
    public bool isGettingHit;
    public bool hitArea;

    void Start()
    {
        // 1. Initial References
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (weaponHandler != null) weaponHandlerScript = weaponHandler.GetComponent<WeaponHandler>();

        // 2. Setup Stats จาก enemyInfo และ Scaling ตามด่าน
        if (enemyInfo != null)
        {
            health = enemyInfo.health;
            attackDamage = enemyInfo.attackDamage;
            expGiven = enemyInfo.expGiven;

            if (InGameScript.currentStage > 0)
            {
                health *= Mathf.Pow(1.025f, InGameScript.currentStage - 1);
                attackDamage += 0.5f * (InGameScript.currentStage - 1);
                expGiven *= Mathf.Pow(1.05f, InGameScript.currentStage - 1);
            }

            maxHealth = health;
            attackTimer = enemyInfo.attackSpeed;

            // ปรับระยะ AttackArea ตามข้อมูลอาวุธ
            if (AttackArea != null)
                AttackArea.transform.localScale = Vector3.one * enemyInfo.attackReach;

            // ตั้งตำแหน่ง Sense (เครื่องหมายตกใจ)
            if (Sense != null)
                Sense.transform.localPosition = new Vector3(Sense.transform.localPosition.x, Sense.transform.localPosition.y + enemyInfo.senseLocate, Sense.transform.localPosition.z);
        }

        maxHealth = health;
        Timer = waitTimer;

        // 3. หา Player แค่ครั้งเดียวตอนเริ่ม (ถ้าหาไม่เจอค่อยหาใหม่ใน Update แบบปลอดภัย)
        FindPlayer();
        UpdateWeaponVisual();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        // ถ้าไม่มี Player ใน Reference ให้พยายามหาใหม่ (เผื่อ Spawn มาทีหลัง)
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // Logic ป้องกันการโดนตีซ้ำซ้อน
        if (player.attackTimer <= 0) isGettingHit = false;

        HealthUpdate();

        // Cooldown การโจมตี
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        // ดึงสถานะจาก Sensor Areas
        playerDetect = DetectionArea.playerDetect;
        hitArea = AttackArea.hitArea;

        // AI Logic
        if (playerDetect)
        {
            if (Timer > 0)
            {
                Timer -= Time.deltaTime;
                if (Sense != null) Sense.GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                ShootWeb();
                Attack();
                MoveAwayFromPlayer(); // เดินหนี (Kiting)
                if (Sense != null) Sense.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    void ShootWeb()
    {
        if (webPrefab == null || attackTimer > 0) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Vector2 spawnPos = (Vector2)transform.position + direction * 0.5f;

        GameObject web = Instantiate(webPrefab, spawnPos, Quaternion.identity);
        web.SetActive(true);

        WebProjectile webScript = web.GetComponent<WebProjectile>();
        if (webScript != null)
            webScript.SetDirection(direction);

        attackTimer = webShootCooldown;
    }

    void Attack()
    {
        if (hitArea && attackTimer <= 0)
        {
            attackTimer = enemyInfo.attackSpeed;
            player.health -= attackDamage;
            player.ApplyKnockback(transform.position, 2f, 0.1f);
            Debug.Log("<color=orange>BlackEnemy:</color> ตบผู้เล่นสำเร็จ!");
        }
    }

    void HealthUpdate()
    {
        if (health <= 0)
        {
            player.currentEXP += expGiven;
            gameObject.SetActive(false);
        }
    }

    void MoveAwayFromPlayer()
    {
        // คำนวณทิศทางหนี (ถอยหลังจาก Player)
        Vector2 directionAway = (transform.position - playerTransform.position).normalized;
        Vector2 targetPos = (Vector2)transform.position + directionAway;

        transform.position = Vector2.MoveTowards(transform.position, targetPos, enemyInfo.speed * Time.deltaTime);

        // หันหน้าจ้อง Player ตลอดเวลา (Moonwalk)
        if (sr != null) sr.flipX = (playerTransform.position.x >= transform.position.x);
    }

    void UpdateWeaponVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (enemyInfo != null && sr != null) sr.sprite = enemyInfo.enemyModel;
    }

    public void ApplyKnockback(Vector3 attackerPos)
    {
        StopCoroutine("KnockbackLerp"); // หยุดตัวเก่าก่อนถ้ามี
        Vector2 direction = (transform.position - attackerPos).normalized;
        Vector2 targetPos = (Vector2)transform.position + (direction * 2f);
        StartCoroutine(KnockbackLerp(targetPos, 0.1f));
    }

    IEnumerator KnockbackLerp(Vector2 target, float duration)
    {
        float time = 0;
        Vector2 startPos = transform.position;
        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPos, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttackHitBox"))
        {
            Collider2D myMainCollider = GetComponent<Collider2D>();
            if (collision.IsTouching(myMainCollider) && !isGettingHit)
            {
                float totalDamage = player.attackDamage + (player.attackDamage * (player.bonusAttackDamage / 100));
                health -= totalDamage;
                isGettingHit = true;

                ApplyKnockback(collision.transform.position);
                if (weaponHandlerScript != null) weaponHandlerScript.DecreaseDurability(enemyInfo.durabilityCost);

                Debug.Log($"<color=red>BlackEnemy:</color> โดนตบไป {totalDamage}");
            }
        }
    }
}