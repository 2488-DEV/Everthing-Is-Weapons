using UnityEngine;

public class PlayerSceneController : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 1.5f;
    public LayerMask doorLayer; // แนะนำให้ใช้ Layer เพื่อความไว

    private bool hasTriggered = false;

    void Update()
    {
        if (hasTriggered) return;

        // เช็กประตูรอบตัว
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius);
        if (hit != null && hit.CompareTag("Door"))
        {
            TriggerNextStage();
        }
    }

    // ใช้ Trigger เป็นตัวสำรอง
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Door"))
        {
            TriggerNextStage();
        }
    }

    private void TriggerNextStage()
    {
        hasTriggered = true;
        Debug.Log("[System] เข้าประตู! กำลังคำนวณด่านถัดไป...");

        // เรียกใช้ Logic จาก InGameScript ที่เราอัปเกรดไว้
        InGameScript.NextStage();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}