using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public Transform handTransform;
    public Transform weaponHandler;
    public Transform HitBox;
    public SpriteRenderer[] characterParts;
    public Rigidbody2D rb;
    public Vector2 moveInput;
    public Animator animator;
    public WeaponHandler weaponHandlerScript;
    public ItemPickup weaponNew;
    public float speed = 5f;
    private Camera mainCam; 
    private SpriteRenderer handSR;
    private SpriteRenderer weaponSR;
    public GameObject selectionUI;
    public bool isHand;

    [Header("PlayerStat")]
    public int level = 1;
    public float maxHealth = 100;
    public float health = 100;
    public float attackDamage;
    public float totalDamage;
    public float attackTime;
    public float attackTimer;
    public float maxEXP = 20;
    public float currentEXP;
    private float remainingEXP;

    private float baseAngle;
    private bool isMouseOnLeft;
    private float mousePos;
    private bool weaponInRange = false;
    public bool isSelecting;
    public float knockbackForce = 2000f;

    [Header("PlayerBoost")]
    public float bonusHealth;
    public float bonusAttackDamage;
    public float bonusAttackSpeed;
    
    
    void Start()
    {
        mainCam = Camera.main; 
        if (characterParts.Length > 4)
        {
            handSR = characterParts[4];
        }

        weaponSR = weaponHandler.GetComponent<SpriteRenderer>();
        weaponHandlerScript = weaponHandler.GetComponent<WeaponHandler>();
        health = maxHealth;
    }

    void Update()
    {
        if (attackTime < 0.1f)
        {
            attackTime = 0.1f;
        }
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        HandleFlipAndMovementLogic();
        totalDamage = attackDamage + (attackDamage * (bonusAttackDamage/100));

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        Attack();
        //LevelUp();
        if (currentEXP >= maxEXP && !isSelecting)
        {
            StartCoroutine(LevelUpRoutine());
        }

        if (weaponInRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    void LevelUp()
    {
        if (currentEXP >= maxEXP)
        {
            currentEXP -= maxEXP;
            level += 1;
            maxEXP *= 1.25f;

            Time.timeScale = 0f;
            selectionUI.SetActive(true);
            selectionUI.GetComponent<CardSelect>().ShowCard();
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

            Transform cardDisplay = selectionUI.transform.Find("Card Display");
            Time.timeScale = 0f; 

            cardDisplay.gameObject.SetActive(true);

            CardSelect cardScript = selectionUI.GetComponent<CardSelect>();
            if (cardScript != null)
            {
                cardScript.ShowCard();
            }

            yield return new WaitUntil(() => isSelecting == false);

            cardDisplay.gameObject.SetActive(false);

            if (currentEXP >= maxEXP) 
            {
                isSelecting = true; 

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        isSelecting = false;
        Time.timeScale = 1f;

        Debug.Log("เลเวลอัปเสร็จสิ้น! เลเวลปัจจุบัน: " + level);
    }
    public void ApplyKnockback(Vector3 attackerPos, float force, float duration) 
    {
        // คำนวณทิศทาง (จากคนตี -> มาที่ตัว Player)
        Vector2 direction = (transform.position - attackerPos).normalized;
        Vector2 targetPos = (Vector2)transform.position + (direction * force);
        
        // สั่งให้เริ่มการขยับแบบ Lerp (เจมส์ต้องมี Coroutine นี้ด้วยนะ)
        StartCoroutine(PlayerKnockbackLerp(targetPos, duration));
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
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    
        // 2. หาทางต่างระหว่าง ตำแหน่งเมาส์ กับ ตำแหน่งแขน (Pivot)
        // เราใช้ z = 0 เพื่อให้การคำนวณใน 2D แม่นยำที่สุด
        Vector3 direction = mousePos - handTransform.position;
        direction.z = 0;
        // 3. ใช้ Atan2 เพื่อหาองศา (มันจะคืนค่าเป็น Radian เลยต้องคูณ Rad2Deg เพื่อเปลี่ยนเป็นองศา 0-360)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;

    }
    void Pickup()
    {
        if (weaponNew == null) return;

        
        // 1. ดึงค่าจาก "พื้น" มาเตรียมไว้
        WeaponInfo groundData = weaponNew.weaponData;
        WeaponRarity groundRarity = weaponNew.weaponRarity;
        float groundDurability = weaponNew.currentDurability; // ค่าที่พื้นจำไว้

        // 2. จำค่าจาก "มือ" ปัจจุบัน
        WeaponInfo handData = weaponHandlerScript.currentWeapon;
        WeaponRarity handRarity = weaponHandlerScript.currentRarity;
        float handDurability = weaponHandlerScript.currentDurability;

        // 3. เอาของจากพื้นขึ้นมือ (พร้อมค่าความทนทานของมัน)
        weaponHandlerScript.SetWeapon(groundData, groundRarity, groundDurability);
        attackTime = weaponHandlerScript.currentWeapon.attackSpeed - (weaponHandlerScript.currentWeapon.attackSpeed * bonusAttackSpeed/100);
        attackDamage = weaponHandlerScript.currentWeapon.baseDamage + weaponHandlerScript.currentRarity.rarityDamage;;

        Debug.Log(weaponHandlerScript.currentWeapon.attackReach);
        HitBox.localScale = new Vector3(0.5f , weaponHandlerScript.currentWeapon.attackReach/1.2f , 0.5f);
        if (handData != null)
        {
            // 4. เอาของจากมือลงพื้น (พร้อมค่าความทนทานที่ใช้ไปแล้ว)
            weaponNew.SetWeapon(handData, handRarity, handDurability);
        }
        else
        {
            Destroy(weaponNew.gameObject);
        }
    }
    void Attack()
    {   
        if (weaponHandlerScript.currentWeapon != null && weaponHandlerScript.currentRarity != null)
        {
            if (weaponHandlerScript.currentWeapon.weaponType == WeaponInfo.WeaponType.Melee)
            {
                if (Input.GetMouseButtonDown(0))
                {   
                    if (attackTimer <= 0)
                    {
                        HitBox.gameObject.SetActive(true);
                        attackTimer = attackTime;
                        baseAngle = GetMouseAngle();

                        string wName = weaponHandlerScript.currentWeapon.weaponName;
                        float finalDmg = weaponHandlerScript.currentWeapon.baseDamage + weaponHandlerScript.currentRarity.rarityDamage;
                        string rName = weaponHandlerScript.currentRarity.rarityName; // สมมติว่ามีชื่อใน SO

                        // 2. ลดความทนทาน (ถ้าพัง currentWeapon จะกลายเป็น null ในบรรทัดนี้)
                        Debug.Log($"ตีด้วย {wName} | ความแรงรวม: {finalDmg} | ระดับ: {rName} | ความเร็วโจมตี: {weaponHandlerScript.currentWeapon.attackSpeed} |ความคงทนเหลือ: {weaponHandlerScript.currentDurability}");
                        

                        // 4. เช็กเสริมเผื่ออยากรู้ว่าพังหรือยัง
                        if (weaponHandlerScript.currentWeapon == null)
                        {
                            Debug.Log("--- อาวุธพังคามือเรียบร้อย! ---");
                        }
                    }
                } 
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
                 // ฟังก์ชันที่เจมส์ใช้คำนวณองศาเมาส์

                if (isMouseOnLeft)
                {
                    if (attackTimer > 0)
                    {   
                        float progress = attackTimer / attackTime;
                        float startOffset = -45f - 180f;
                        float endOffset = 90f - 180f;
                        float currentOffset = Mathf.Lerp(endOffset, startOffset, progress);
                        handTransform.localRotation = Quaternion.Euler(0, 0, baseAngle + currentOffset);
                    }
                }
                else if (!isMouseOnLeft)
                {
                    if (attackTimer > 0)
                    {   
                        float progress = attackTimer / attackTime;
                        float startOffset = 45f;
                        float endOffset = -90f;
                        float currentOffset = Mathf.Lerp(endOffset, startOffset, progress);
                        handTransform.localRotation = Quaternion.Euler(0, 0, baseAngle + currentOffset);
                    }
                }
            }
            else if (weaponHandlerScript.currentWeapon.weaponType == WeaponInfo.WeaponType.Range)
            {

            }
        }
    }
    public void ResetAttack()
    {
        attackTimer = 0; // หยุดการนับเวลาโจมตี
        HitBox.gameObject.SetActive(false); // ปิด Hitbox ทันที
        // คืนค่าตำแหน่งหมุนของมือให้เป็นปกติ (0 องศา หรือตำแหน่งเริ่มต้น)
        handTransform.localRotation = Quaternion.identity; 
    }
    void HandleFlipAndMovementLogic()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - handTransform.position;
        
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (attackTimer <= 0)
        {   
            HitBox.gameObject.SetActive(false);
            isMouseOnLeft = mousePos.x < transform.position.x;
            if (isMouseOnLeft)
            {
                handTransform.rotation = Quaternion.Euler(0, 0, angle + 180f);
                if (handSR != null) handSR.sortingOrder = 3;
                if (weaponSR != null) weaponSR.sortingOrder = 2;
                if (weaponHandler.localPosition.x > 0)
                {
                    weaponHandler.localPosition = new Vector3(-weaponHandler.localPosition.x, weaponHandler.localPosition.y, weaponHandler.localPosition.z);
                    HitBox.localPosition = new Vector3(-HitBox.localPosition.x, HitBox.localPosition.y, HitBox.localPosition.z);
                }
            }   
            else
            {
                handTransform.rotation = Quaternion.Euler(0, 0, angle);
                if (handSR != null) handSR.sortingOrder = -2;
                if (weaponSR != null) weaponSR.sortingOrder = -1;
                if (weaponHandler.localPosition.x < 0)
                {
                    weaponHandler.localPosition = new Vector3(-weaponHandler.localPosition.x, weaponHandler.localPosition.y, weaponHandler.localPosition.z);
                    HitBox.localPosition = new Vector3(-HitBox.localPosition.x, HitBox.localPosition.y, HitBox.localPosition.z);
                }
            }

            foreach (SpriteRenderer part in characterParts)
            {
                if (part != null)
                {
                    part.flipX = isMouseOnLeft;
                }
            }
        }

        

        float currentSpeed = speed;
        bool isBackstepping = false;        

        if (moveInput.x != 0)
        {
            if (isMouseOnLeft)
            {
                if (moveInput.x > 0)
                {
                    isBackstepping = false;
                    currentSpeed = speed * 0.5f;
                }
                else if (moveInput.x < 0)
                {
                    isBackstepping = true;
                    currentSpeed = speed;
                }
            }
            else if (!isMouseOnLeft)
            {   
                if (moveInput.x > 0)
                {
                    isBackstepping = false;
                    currentSpeed = speed;
                }
                else if (moveInput.x < 0)
                {
                    isBackstepping = true;
                    currentSpeed = speed * 0.5f;
                }
            }
        }

        animator.SetBool("isMove", moveInput != Vector2.zero);
        animator.SetBool("isBackstep", isBackstepping);

        
        rb.linearVelocity = moveInput * currentSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Weapon"))
        {
            ItemPickup item = collision.gameObject.GetComponent<ItemPickup>();

            if (item != null && item.weaponData != null)
            {
                Debug.Log("เจอไอเทม: " + item.weaponData.weaponName + 
          "\nระดับความหายาก: " + item.weaponRarity.rarityName + 
          "\nดาเมจรวม: " + item.FinalDamage + 
          "\nความเร็วรวม: " + item.weaponData.attackSpeed);

                weaponInRange = true;
                weaponNew = item;
            }
        }
        if (collision.gameObject.CompareTag("Hand"))
        {
            isHand = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Weapon"))
        {
            weaponInRange = false;
            weaponNew = null;
        }
    }
}