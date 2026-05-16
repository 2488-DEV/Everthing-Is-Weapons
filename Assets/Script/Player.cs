using System.Transactions;
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    // --- ระบบ Singleton ---
    public static Player instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ถ้ามีตัวเก่าอยู่แล้ว ทำลายตัวใหม่ทิ้งทันที
            Destroy(gameObject);
            return;
        }
    }

    [Header("Core References")]
    public Transform playerSpawn;
    public Transform handTransform;
    public Transform weaponHandler;
    public Transform HitBox;
    public SpriteRenderer[] characterParts;
    public Rigidbody2D rb;
    public Animator animator;
    public WeaponHandler weaponHandlerScript;
    public GameObject selectionUI; // ตัวนี้จะถูกอัปเดตอัตโนมัติถ้าหลุด

    [Header("Movement Settings")]
    public float speed = 5f;
    public Vector2 moveInput;
    private Camera mainCam;
    private SpriteRenderer handSR;
    private SpriteRenderer weaponSR;

    [Header("Player Stats")]
    public int level = 1;
    public float maxHealth = 100;
    public float health = 100;
    public float attackDamage;
    public float totalDamage;
    public float attackTime;
    public float attackTimer;
    public float maxEXP = 20;
    public float currentEXP;

    [Header("Player Boosts")]
    public float bonusHealth;
    public float bonusAttackDamage;
    public float bonusAttackSpeed;

    [Header("State Flags")]
    public bool isHand;
    public bool isSelecting;
    public bool weaponInRange = false;
    public ItemPickup weaponNew;
    public bool isDead = false;

    [Header("Debuff State")]
    public float speedMultiplier = 1f;
    private Coroutine webDebuffCoroutine;
    private GameObject webVisualInstance;

    [Header("Audio")]
    public AudioClip swingSound;
    public AudioClip[] footstepSounds;
    public AudioClip equipSound;
    public float footstepInterval = 0.4f;

    private float footstepTimer;
    private int lastFootstepIndex;

    private float baseAngle;
    private bool isMouseOnLeft;
    private bool attackFacingLeft;
    private bool rangedThrown;

    void Start()
    {
        RefreshReferences(); // เรียกใช้การหา Reference เริ่มต้น

        if (characterParts.Length > 4) handSR = characterParts[4];
        weaponSR = weaponHandler.GetComponent<SpriteRenderer>();
        weaponHandlerScript = weaponHandler.GetComponent<WeaponHandler>();

        health = maxHealth + bonusHealth;
        speedMultiplier = 1f;
    }

    void Update()
    {
        if (isDead) return;

        // --- ส่วนที่แก้ไข: "ฉลาดข้ามฉาก" เช็ก Reference ที่อาจพังเมื่อเปลี่ยน Scene ---
        if (mainCam == null) mainCam = Camera.main;

        if (selectionUI == null || !selectionUI.activeInHierarchy)
        {
            // พยายามหา UI ใหม่ในฉาก (ตรวจสอบว่า Canvas ในฉากใหม่มีชื่อ InGameUI หรือไม่)
            GameObject foundUI = GameObject.Find("InGameUI");
            if (foundUI != null) selectionUI = foundUI;
        }

        attackTime = Mathf.Max(0.1f, attackTime);

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        if (weaponHandlerScript != null && weaponHandlerScript.currentWeapon != null)
        {
            totalDamage = attackDamage * (1 + (bonusAttackDamage / 100));
        }

        HandleFlipAndMovementLogic();
        HandleFootstepSounds();
        HandleStatsAndExperience();
        Attack();

        if (weaponInRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    // ฟังก์ชันช่วยหา Reference ใหม่เวลาเปลี่ยนด่าน
    void RefreshReferences()
    {
        mainCam = Camera.main;
        if (selectionUI == null) selectionUI = GameObject.Find("InGameUI");
    }

    void HandleStatsAndExperience()
    {
        float currentMax = maxHealth + bonusHealth;
        if (health > currentMax) health = currentMax;

        if (health <= 0 && !isDead)
        {
            health = 0;
            Die();
        }

        if (currentEXP >= maxEXP && !isSelecting)
        {
            StartCoroutine(LevelUpRoutine());
        }
    }

    void HandleFootstepSounds()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        if (moveInput != Vector2.zero)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                footstepTimer = footstepInterval;
                PlayFootstep();
            }
        }
        else
        {
            footstepTimer = 0;
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds.Length == 0) return;

        lastFootstepIndex = (lastFootstepIndex + 1) % footstepSounds.Length;
        AudioClip clip = footstepSounds[lastFootstepIndex];
        if (SoundManagers.instance != null)
            SoundManagers.instance.PlayFootstep(clip);
    }

    void Die()
    {
        isDead = true;
        Debug.Log("<color=red>Player HAS DIED!</color>");
    }

    IEnumerator LevelUpRoutine()
    {
        isSelecting = true;
        while (currentEXP >= maxEXP)
        {
            currentEXP -= maxEXP;
            level += 1;
            maxEXP *= 1.25f;

            // ตรวจสอบ selectionUI อีกครั้งกันพลาด
            if (selectionUI == null) selectionUI = GameObject.Find("InGameUI");

            if (selectionUI != null)
            {
                Transform cardDisplay = selectionUI.transform.Find("Card Display");
                if (cardDisplay != null)
                {
                    Time.timeScale = 0f;
                    cardDisplay.gameObject.SetActive(true);

                    CardSelect cardScript = selectionUI.GetComponent<CardSelect>();
                    if (cardScript != null) cardScript.ShowCard();

                    yield return new WaitUntil(() => isSelecting == false);
                    cardDisplay.gameObject.SetActive(false);
                }
            }

            if (currentEXP >= maxEXP)
            {
                isSelecting = true;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
        isSelecting = false;
        Time.timeScale = 1f;
    }

    public void Attack()
    {
        bool hasWeapon = weaponHandlerScript != null && weaponHandlerScript.currentWeapon != null;

        if (weaponHandlerScript == null || (!hasWeapon && attackTimer <= 0))
        {
            ResetAttack();
            return;
        }

        if (hasWeapon && weaponHandlerScript.currentWeapon.weaponType == WeaponInfo.WeaponType.Melee)
        {
            if (Input.GetMouseButtonDown(0) && attackTimer <= 0)
            {
                HitBox.gameObject.SetActive(true);
                attackTimer = attackTime;
                attackFacingLeft = isMouseOnLeft;
                baseAngle = GetMouseAngle() + (attackFacingLeft ? 180f : 0f);
                weaponHandlerScript.DecreaseDurability(1f);

                if (SoundManagers.instance != null)
                    SoundManagers.instance.PlaySFX(swingSound);
            }

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
                float progress = 1f - (attackTimer / attackTime);
                float windUpEnd = 0.10f;
                float swingEnd = 0.30f;
                float startOffset = attackFacingLeft ? -90f : 90f;
                float endOffset = attackFacingLeft ? 90f : -90f;
                float currentOffset;

                if (progress < windUpEnd)
                {
                    float t = Mathf.SmoothStep(0f, 1f, progress / windUpEnd);
                    currentOffset = Mathf.Lerp(0f, startOffset, t);
                    HitBox.gameObject.SetActive(false);
                }
                else if (progress < swingEnd)
                {
                    float t = (progress - windUpEnd) / (swingEnd - windUpEnd);
                    t = t * t * (3f - 2f * t);
                    currentOffset = Mathf.Lerp(startOffset, endOffset, t);
                    HitBox.gameObject.SetActive(true);
                }
                else
                {
                    float t = Mathf.SmoothStep(0f, 1f, (progress - swingEnd) / (1f - swingEnd));
                    currentOffset = Mathf.Lerp(endOffset, 0f, t);
                    HitBox.gameObject.SetActive(false);
                }

                handTransform.rotation = Quaternion.Euler(0, 0, baseAngle + currentOffset);
            }
            else
            {
                HitBox.gameObject.SetActive(false);
            }
        }

        // Ranged attack (handles both pre-throw and post-throw animation)
        if ((hasWeapon && weaponHandlerScript.currentWeapon.weaponType == WeaponInfo.WeaponType.Range) || (!hasWeapon && attackTimer > 0 && rangedThrown))
        {
            if (hasWeapon && Input.GetMouseButtonDown(0) && attackTimer <= 0)
            {
                attackTimer = attackTime;
                attackFacingLeft = isMouseOnLeft;
                baseAngle = GetMouseAngle() + (attackFacingLeft ? 180f : 0f);
                rangedThrown = false;
            }

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
                float progress = 1f - (attackTimer / attackTime);
                float windUpEnd = 0.20f;
                float startOffset = attackFacingLeft ? -90f : 90f;
                float currentOffset;

                if (progress < windUpEnd)
                {
                    // Phase 1: wind-up — pull hand back
                    float t = Mathf.SmoothStep(0f, 1f, progress / windUpEnd);
                    currentOffset = Mathf.Lerp(0f, startOffset, t);
                }
                else if (!rangedThrown)
                {
                    // Spawn projectile at the peak of wind-up
                    if (hasWeapon)
                    {
                        WeaponInfo thrownWeapon = weaponHandlerScript.currentWeapon;
                        SpawnProjectile(thrownWeapon);
                        weaponHandlerScript.ThrowWeapon();
                        hasWeapon = false;
                    }
                    rangedThrown = true;
                    currentOffset = startOffset;
                }
                else
                {
                    // Phase 3: recovery — hand returns to neutral
                    float t = Mathf.SmoothStep(0f, 1f, (progress - windUpEnd) / (1f - windUpEnd));
                    currentOffset = Mathf.Lerp(startOffset, 0f, t);
                }

                handTransform.rotation = Quaternion.Euler(0, 0, baseAngle + currentOffset);
            }
        }
    }

    public void ResetAttack()
    {
        attackTimer = 0;
        rangedThrown = false;
        if (HitBox != null) HitBox.gameObject.SetActive(false);
        handTransform.rotation = Quaternion.identity;
    }

    void SpawnProjectile(WeaponInfo weapon)
    {
        if (weapon == null) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 direction = (Vector2)(mouseWorld - handTransform.position);
        Vector2 spawnPos = (Vector2)handTransform.position + direction.normalized * 1.5f;

        GameObject projObj = new GameObject("Projectile_" + weapon.weaponName);
        projObj.transform.position = spawnPos;
        projObj.tag = "PlayerAttackHitBox";

        SpriteRenderer sr = projObj.AddComponent<SpriteRenderer>();
        sr.sprite = weapon.itemModel;
        sr.sortingOrder = 5;
        projObj.transform.localScale = weaponHandler.lossyScale;
        if (weapon.itemModel == null)
        {
            sr.color = Color.red;
            projObj.transform.localScale = Vector3.one * 0.3f;
        }

        Rigidbody2D rb = projObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = projObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        // Ignore "for cam" and any camera-related colliders
        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.name.IndexOf("cam", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                foreach (Collider2D c in go.GetComponents<Collider2D>())
                    Physics2D.IgnoreCollision(col, c);
            }
        }

        float totalDamage = attackDamage * (1f + (bonusAttackDamage / 100f));

        ThrowableProjectile proj = projObj.AddComponent<ThrowableProjectile>();
        proj.lifetime = weapon.projectileLifetime;
        proj.Setup(direction, weapon.projectileSpeed, totalDamage);
    }

    public void ApplyKnockback(Vector3 attackerPos, float force, float duration)
    {
        if (isDead) return;
        Vector2 direction = (transform.position - attackerPos).normalized;
        StartCoroutine(PlayerKnockbackLerp((Vector2)transform.position + (direction * force), duration));
    }

    IEnumerator PlayerKnockbackLerp(Vector2 target, float duration)
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

    public void ApplyWebDebuff(float multiplier, float duration, GameObject webVisualPrefab)
    {
        if (webDebuffCoroutine != null)
            StopCoroutine(webDebuffCoroutine);

        speedMultiplier = multiplier;

        if (webVisualPrefab != null && webVisualInstance == null)
        {
            webVisualInstance = Instantiate(webVisualPrefab, transform);
            webVisualInstance.transform.localPosition = Vector3.zero;
        }

        webDebuffCoroutine = StartCoroutine(WebDebuffTimer(duration));
    }

    IEnumerator WebDebuffTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
        if (webVisualInstance != null)
        {
            Destroy(webVisualInstance);
            webVisualInstance = null;
        }
        webDebuffCoroutine = null;
    }

    float GetMouseAngle()
    {
        if (mainCam == null) return 0f;
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - handTransform.position;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    void Pickup()
    {
        if (weaponNew == null) return;
        WeaponInfo groundData = weaponNew.weaponData;
        WeaponRarity groundRarity = weaponNew.weaponRarity;
        float groundDurability = weaponNew.currentDurability;
        WeaponInfo handData = weaponHandlerScript.currentWeapon;
        WeaponRarity handRarity = weaponHandlerScript.currentRarity;
        float handDurability = weaponHandlerScript.currentDurability;

        weaponHandlerScript.SetWeapon(groundData, groundRarity, groundDurability);
        attackTime = groundData.attackSpeed * (1 - (bonusAttackSpeed / 100));
        attackDamage = groundData.baseDamage + groundRarity.rarityDamage;
        HitBox.localScale = new Vector3(0.5f, groundData.attackReach / 1.2f, 0.5f);

        if (SoundManagers.instance != null)
            SoundManagers.instance.PlaySFX(equipSound);

        if (handData != null) weaponNew.SetWeapon(handData, handRarity, handDurability);
        else Destroy(weaponNew.gameObject);
    }

    void HandleFlipAndMovementLogic()
    {
        if (mainCam == null) return;

        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        float angle = GetMouseAngle();
        isMouseOnLeft = mousePos.x < transform.position.x;

        if (attackTimer <= 0)
        {
            if (isMouseOnLeft)
            {
                handTransform.rotation = Quaternion.Euler(0, 0, angle + 180f);
                if (handSR != null) handSR.sortingOrder = 3;
                if (weaponSR != null) weaponSR.sortingOrder = 2;
                FixWeaponSide(-1);
            }
            else
            {
                handTransform.rotation = Quaternion.Euler(0, 0, angle);
                if (handSR != null) handSR.sortingOrder = -2;
                if (weaponSR != null) weaponSR.sortingOrder = -1;
                FixWeaponSide(1);
            }

            foreach (SpriteRenderer part in characterParts)
            {
                if (part != null) part.flipX = isMouseOnLeft;
            }
        }

        float currentSpeed = speed;
        bool isBackstepping = false;
        if (moveInput.x != 0)
        {
            if (isMouseOnLeft)
            {
                isBackstepping = true;
            }
            bool movingOpposite = (isMouseOnLeft && moveInput.x > 0) || (!isMouseOnLeft && moveInput.x < 0);
            if (moveInput.x < 0)
            {
                isBackstepping = true;
            }
            else if (moveInput.x > 0)
            {
                isBackstepping = false;
            }
            currentSpeed = movingOpposite ? speed * 0.5f : speed;
        }

        animator.SetBool("isMove", moveInput != Vector2.zero);
        animator.SetBool("isBackstep", isBackstepping);
        rb.linearVelocity = moveInput * currentSpeed * speedMultiplier;
    }

    void FixWeaponSide(int side)
    {
        if ((side == -1 && weaponHandler.localPosition.x > 0) || (side == 1 && weaponHandler.localPosition.x < 0))
        {
            weaponHandler.localPosition = new Vector3(-weaponHandler.localPosition.x, weaponHandler.localPosition.y, weaponHandler.localPosition.z);
            HitBox.localPosition = new Vector3(-HitBox.localPosition.x, HitBox.localPosition.y, HitBox.localPosition.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            ItemPickup item = collision.GetComponent<ItemPickup>();
            if (item != null)
            {
                weaponInRange = true;
                weaponNew = item;
            }
        }
        if (collision.CompareTag("Hand")) isHand = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            weaponInRange = false;
            weaponNew = null;
        }
    }
}