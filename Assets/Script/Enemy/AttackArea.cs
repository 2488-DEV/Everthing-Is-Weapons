using UnityEngine;

public class AttackArea : MonoBehaviour
{
    [Header("Status")]
    public bool hitArea;

    // ตัวแปรไว้เก็บสคริปต์แม่
    private BaseEnemy baseEnemy;
    private BlackEnemy blackEnemy;

    void Start()
    {
        // หาแม่จาก Object ที่เราเกาะอยู่ (Parent)
        baseEnemy = GetComponentInParent<BaseEnemy>();
        blackEnemy = GetComponentInParent<BlackEnemy>();
    }

    private void OnDisable()
    {
        hitArea = false;
        UpdateParentStatus(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            hitArea = true;
            UpdateParentStatus(true);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // ตอกย้ำความเป็นจริง เผื่อมีการพลาดเฟรม
            hitArea = true;
            UpdateParentStatus(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            hitArea = false;
            UpdateParentStatus(false);
        }
    }

    // ฟังก์ชันยัดเยียดค่าให้แม่ ไม่ต้องรอให้แม่มาถาม
    void UpdateParentStatus(bool status)
    {
        if (baseEnemy != null) baseEnemy.hitArea = status;
        if (blackEnemy != null) blackEnemy.hitArea = status;
    }
}