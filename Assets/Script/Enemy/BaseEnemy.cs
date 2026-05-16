using UnityEngine;
using System.Collections;

public class BaseEnemy : MonoBehaviour
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

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Player player;
    private Transform playerTransform;
    private WeaponHandler weaponHandlerScript;

    [Header("Attack Animation")]
    public float spinDuration = 0.2f;
    public float spinSpeed = 1800f;
    public float lungeDistance = 1.5f;

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
    public bool isAttacking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (weaponHandler != null) weaponHandlerScript = weaponHandler.GetComponent<WeaponHandler>();

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

            if (AttackArea != null)
                AttackArea.transform.localScale = Vector3.one * enemyInfo.attackReach;

            if (Sense != null)
                Sense.transform.localPosition = new Vector3(Sense.transform.localPosition.x, Sense.transform.localPosition.y + enemyInfo.senseLocate, Sense.transform.localPosition.z);
        }

        Timer = waitTimer;
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
        if (player == null || playerTransform == null)
        {
            FindPlayer();
            return;
        }

        if (player.attackTimer <= 0) isGettingHit = false;

        HealthUpdate();

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        if (DetectionArea != null) playerDetect = DetectionArea.playerDetect;
        if (AttackArea != null) hitArea = AttackArea.hitArea;

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
                MoveTowardsPlayer();
                if (Sense != null) Sense.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    void Attack()
    {
        if (hitArea && attackTimer <= 0 && !isAttacking)
        {
            attackTimer = enemyInfo.attackSpeed;

            GameObject target = GameObject.FindGameObjectWithTag("Player");

            if (target != null)
            {
                Player targetPlayer = target.GetComponent<Player>();

                if (targetPlayer != null && !targetPlayer.isDead)
                {
                    targetPlayer.health -= attackDamage;
                    targetPlayer.ApplyKnockback(transform.position, 2f, 0.1f);

                    Debug.Log($"<color=yellow>{gameObject.name}:</color> ตบเข้าจังๆ! ดาเมจ: {attackDamage} | เลือดเป้าหมายเหลือ: {targetPlayer.health}");
                }
            }

            StartCoroutine(AttackSpinCoroutine());
        }
    }

    IEnumerator AttackSpinCoroutine()
    {
        isAttacking = true;

        Vector3 startPos = transform.position;
        Vector3 lungeDir = playerTransform != null
            ? (playerTransform.position - transform.position).normalized
            : Vector3.zero;

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            float t = elapsed / spinDuration;
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(startPos, startPos + lungeDir * lungeDistance, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        isAttacking = false;
    }

    void HealthUpdate()
    {
        if (health <= 0)
        {
            // ให้ EXP กับ Player ตัวที่ยังมีชีวิตอยู่
            Player p = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
            if (p != null) p.currentEXP += expGiven;

            gameObject.SetActive(false);
        }
    }

    void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;

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

        if (sr != null) sr.flipX = (playerTransform.position.x >= transform.position.x);
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
            // ดึงข้อมูลดาเมจจาก Player ล่าสุดเสมอ
            Player attacker = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
            if (attacker == null) return;

            Collider2D myMainCollider = GetComponent<Collider2D>();
            if (collision.IsTouching(myMainCollider) && !isGettingHit)
            {
                float totalDamage = attacker.attackDamage + (attacker.attackDamage * (attacker.bonusAttackDamage / 100));
                health -= totalDamage;
                isGettingHit = true;

                ApplyKnockback(collision.transform.position);

                if (weaponHandlerScript != null)
                    weaponHandlerScript.DecreaseDurability(enemyInfo.durabilityCost);

                Debug.Log($"<color=red>{gameObject.name}:</color> โดนตบคืน! ดาเมจที่ได้รับ: {totalDamage}");
            }
        }
    }
}