using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public bool isAttacking = false;
    public float warningDuration = 1.0f;
    public float chargeDuration = 0.5f;
    public bool isDashing = false;

    [Header("JumpMove")]
    public float slamWarningDuration = 1.5f;
    public float slamRadius = 6.5f;
    public float smokeScaleDuration = 0.5f;
    private List<GameObject> activeWarnings = new List<GameObject>();
    
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

        // ซ่อน hitBox และ VFX ทั้งหมดตอน Start
        foreach (var go in hitBox) { if (go != null) go.SetActive(false); }
        foreach (var go in VFX) { if (go != null) go.SetActive(false); }
    }

    void Update()
    {   
        if (player.attackTimer <= 0)
        {
            isGettingHit = false;
        }
        HealthUpdate();
        if (health <= 0) return; // บอสตายแล้ว หยุด Update เลย
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
                if (attackTimer <= 0 && !isAttacking)
                {
                    if (this.name == "Boss")
                    {
                        ChooseBossMove();
                    }
                    else if (this.name == "DogBoss")
                    {
                        ChooseDogBossMove();
                    }
                    else if (this.name == "BigBoss")
                    {
                        ChooseBigBossMove();
                    }
                }
            }
        }
    }
    void ChooseBigBossMove()
    {
        float playerRange = Vector2.Distance(transform.position, playerTransform.position);
        if (playerRange < 5)
        {
            StartCoroutine(BigBossSlamMoveCoroutine());
        }
        else
        {
            int randomMove = Random.Range(0, 2); // 0 or 1
            if (randomMove == 0)
            {
                StartCoroutine(BigBossChargeMoveCoroutine());
            }
            else if (randomMove == 1)
            {
                StartCoroutine(BigBossJumpSlamMoveCoroutine());
            }
        }
    }
    void ChooseDogBossMove()
    {
        float playerRange = Vector2.Distance(transform.position, playerTransform.position);
        if (playerRange > 5)
        {
            int randomMove = Random.Range(0, 2); // 0 or 1
            if (randomMove == 0)
            {
                DogBossFirstMove();
            }
            else
            {
                StartCoroutine(DogBossJumpSlamMoveCoroutine());
            }
        }
        else
        {
            StartCoroutine(DogBossJumpSlamMoveCoroutine());
        }
    }  
    void ChooseBossMove()
    {
        float playerRange = Vector2.Distance(transform.position, playerTransform.position);
        if (playerRange > 5)
        {
            int randomMove = Random.Range(0, 2); // 0 or 1
            if (randomMove == 0)
            {
                BossFirstMove();
            }
            else
            {
                StartCoroutine(BossChargeMoveCoroutine());
            }
        }
        else
        {
            StartCoroutine(BossSlamMoveCoroutine());
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
            CleanUpAttacks();
            this.gameObject.SetActive(false);
            player.currentEXP += enemyInfo.expGiven;
        }
    }

    void OnDisable()
    {
        CleanUpAttacks();
    }

    void CleanUpAttacks()
    {
        StopAllCoroutines();
        foreach (GameObject go in activeWarnings)
        {
            if (go != null) Destroy(go);
        }
        activeWarnings.Clear();
        isAttacking = false;
        isDashing = false;
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
    
    void DogBossFirstMove()
    {
        if (playerTransform == null) return;

        isAttacking = true;

        GameObject bonePrefab = null;

        foreach (GameObject go in VFX)
        {
            // --- เพิ่มบรรทัดนี้เพื่อกัน Error ครับเจมส์ ---
            if (go == null) continue; 
            // ---------------------------------------
    
            if (go.name.Contains("Bone")) 
            {
                bonePrefab = go;
                break;
            }
        }

        if (bonePrefab == null)
        {
            Debug.LogError("หา VFX ชื่อ Bone ไม่เจอจ้าเจมส์");
            isAttacking = false;
            return;
        }

        // 2. เสกกระดาษออกมาจาก "กลางตัวบอส" (transform.position)
        GameObject projectile = Instantiate(bonePrefab, transform.position, Quaternion.identity);
        projectile.SetActive(true);

        // 3. คำนวณทิศทางจาก "กลางตัวบอส" ไปหา Player
        Vector2 shootDirection = (Vector2)playerTransform.position - (Vector2)transform.position;

        // 4. ส่งทิศทางไปให้กระดาษพุ่ง
        PaperProjectile paperScript = projectile.GetComponent<PaperProjectile>();
        if (paperScript != null)
        {
            paperScript.SetDirection(shootDirection);
        }

        attackTimer = 2;
        isAttacking = false;
    }
    void BossFirstMove()
    {
        if (playerTransform == null) return;

        isAttacking = true;

        GameObject paperPrefab = null;

        foreach (GameObject go in VFX)
        {
            // --- เพิ่มบรรทัดนี้เพื่อกัน Error ครับเจมส์ ---
            if (go == null) continue; 
            // ---------------------------------------
    
            if (go.name.Contains("Paper")) 
            {
                paperPrefab = go;
                break;
            }
        }

        if (paperPrefab == null)
        {
            Debug.LogError("หา VFX ชื่อ Paper ไม่เจอจ้าเจมส์");
            isAttacking = false;
            return;
        }

        // 2. เสกกระดาษออกมาจาก "กลางตัวบอส" (transform.position)
        GameObject projectile = Instantiate(paperPrefab, transform.position, Quaternion.identity);
        projectile.SetActive(true);

        // 3. คำนวณทิศทางจาก "กลางตัวบอส" ไปหา Player
        Vector2 shootDirection = (Vector2)playerTransform.position - (Vector2)transform.position;

        // 4. ส่งทิศทางไปให้กระดาษพุ่ง
        PaperProjectile paperScript = projectile.GetComponent<PaperProjectile>();
        if (paperScript != null)
        {
            paperScript.SetDirection(shootDirection);
        }

        attackTimer = 2;
        isAttacking = false;
    }

    IEnumerator BossChargeMoveCoroutine()
    {
        if (playerTransform == null) yield break;

        isAttacking = true;

        // 1. จำตำแหน่ง Player ณ ตอนนี้
        Vector2 targetPos = playerTransform.position;
        Vector2 bossPos = transform.position;
        Vector2 direction = targetPos - bossPos;
        float distance = direction.magnitude;

        // 2. สร้าง Warning Indicator (hitBox[0]) เป็นเส้นยาวจากบอสไปหา Player
        GameObject warningIndicator = null;
        if (hitBox.Length > 0 && hitBox[0] != null)
        {
            Vector2 midPoint = (bossPos + targetPos) / 2f;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            warningIndicator = Instantiate(hitBox[0], midPoint, Quaternion.Euler(0, 0, angle), transform);
            // Scale X = ระยะทาง (ความยาวเส้น), Scale Y = ความกว้างของ hitbox
            warningIndicator.transform.localScale = new Vector3(distance, 2f, 1f);
            warningIndicator.SetActive(true);

            SpriteRenderer warningSR = warningIndicator.GetComponent<SpriteRenderer>();
            if (warningSR != null)
            {
                warningSR.enabled = true;
                Color c = warningSR.color;
                c.a = 150f / 255f;
                warningSR.color = c;
            }

            activeWarnings.Add(warningIndicator);
        }

        // 3. รอให้ผู้เล่นเห็น Warning
        yield return new WaitForSeconds(warningDuration);

        // 4. ลบ Warning Indicator
        if (warningIndicator != null)
        {
            activeWarnings.Remove(warningIndicator);
            Destroy(warningIndicator);
        }

        // 5. เริ่ม Dash ไปยังตำแหน่งเป้าหมาย
        isDashing = true;
        float elapsedTime = 0f;
        Vector2 startPos = transform.position;

        while (elapsedTime < chargeDuration)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, elapsedTime / chargeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        // 6. จบ Dash
        isDashing = false;
        attackTimer = 2;
        isAttacking = false;
    }
    IEnumerator BigBossChargeMoveCoroutine()
    {
        if (playerTransform == null) yield break;

        isAttacking = true;

        int maxCharges = 4;
        for (int i = 0; i < maxCharges; i++)
        {
            // Get current player position and dash past them by 5 units
            Vector2 bossPos = transform.position;
            Vector2 playerPos = playerTransform.position;
            Vector2 direction = playerPos - bossPos;
            float distance = direction.magnitude;
            Vector2 targetPos = playerPos + direction.normalized * 5f;

            // Spawn warning indicator
            GameObject warningIndicator = null;
            if (hitBox.Length > 0 && hitBox[0] != null)
            {
                Vector2 midPoint = (bossPos + targetPos) / 2f;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float warningLength = Vector2.Distance(bossPos, targetPos);

                warningIndicator = Instantiate(hitBox[0], midPoint, Quaternion.Euler(0, 0, angle), transform);
                warningIndicator.transform.localScale = new Vector3(warningLength, 2f, 2f);
                warningIndicator.SetActive(true);

                SpriteRenderer warningSR = warningIndicator.GetComponent<SpriteRenderer>();
                if (warningSR != null)
                {
                    warningSR.enabled = true;
                    Color c = warningSR.color;
                    c.a = 150f / 255f;
                    warningSR.color = c;
                }

                activeWarnings.Add(warningIndicator);
            }

            // Wait for player to see warning
            yield return new WaitForSeconds(warningDuration);

            // Remove warning
            if (warningIndicator != null)
            {
                activeWarnings.Remove(warningIndicator);
                Destroy(warningIndicator);
            }

            // Dash to target position
            isDashing = true;
            float elapsedTime = 0f;
            Vector2 startPos = transform.position;

            while (elapsedTime < chargeDuration)
            {
                transform.position = Vector2.Lerp(startPos, targetPos, elapsedTime / chargeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;
            isDashing = false;

            // Check if player is still nearby to chain another charge
            if (i < maxCharges - 1)
            {
                float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                if (distToPlayer > slamRadius)
                    break;
            }
        }

        attackTimer = 2;
        isAttacking = false;
    }

    // เช็กว่าบอสชนผู้เล่นขณะ Dash หรือไม่
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing && collision.gameObject.CompareTag("Player"))
        {
            player.health -= attackDamage;
            player.ApplyKnockback(transform.position, 2f, 0.1f);
        }
    }
    IEnumerator BossSlamMoveCoroutine()
    {
        if (playerTransform == null) yield break;

        isAttacking = true;

        // 1. หา Circle Warning prefab จาก hitBox array
        GameObject circlePrefab = null;
        if (hitBox.Length > 1 && hitBox[1] != null)
        {
            circlePrefab = hitBox[1];
        }

        // 2. สร้าง Circle Warning ตรงตำแหน่งบอส เป็น child ของบอสเพื่อให้ตามบอสไป
        GameObject circleWarning = null;
        if (circlePrefab != null)
        {
            circleWarning = Instantiate(circlePrefab, transform.position, Quaternion.identity, transform);
            circleWarning.transform.localPosition = Vector3.zero;
            // Scale ให้เท่ากับ slamRadius * 2 (เส้นผ่านศูนย์กลาง)
            float diameter = slamRadius * 2f;
            circleWarning.transform.localScale = new Vector3(diameter, diameter, 1f);
            circleWarning.SetActive(true);
            activeWarnings.Add(circleWarning);
        }

        // 3. รอให้ผู้เล่นเห็น Warning แล้วหนี
        yield return new WaitForSeconds(slamWarningDuration);

        // 4. ปิด SpriteRenderer ของ hitbox warning (ไม่ทำลาย เพราะอาจใช้ collider ต่อ)
        if (circleWarning != null)
        {
            SpriteRenderer warningSR = circleWarning.GetComponent<SpriteRenderer>();
            if (warningSR != null)
            {
                warningSR.enabled = false;
            }
        }

        // 5. บอสนั่งทับ! Squish sprite ลงเพื่อจำลองการกระแทก
        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.6f, originalScale.z);
        yield return new WaitForSeconds(0.15f);
        transform.localScale = originalScale;

        // 6. เช็กว่า Player อยู่ในรัศมี slam หรือไม่ ถ้าอยู่ก็โดนดาเมจ
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer <= slamRadius)
        {
            player.health -= attackDamage;
            player.ApplyKnockback(transform.position, 3f, 0.15f);
        }

        // 7. ลบ Circle Warning
        if (circleWarning != null)
        {
            activeWarnings.Remove(circleWarning);
            Destroy(circleWarning);
        }

        // 8. เสก Smoke VFX
        GameObject smokePrefab = null;
        foreach (GameObject go in VFX)
        {
            if (go == null) continue;
            if (go.name.Contains("Smoke"))
            {
                smokePrefab = go;
                break;
            }
        }

        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);
            smoke.SetActive(true);

            // บังคับให้ SpriteRenderer เปิดและ alpha เต็ม
            SpriteRenderer smokeSR = smoke.GetComponent<SpriteRenderer>();
            if (smokeSR != null)
            {
                smokeSR.enabled = true;
                Color c = smokeSR.color;
                c.a = 1f;
                smokeSR.color = c;
            }

            Debug.Log("<color=green>Smoke VFX spawned!</color> Position: " + smoke.transform.position + " Active: " + smoke.activeSelf);
            StartCoroutine(SmokeEffectCoroutine(smoke));
        }
        else
        {
            Debug.LogWarning("หา Smoke VFX ไม่เจอใน VFX array!");
        }

        attackTimer = 2;
        isAttacking = false;
    }
    IEnumerator DogBossJumpSlamMoveCoroutine()
    {
        if (playerTransform == null) yield break;

        isAttacking = true;

        // 1. Anticipation squish — telegraph the jump
        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.6f, originalScale.z);
        yield return new WaitForSeconds(0.15f);
        transform.localScale = originalScale;

        // 2. Disappear (jump up), capture player position
        if (sr != null) sr.enabled = false;
        Vector2 landingPos = playerTransform.position;

        // 3. Spawn stationary circle warning at player position (not parented, so it stays put)
        GameObject circlePrefab = (hitBox.Length > 0 && hitBox[0] != null) ? hitBox[0] : null;
        GameObject circleWarning = null;
        if (circlePrefab != null)
        {
            circleWarning = Instantiate(circlePrefab, landingPos, Quaternion.identity);
            Debug.Log("Circle warning spawned at: " + landingPos + " | Active: " + circleWarning.activeSelf);
            float diameter = slamRadius * 2f;
            circleWarning.transform.localScale = new Vector3(diameter, diameter, 1f);
            circleWarning.SetActive(true);

            SpriteRenderer circleSR = circleWarning.GetComponent<SpriteRenderer>();
            if (circleSR != null)
            {
                circleSR.enabled = true;
                Color c = circleSR.color;
                c.a = 150f / 255f;
                circleSR.color = c;
            }

            activeWarnings.Add(circleWarning);
        }
        else
        {
            Debug.LogWarning("circlePrefab is null! hitBox.Length=" + hitBox.Length + " hitBox[0]=" + (hitBox.Length > 0 ? hitBox[0] : "out of range"));
        }

        // 4. Wait for player to see warning and dodge
        yield return new WaitForSeconds(slamWarningDuration);

        // 5. Hide warning sprite
        if (circleWarning != null)
        {
            SpriteRenderer warningSR = circleWarning.GetComponent<SpriteRenderer>();
            if (warningSR != null) warningSR.enabled = false;
        }

        // 6. Teleport to landing position and reappear
        transform.position = landingPos;
        if (sr != null) sr.enabled = true;

        // 7. Landing squish impact
        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.6f, originalScale.z);
        yield return new WaitForSeconds(0.15f);
        transform.localScale = originalScale;

        // 8. Deal damage if player is in slam radius
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer <= slamRadius)
        {
            player.health -= attackDamage;
            player.ApplyKnockback(transform.position, 3f, 0.15f);
        }

        // 9. Destroy circle warning
        if (circleWarning != null)
        {
            activeWarnings.Remove(circleWarning);
            Destroy(circleWarning);
        }

        // 10. Smoke VFX
        GameObject smokePrefab = null;
        foreach (GameObject go in VFX)
        {
            if (go == null) continue;
            if (go.name.Contains("Smoke"))
            {
                smokePrefab = go;
                break;
            }
        }

        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);
            smoke.SetActive(true);

            SpriteRenderer smokeSR = smoke.GetComponent<SpriteRenderer>();
            if (smokeSR != null)
            {
                smokeSR.enabled = true;
                Color c = smokeSR.color;
                c.a = 1f;
                smokeSR.color = c;
            }

            Debug.Log("<color=green>Smoke VFX spawned!</color> Position: " + smoke.transform.position + " Active: " + smoke.activeSelf);
            StartCoroutine(SmokeEffectCoroutine(smoke));
        }
        else
        {
            Debug.LogWarning("หา Smoke VFX ไม่เจอใน VFX array!");
        }

        attackTimer = 2;
        isAttacking = false;
    }
    IEnumerator BigBossSlamMoveCoroutine()
    {
        if (playerTransform == null) yield break;

        isAttacking = true;

        // 1. หา Circle Warning prefab จาก hitBox array
        GameObject circlePrefab = null;
        if (hitBox.Length > 1 && hitBox[1] != null)
        {
            circlePrefab = hitBox[1];
        }

        // 2. สร้าง Circle Warning ตรงตำแหน่งบอส เป็น child ของบอสเพื่อให้ตามบอสไป
        GameObject circleWarning = null;
        if (circlePrefab != null)
        {
            circleWarning = Instantiate(circlePrefab, transform.position, Quaternion.identity, transform);
            circleWarning.transform.localPosition = Vector3.zero;
            // Scale ให้เท่ากับ slamRadius * 2 (เส้นผ่านศูนย์กลาง)
            float diameter = slamRadius * 2f;
            circleWarning.transform.localScale = new Vector3(diameter, diameter, 1f);
            circleWarning.SetActive(true);
            activeWarnings.Add(circleWarning);
        }

        // 3. รอให้ผู้เล่นเห็น Warning แล้วหนี
        yield return new WaitForSeconds(slamWarningDuration);

        // 4. ปิด SpriteRenderer ของ hitbox warning (ไม่ทำลาย เพราะอาจใช้ collider ต่อ)
        if (circleWarning != null)
        {
            SpriteRenderer warningSR = circleWarning.GetComponent<SpriteRenderer>();
            if (warningSR != null)
            {
                warningSR.enabled = false;
            }
        }

        // 5. บอสนั่งทับ! Squish sprite ลงเพื่อจำลองการกระแทก
        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.6f, originalScale.z);
        yield return new WaitForSeconds(0.15f);
        transform.localScale = originalScale;

        // 6. เช็กว่า Player อยู่ในรัศมี slam หรือไม่ ถ้าอยู่ก็โดนดาเมจ
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer <= slamRadius)
        {
            player.health -= attackDamage;
            player.ApplyKnockback(transform.position, 3f, 0.15f);
        }

        // 7. ลบ Circle Warning
        if (circleWarning != null)
        {
            activeWarnings.Remove(circleWarning);
            Destroy(circleWarning);
        }

        // 8. เสก Smoke VFX
        GameObject smokePrefab = null;
        foreach (GameObject go in VFX)
        {
            if (go == null) continue;
            if (go.name.Contains("Smoke"))
            {
                smokePrefab = go;
                break;
            }
        }

        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);
            smoke.SetActive(true);

            // บังคับให้ SpriteRenderer เปิดและ alpha เต็ม
            SpriteRenderer smokeSR = smoke.GetComponent<SpriteRenderer>();
            if (smokeSR != null)
            {
                smokeSR.enabled = true;
                Color c = smokeSR.color;
                c.a = 1f;
                smokeSR.color = c;
            }

            Debug.Log("<color=green>Smoke VFX spawned!</color> Position: " + smoke.transform.position + " Active: " + smoke.activeSelf);
            StartCoroutine(SmokeEffectCoroutine(smoke));
        }
        else
        {
            Debug.LogWarning("หา Smoke VFX ไม่เจอใน VFX array!");
        }

        attackTimer = 2;
        isAttacking = false;
    }
    IEnumerator BigBossJumpSlamMoveCoroutine()
    {
        if (playerTransform == null) yield break;

        isAttacking = true;

        // 1. Anticipation squish — telegraph the jump
        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.6f, originalScale.z);
        yield return new WaitForSeconds(0.15f);
        transform.localScale = originalScale;

        // 2. Disappear (jump up), capture player position
        if (sr != null) sr.enabled = false;
        Vector2 landingPos = playerTransform.position;

        // 3. Spawn stationary circle warning at player position (not parented, so it stays put)
        GameObject circlePrefab = (hitBox.Length > 0 && hitBox[1] != null) ? hitBox[1] : null;
        GameObject circleWarning = null;
        if (circlePrefab != null)
        {
            circleWarning = Instantiate(circlePrefab, landingPos, Quaternion.identity);
            Debug.Log("Circle warning spawned at: " + landingPos + " | Active: " + circleWarning.activeSelf);
            float diameter = slamRadius * 2f;
            circleWarning.transform.localScale = new Vector3(diameter, diameter, 1f);
            circleWarning.SetActive(true);

            SpriteRenderer circleSR = circleWarning.GetComponent<SpriteRenderer>();
            if (circleSR != null)
            {
                circleSR.enabled = true;
                Color c = circleSR.color;
                c.a = 150f / 255f;
                circleSR.color = c;
            }

            activeWarnings.Add(circleWarning);
        }
        else
        {
            Debug.LogWarning("circlePrefab is null! hitBox.Length=" + hitBox.Length + " hitBox[0]=" + (hitBox.Length > 0 ? hitBox[0] : "out of range"));
        }

        // 4. Wait for player to see warning and dodge
        yield return new WaitForSeconds(slamWarningDuration);

        // 5. Hide warning sprite
        if (circleWarning != null)
        {
            SpriteRenderer warningSR = circleWarning.GetComponent<SpriteRenderer>();
            if (warningSR != null) warningSR.enabled = false;
        }

        // 6. Teleport to landing position and reappear
        transform.position = landingPos;
        if (sr != null) sr.enabled = true;

        // 7. Landing squish impact
        transform.localScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.6f, originalScale.z);
        yield return new WaitForSeconds(0.15f);
        transform.localScale = originalScale;

        // 8. Deal damage if player is in slam radius
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer <= slamRadius)
        {
            player.health -= attackDamage;
            player.ApplyKnockback(transform.position, 3f, 0.15f);
        }

        // 9. Destroy circle warning
        if (circleWarning != null)
        {
            activeWarnings.Remove(circleWarning);
            Destroy(circleWarning);
        }

        // 10. Smoke VFX
        GameObject smokePrefab = null;
        foreach (GameObject go in VFX)
        {
            if (go == null) continue;
            if (go.name.Contains("Smoke"))
            {
                smokePrefab = go;
                break;
            }
        }

        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);
            smoke.SetActive(true);

            SpriteRenderer smokeSR = smoke.GetComponent<SpriteRenderer>();
            if (smokeSR != null)
            {
                smokeSR.enabled = true;
                Color c = smokeSR.color;
                c.a = 1f;
                smokeSR.color = c;
            }

            Debug.Log("<color=green>Smoke VFX spawned!</color> Position: " + smoke.transform.position + " Active: " + smoke.activeSelf);
            StartCoroutine(SmokeEffectCoroutine(smoke));
        }
        else
        {
            Debug.LogWarning("หา Smoke VFX ไม่เจอใน VFX array!");
        }

        attackTimer = 2;
        isAttacking = false;
    }

    IEnumerator SmokeEffectCoroutine(GameObject smoke)
    {
        if (smoke == null) yield break;

        SpriteRenderer smokeSR = smoke.GetComponent<SpriteRenderer>();
        Vector3 startScale = smoke.transform.localScale;
        Vector3 endScale = startScale * 5f;
        Color startColor = smokeSR != null ? smokeSR.color : Color.white;

        float elapsed = 0f;

        while (elapsed < smokeScaleDuration)
        {
            float t = elapsed / smokeScaleDuration;

            smoke.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            if (smokeSR != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                smokeSR.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(smoke);
    }
}
