using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // --- [ส่วนที่เพิ่มเพื่อแก้ปัญหาห้อง 2] ---
            // สั่งให้ตัวเองออกจาก Parent (Canvas) ทันที 
            // เพื่อให้ DontDestroyOnLoad ทำงานได้จริง
            transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ถ้ามีตัวเกินมา (เช่น ในฉากห้อง 2 นายเผลอวางทิ้งไว้) ให้ลบทิ้ง
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}