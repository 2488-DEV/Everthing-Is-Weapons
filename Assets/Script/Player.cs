using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform handTransform;
    public Transform weaponHandler;
    public SpriteRenderer[] characterParts;
    public Rigidbody2D rb;
    public Vector2 moveInput;
    public Animator animator;
    public float speed = 5f;
    private Camera mainCam; 
    private SpriteRenderer handSR;
    private SpriteRenderer weaponSR;
    void Start()
    {
        mainCam = Camera.main; 
        if (characterParts.Length > 4)
        {
            handSR = characterParts[4];
        }

        weaponSR = weaponHandler.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        HandleFlipAndMovementLogic();

        Debug.Log(weaponHandler.transform.position.x);

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }
    void Attack()
    {
        Debug.Log("Attack");
    }
    void HandleFlipAndMovementLogic()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - handTransform.position;
        
        bool isMouseOnLeft = mousePos.x < transform.position.x;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (isMouseOnLeft)
        {
            handTransform.rotation = Quaternion.Euler(0, 0, angle + 180f);
            if (handSR != null) handSR.sortingOrder = 1;
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
}