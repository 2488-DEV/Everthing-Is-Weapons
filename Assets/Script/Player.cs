using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform handTransform;
    public Transform weaponHandler;
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
    public float attackTime;
    public float attackTimer;

    private bool weaponInRange = false;
    void Start()
    {
        mainCam = Camera.main; 
        if (characterParts.Length > 4)
        {
            handSR = characterParts[4];
        }

        weaponSR = weaponHandler.GetComponent<SpriteRenderer>();
        weaponHandlerScript = weaponHandler.GetComponent<WeaponHandler>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        HandleFlipAndMovementLogic();

        if (weaponHandlerScript.currentWeapon != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
                if (attackTimer <= 0)
                {
                    attackTimer = attackTime;
                }
            } 
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
        }

        if (weaponInRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
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
        attackTime = weaponHandlerScript.currentWeapon.AttackSpeed;
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
            if (attackTimer <= 0)
            {
                string wName = weaponHandlerScript.currentWeapon.weaponName;
                float finalDmg = weaponHandlerScript.currentWeapon.baseDamage + weaponHandlerScript.currentRarity.rarityDamage;
                string rName = weaponHandlerScript.currentRarity.rarityName; // สมมติว่ามีชื่อใน SO

                // 2. ลดความทนทาน (ถ้าพัง currentWeapon จะกลายเป็น null ในบรรทัดนี้)
                float durabilityCost = 10f;
                weaponHandlerScript.DecreaseDurability(durabilityCost);

                // 3. แสดงผลโดยใช้ตัวแปรที่เราจดไว้ (ไม่ไปดึงจาก currentWeapon โดยตรงแล้ว)
                // ใช้ string.Format หรือ interpolation จะอ่านง่ายขึ้นครับเจมส์
                Debug.Log($"ตีด้วย {wName} | ความแรงรวม: {finalDmg} | ระดับ: {rName} | ความเร็วโจมตี: {weaponHandlerScript.currentWeapon.AttackSpeed} |ความคงทนเหลือ: {weaponHandlerScript.currentDurability}");

                // 4. เช็กเสริมเผื่ออยากรู้ว่าพังหรือยัง
                if (weaponHandlerScript.currentWeapon == null)
                {
                    Debug.Log("--- อาวุธพังคามือเรียบร้อย! ---");
                }
            }
        }
    }
    void HandleFlipAndMovementLogic()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - handTransform.position;
        
        bool isMouseOnLeft = mousePos.x < transform.position.x;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (attackTimer <= 0)
        {
            if (isMouseOnLeft)
            {
                handTransform.rotation = Quaternion.Euler(0, 0, angle + 180f);
                if (handSR != null) handSR.sortingOrder = 3;
                if (weaponSR != null) weaponSR.sortingOrder = 2;
                if (weaponHandler.localPosition.x > 0)
                {
                    weaponHandler.localPosition = new Vector3(-weaponHandler.localPosition.x, weaponHandler.localPosition.y, weaponHandler.localPosition.z);
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
          "\nความเร็วรวม: " + item.weaponData.AttackSpeed);

                weaponInRange = true;
                weaponNew = item;
            }
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