using UnityEngine;

public class PaperProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    private Vector2 moveDirection;

    // ฟังก์ชันสำหรับตั้งค่าทิศทางจากตัวบอส
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        
        // แถม: หมุนใบกระดาษให้หันไปตามทิศที่พุ่ง (ถ้าอยากให้เท่)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // สั่งลดเลือด Player ตรงนี้ (อิงตามสคริปต์ Player ของเจมส์)
            collision.GetComponent<Player>().health -= damage;
            Destroy(gameObject); // ชนแล้วหายไป
        }
    }
}