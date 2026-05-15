using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ถ้าไม่ใช่ตัวจริง ให้ปิด Object ทิ้งทันที (เผื่อการ Destroy ใช้เวลานิดหน่อย)
            // แล้วค่อยสั่ง Destroy เพื่อลบขยะออกจากฉาก
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
    }
}