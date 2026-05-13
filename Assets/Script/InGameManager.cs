using UnityEngine;

public class InGameManager : MonoBehaviour
{
    // เราจะไม่ดักจับ Update() ที่นี่แล้ว เพราะ MainMenuControl ทำหน้าที่นั้นให้แล้วครับ
    // สคริปต์นี้เอาไว้ช่วยจัดการสิ่งที่เกิดขึ้นเฉพาะในด่านนั้นๆ แทน

    [Header("References")]
    public MainMenuControl uiControl; // ลาก Object ที่ถือสคริปต์ MainMenuControl มาใส่

    void Start()
    {
        // ตรวจสอบความเรียบร้อยเฉยๆ
        if (uiControl == null)
        {
            uiControl = Object.FindFirstObjectByType<MainMenuControl>();
        }

        // มั่นใจว่าเริ่มเกมมาเวลาต้องเดินปกติ
        Time.timeScale = 1f;
    }

    // ถ้านายอยากมีปุ่ม Resume ในฉากที่ไม่ได้อยู่ใน SettingPanel 
    // สามารถสั่งผ่านฟังก์ชันนี้ได้
    public void ResumeGameFromButton()
    {
        if (uiControl != null)
        {
            uiControl.CloseSetting();
        }
    }
}