using UnityEngine;

public class pLaYerDeRNnaJa : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform flip;
    public float moveInputX;
    public float moveInputY;
    public float moveSpeed;

    private Vector2 moveDirection;

    void Update()
    {
        moveInputX = Input.GetAxis("Horizontal");
        moveInputY = Input.GetAxis("Vertical");
        moveDirection = new Vector2(moveInputX,moveInputY).normalized;
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    
        if(moveInputX >= 1)
        {
            flip.eulerAngles = new Vector3(0, 180, 0);  
        }
        else if(moveInputX <= -1)
        {
            flip.eulerAngles = new Vector3(0, 0, 0);  
        }
    }
}
