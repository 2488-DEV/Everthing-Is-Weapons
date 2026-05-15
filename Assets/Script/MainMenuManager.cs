using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        // ทำลาย Player ตัวเก่าทิ้งถ้ามันหลงมาหน้าเมนู
        if (Player.instance != null)
        {
            Destroy(Player.instance.gameObject);
        }

        // ทำลาย UI ตัวเก่าทิ้ง
        if (UIManager.instance != null)
        {
            Destroy(UIManager.instance.gameObject);
        }

        // คืนค่าเวลา (เผื่อเกมค้างตอน Pause/LevelUp)
        Time.timeScale = 1f;
    }
}