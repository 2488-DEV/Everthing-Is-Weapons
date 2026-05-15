using UnityEngine;

public class DetectionArea : MonoBehaviour
{
    [Header("Detection Settings")]
    public bool playerDetect;
    public bool handDetect;
    public float exitDelay = 1f; // ตั้งเวลาหน่วงจาก Inspector ได้เลย

    [Header("Status")]
    public bool isExit;
    private float timer;

    void Start()
    {
        timer = exitDelay;
    }

    void Update()
    {
        // ระบบหน่วงเวลาหลังจากผู้เล่นออกจากระยะ
        if (isExit)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                playerDetect = false;
                handDetect = false; // ปิดการจับมือ/อาวุธด้วย
                isExit = false;
                timer = exitDelay;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("<color=cyan>DetectionArea:</color> เจอตัว Player แล้ว!");
            playerDetect = true;
            handDetect = true;

            // ถ้าเดินกลับเข้ามาในระยะ ให้ยกเลิกการนับถอยหลังเลิกตาม
            isExit = false;
            timer = exitDelay;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // เช็กว่าศัตรูตัวนี้ต้องการระบบหน่วงเวลาไหม (เช่น BlackEnemy หรือ Boss)
            // วิธีที่ปลอดภัยกว่าการเช็กชื่อคือการเช็ก Tag ของ Parent หรือใช้ตัวแปรคุมครับ
            if (transform.parent != null &&
               (transform.parent.name.Contains("BlackGuy") || transform.parent.CompareTag("Enemy")))
            {
                isExit = true;
            }
            else
            {
                // ถ้าเป็นลูกกระจ๊อกทั่วไป ให้เลิกตามทันที
                playerDetect = false;
                handDetect = false;
            }
        }
    }
}