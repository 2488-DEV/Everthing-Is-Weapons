using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossEnemy : MonoBehaviour
{
    [Header("Core References")]
    public EnemyInfo enemyInfo;
    public Transform weaponHandler;
    public DetectionArea DetectionArea;
    public AttackArea AttackArea;
    public GameObject Sense;
    public WeaponHandler weaponHandlerScript;

    [Header("Boss Skills & VFX")]
    public GameObject[] hitBox; // [0] = Dash Line, [1] = Slam Circle
    public GameObject[] VFX;    // Objects containing "Paper" or "Smoke" names

    [Header("Attack Settings")]
    public float waitTimer = 1.0f; // <--- แก้จุดนี้เรียบร้อยครับนาย!
    public float warningDuration = 1.0f;
    public float chargeDuration = 0.5f;
    public float slamWarningDuration = 1.5f;
    public float slamRadius = 6.5f;
    public float smokeScaleDuration = 0.5f;

    // Internal Variables
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Player player;
    private Transform playerTransform;
    private List<GameObject> activeWarnings = new List<GameObject>();

    [Header("Status (Read Only)")]
    public float health;
    public float maxHealth;
    public float attackDamage;
    public float expGiven;
    public float Timer;
    public float attackTimer;
    public bool playerDetect;
    public bool hitArea;
    public bool isAttacking = false;
    public bool isDashing = false;
    public bool isGettingHit;

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
            if (AttackArea != null) AttackArea.transform.localScale = Vector3.one * enemyInfo.attackReach;
            if (Sense != null) Sense.transform.localPosition = new Vector3(0, enemyInfo.senseLocate, 0);
        }

        Timer = waitTimer;
        FindPlayer();
        UpdateWeaponVisual();

        foreach (var go in hitBox) { if (go != null) go.SetActive(false); }
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
        if (health <= 0) return;
        if (playerTransform == null) { FindPlayer(); return; }

        // ป้องกัน Error ถ้า player ยังโหลดไม่เสร็จ
        if (player != null && player.attackTimer <= 0) isGettingHit = false;

        HealthUpdate();
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        playerDetect = DetectionArea != null && DetectionArea.playerDetect;
        hitArea = AttackArea != null && AttackArea.hitArea;

        if (playerDetect && !isAttacking)
        {
            if (Timer > 0)
            {
                Timer -= Time.deltaTime;
                if (Sense != null) Sense.GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                Timer = 0; // ล็อกไว้ที่ 0 ไม่ให้ติดลบ
                if (Sense != null) Sense.GetComponent<SpriteRenderer>().enabled = false;
                LookAtPlayer();

                if (attackTimer <= 0)
                {
                    ChooseBossMove();
                }
            }
        }
    }

    void ChooseBossMove()
    {
        float playerRange = Vector2.Distance(transform.position, playerTransform.position);

        if (playerRange > 5f)
        {
            if (Random.value > 0.5f) BossFirstMove();
            else StartCoroutine(BossChargeMoveCoroutine());
        }
        else
        {
            StartCoroutine(BossSlamMoveCoroutine());
        }
    }

    void BossFirstMove()
    {
        isAttacking = true;
        GameObject paperPrefab = System.Array.Find(VFX, go => go != null && go.name.Contains("Paper"));

        if (paperPrefab != null)
        {
            GameObject projectile = Instantiate(paperPrefab, transform.position, Quaternion.identity);
            projectile.SetActive(true);
            Vector2 shootDirection = (Vector2)playerTransform.position - (Vector2)transform.position;

            PaperProjectile paperScript = projectile.GetComponent<PaperProjectile>();
            if (paperScript != null) paperScript.SetDirection(shootDirection);
        }

        FinishAttack(2f);
    }

    IEnumerator BossChargeMoveCoroutine()
    {
        isAttacking = true;
        Vector2 targetPos = playerTransform.position;
        Vector2 direction = targetPos - (Vector2)transform.position;
        float distance = direction.magnitude;

        if (hitBox.Length > 0 && hitBox[0] != null)
        {
            GameObject warning = Instantiate(hitBox[0], (Vector2)transform.position + direction / 2f,
                Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg), transform);
            warning.transform.localScale = new Vector3(distance, warning.transform.localScale.y, 1f);
            warning.SetActive(true);
            activeWarnings.Add(warning);
            yield return new WaitForSeconds(warningDuration);
            activeWarnings.Remove(warning);
            Destroy(warning);
        }

        isDashing = true;
        float elapsed = 0;
        Vector2 startPos = transform.position;
        while (elapsed < chargeDuration)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed / chargeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        isDashing = false;
        FinishAttack(2f);
    }

    IEnumerator BossSlamMoveCoroutine()
    {
        isAttacking = true;
        if (hitBox.Length > 1 && hitBox[1] != null)
        {
            GameObject circle = Instantiate(hitBox[1], transform.position, Quaternion.identity, transform);
            circle.transform.localScale = new Vector3(slamRadius * 2, slamRadius * 2, 1);
            circle.SetActive(true);
            activeWarnings.Add(circle);
            yield return new WaitForSeconds(slamWarningDuration);
            activeWarnings.Remove(circle);
            Destroy(circle);
        }

        Vector3 oldScale = transform.localScale;
        transform.localScale = new Vector3(oldScale.x * 1.3f, oldScale.y * 0.6f, oldScale.z);
        yield return new WaitForSeconds(0.15f);
        transform.localScale = oldScale;

        if (Vector2.Distance(transform.position, playerTransform.position) <= slamRadius)
        {
            if (player != null)
            {
                player.health -= attackDamage;
                player.ApplyKnockback(transform.position, 3f, 0.15f);
            }
        }

        GameObject smokePrefab = System.Array.Find(VFX, go => go != null && go.name.Contains("Smoke"));
        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);
            smoke.SetActive(true);
            StartCoroutine(SmokeEffectCoroutine(smoke));
        }

        FinishAttack(2f);
    }

    void FinishAttack(float cooldown)
    {
        attackTimer = cooldown;
        isAttacking = false;
        Timer = waitTimer; // รีเซ็ต Timer ให้เริ่มรอใหม่หลังจบการโจมตี
    }

    IEnumerator SmokeEffectCoroutine(GameObject smoke)
    {
        SpriteRenderer smokeSR = smoke.GetComponent<SpriteRenderer>();
        Vector3 startScale = smoke.transform.localScale;
        float elapsed = 0f;
        while (elapsed < smokeScaleDuration)
        {
            float t = elapsed / smokeScaleDuration;
            smoke.transform.localScale = Vector3.Lerp(startScale, startScale * 3f, t);
            if (smokeSR != null)
            {
                Color c = smokeSR.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                smokeSR.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(smoke);
    }

    void HealthUpdate()
    {
        if (health <= 0)
        {
            CleanUpAttacks();
            if (player != null) player.currentEXP += expGiven;
            gameObject.SetActive(false);
        }
    }

    void CleanUpAttacks()
    {
        StopAllCoroutines();
        foreach (var warning in activeWarnings) { if (warning != null) Destroy(warning); }
        activeWarnings.Clear();
        isAttacking = false;
        isDashing = false;
        if (Sense != null) Sense.GetComponent<SpriteRenderer>().enabled = false;
    }

    void LookAtPlayer()
    {
        if (sr != null && playerTransform != null)
            sr.flipX = (playerTransform.position.x >= transform.position.x);
    }

    void UpdateWeaponVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (enemyInfo != null && sr != null) sr.sprite = enemyInfo.enemyModel;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing && collision.gameObject.CompareTag("Player"))
        {
            if (player != null)
            {
                player.health -= attackDamage;
                player.ApplyKnockback(transform.position, 2f, 0.1f);
            }
        }
    }

    public void ApplyKnockback(Vector3 attackerPos)
    {
        StopCoroutine("KnockbackLerp");
        Vector2 direction = (transform.position - attackerPos).normalized;
        StartCoroutine(KnockbackLerp((Vector2)transform.position + direction * 1.5f, 0.1f));
    }

    IEnumerator KnockbackLerp(Vector2 target, float duration)
    {
        float t = 0; Vector2 start = transform.position;
        while (t < duration)
        {
            transform.position = Vector2.Lerp(start, target, t / duration);
            t += Time.deltaTime; yield return null;
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
                if (player != null)
                {
                    float damage = player.attackDamage + (player.attackDamage * (player.bonusAttackDamage / 100));
                    health -= damage;
                    isGettingHit = true;
                    ApplyKnockback(collision.transform.position);
                    if (weaponHandlerScript != null) weaponHandlerScript.DecreaseDurability(enemyInfo.durabilityCost);
                }
            }
        }
    }
}