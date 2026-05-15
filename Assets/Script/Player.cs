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
            Destroy(gameObject);
            return;
        }
    }

    [Header("Core References")]
    public Transform handTransform;
    public Transform weaponHandler;
    public Transform HitBox;
    public SpriteRenderer[] characterParts;
    public Rigidbody2D rb;
    public Animator animator;
    public WeaponHandler weaponHandlerScript;
    public GameObject selectionUI;

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

    [Header("Audio Settings")]
    public AudioClip pickupSound;

    [Header("End Game Panels")] // --- [ส่วนที่อัปเดตใหม่] ---
    [Tooltip("ลาก DeadPanel มาใส่ที่นี่")]
    public GameObject deadPanel;
    [Tooltip("ลาก VictoryPanel มาใส่ที่นี่")]
    public GameObject victoryPanel;

    [Header("State Flags")]
    public bool isHand;
    public bool isSelecting;
    public bool weaponInRange = false;
    public ItemPickup weaponNew;
    public bool isDead = false;

    private float baseAngle;
    private bool isMouseOnLeft;

    void Start()
    {
        RefreshReferences();

        if (characterParts.Length > 4) handSR = characterParts[4];
        weaponSR = weaponHandler.GetComponent<SpriteRenderer>();
        weaponHandlerScript = weaponHandler.GetComponent<WeaponHandler>();

        health = maxHealth + bonusHealth;

        // ปิด Panel ไว้ก่อนเริ่มเกมกันพลาด
        if (deadPanel != null) deadPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        if (mainCam == null) mainCam = Camera.main;

        if (selectionUI == null || !selectionUI.activeInHierarchy)
        {
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
        HandleStatsAndExperience();
        Attack();

        if (weaponInRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    void RefreshReferences()
    {
        mainCam = Camera.main;
        if (selectionUI == null) selectionUI = GameObject.Find("InGameUI");
    }

    void Pickup()
    {
        if (weaponNew == null) return;

        if (MainMenuControl.instance != null && pickupSound != null)
        {
            MainMenuControl.instance.PlaySFXWithPitch(pickupSound);
        }

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

        if (handData != null) weaponNew.SetWeapon(handData, handRarity, handDurability);
        else Destroy(weaponNew.gameObject);
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

    // --- [ฟังก์ชันตาย: เปิด Dead Panel] ---
    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("<color=red>Player HAS DIED!</color>");

        if (deadPanel != null)
        {
            deadPanel.SetActive(true);
            Time.timeScale = 0f; // หยุดเวลาเกม
        }
    }

    // --- [ฟังก์ชันชนะ: เปิด Victory Panel] ---
    // นายสามารถเรียกฟังก์ชันนี้จากสคริปต์อื่นได้ เช่น มอนสเตอร์ตายหมด หรือเข้าเส้นชัย
    public void Victory()
    {
        if (isDead) return;
        Debug.Log("<color=green>VICTORY!</color>");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // หยุดเวลาเกม
        }
    }

    IEnumerator LevelUpRoutine()
    {
        isSelecting = true;
        while (currentEXP >= maxEXP)
        {
            currentEXP -= maxEXP;
            level += 1;
            maxEXP *= 1.25f;

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
        if (weaponHandlerScript == null || weaponHandlerScript.currentWeapon == null)
        {
            ResetAttack();
            return;
        }

        if (weaponHandlerScript.currentWeapon.weaponType == WeaponInfo.WeaponType.Melee)
        {
            if (Input.GetMouseButtonDown(0) && attackTimer <= 0)
            {
                HitBox.gameObject.SetActive(true);
                attackTimer = attackTime;
                baseAngle = GetMouseAngle();
                weaponHandlerScript.DecreaseDurability(1f);
            }

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
                float progress = attackTimer / attackTime;
                float startOffset = isMouseOnLeft ? (-45f - 180f) : 45f;
                float endOffset = isMouseOnLeft ? (90f - 180f) : -90f;
                float currentOffset = Mathf.Lerp(endOffset, startOffset, progress);
                handTransform.localRotation = Quaternion.Euler(0, 0, baseAngle + currentOffset);
            }
            else
            {
                HitBox.gameObject.SetActive(false);
            }
        }
    }

    public void ResetAttack()
    {
        attackTimer = 0;
        if (HitBox != null) HitBox.gameObject.SetActive(false);
        handTransform.localRotation = Quaternion.identity;
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

    float GetMouseAngle()
    {
        if (mainCam == null) return 0f;
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - handTransform.position;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
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
            bool movingOpposite = (isMouseOnLeft && moveInput.x > 0) || (!isMouseOnLeft && moveInput.x < 0);
            isBackstepping = movingOpposite;
            currentSpeed = movingOpposite ? speed * 0.5f : speed;
        }

        animator.SetBool("isMove", moveInput != Vector2.zero);
        animator.SetBool("isBackstep", isBackstepping);
        rb.linearVelocity = moveInput * currentSpeed;
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