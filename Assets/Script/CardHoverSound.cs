using UnityEngine;
using UnityEngine.EventSystems; // จำเป็นมากสำหรับการเช็ค UI

public class CardHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("Settings")]
    public AudioClip hoverClip; // ลากไฟล์เสียงที่จะใช้ตอนเมาส์ชี้มาใส่ที่นี่ ใน Inspector ของการ์ดแต่ละใบ

    public void OnPointerEnter(PointerEventData eventData)
    {
        // เปลี่ยนมาเรียกใช้ MainMenuControl.instance ที่เราเพิ่งอัปเดตไป
        if (MainMenuControl.instance != null && hoverClip != null)
        {
            // เรียกฟังก์ชัน PlayHoverSound ที่อยู่ใน MainMenuControl
            MainMenuControl.instance.PlayHoverSound(hoverClip);
        }
        else
        {
            // ถ้าเสียงไม่ดัง ให้ดูใน Console ว่ามันแจ้งเตือนอันนี้ไหม
            if (MainMenuControl.instance == null)
                Debug.LogWarning("ยังไม่ได้วาง MainMenuControl ไว้ในฉาก หรือลืมใส่ระบบ Singleton ครับนาย!");
        }
    }
}