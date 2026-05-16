using UnityEngine;
using System.Collections;

public class BlackEnemy : MonoBehaviour
{
    [Header("References")]
    public EnemyInfo enemyInfo;
    public Transform weaponHandler;
    public DetectionArea DetectionArea;
    public AttackArea AttackArea;
    public GameObject Sense;
    public GameObject Hand;

    [Header("Settings")]
    public float handSpeed;
    public float waitTimer = 1.0f;
    public float knockbackForce = 2000f;
    public float offsetY = 1f;

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
                Attack();
                MoveAwayFromPlayer(); // เดินหนี (Kiting)
                if (Sense != null) Sense.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        // Logic การส่งมือไปตบ
        if (DetectionArea.handDetect && Timer <= 0)
        {
            MoveTowardsPlayer();
        }
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
            InGameScript.currentEnemyCount -= 1;
            gameObject.SetActive(false);
        }
    }

    void MoveTowardsPlayer()
    {
        if (Hand == null) return;

        // ส่งมือไปที่ตำแหน่ง Player + OffsetY
        Vector2 targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y + offsetY);
        Hand.transform.position = Vector2.MoveTowards(Hand.transform.position, targetPosition, handSpeed * Time.deltaTime);

        // หันหน้าศัตรูตามเป้าหมายของมือ
        if (sr != null) sr.flipX = (targetPosition.x >= transform.position.x);
    }

    void MoveAwayFromPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float retreatDistance = GetRetreatDistance();

        if (distanceToPlayer >= retreatDistance) return;
        if (IsNearMapEdge()) return;

        Vector2 directionAway = (transform.position - playerTransform.position).normalized;
        Vector2 targetPos = (Vector2)transform.position + directionAway;
        targetPos = ClampToCameraBounds(targetPos);

        transform.position = Vector2.MoveTowards(transform.position, targetPos, enemyInfo.speed * Time.deltaTime);

        if (sr != null) sr.flipX = (playerTransform.position.x >= transform.position.x);
    }

    float GetRetreatDistance()
    {
        Camera cam = Camera.main;
        if (cam != null)
            return cam.orthographicSize * 0.7f;
        return 3f;
    }

    bool IsNearMapEdge()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, -cam.transform.position.z));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, -cam.transform.position.z));

        float wallRadius = 2f;
        Vector2 pos = transform.position;

        if (pos.x - bottomLeft.x < wallRadius) return true;
        if (topRight.x - pos.x < wallRadius) return true;
        if (pos.y - bottomLeft.y < wallRadius) return true;
        if (topRight.y - pos.y < wallRadius) return true;

        return false;
    }

    Vector2 ClampToCameraBounds(Vector2 position)
    {
        Camera cam = Camera.main;
        if (cam == null) return position;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, -cam.transform.position.z));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, -cam.transform.position.z));

        float margin = 0.5f;
        float clampedX = Mathf.Clamp(position.x, bottomLeft.x + margin, topRight.x - margin);
        float clampedY = Mathf.Clamp(position.y, bottomLeft.y + margin, topRight.y - margin);
        return new Vector2(clampedX, clampedY);
    }

    void UpdateWeaponVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (enemyInfo != null && sr != null) sr.sprite = enemyInfo.enemyModel;
    }

    public void ApplyKnockback(Vector3 attackerPos)
    {
        StopCoroutine("KnockbackLerp");
        Vector2 direction = (transform.position - attackerPos).normalized;
        Vector2 targetPos = (Vector2)transform.position + (direction * 2f);
        targetPos = ClampToCameraBounds(targetPos);
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