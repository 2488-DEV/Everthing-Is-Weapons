using UnityEngine;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;

    void Awake()
    {
        // ถ้ามี Manager ตัวอื่นอยู่แล้ว (จากการย้อนกลับมาฉากเดิม) ให้ทำลายตัวใหม่ทิ้ง
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        // สั่งให้ Object นี้ "ห้ามตาย" เมื่อเปลี่ยน Scene
        DontDestroyOnLoad(gameObject);
    }
}