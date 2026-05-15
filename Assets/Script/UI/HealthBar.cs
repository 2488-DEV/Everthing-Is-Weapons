using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider healthSlider;

    [Header("Smooth Settings")]
    public float lerpSpeed = 10f;

    void Start()
    {
        // เมื่อเริ่มฉากใหม่ ให้เซ็ตค่าหลอดเลือดให้ตรงกับค่าจริงของ Player ทันที
        SyncHealthWithPlayer();
    }

    // ฟังก์ชันช่วยซิงค์ค่าให้ตรงเป๊ะ (ใช้เรียกตอน Start หรือตอนเปลี่ยนฉาก)
    public void SyncHealthWithPlayer()
    {
        if (Player.instance != null && healthSlider != null)
        {
            float currentMax = Player.instance.maxHealth + Player.instance.bonusHealth;
            healthSlider.maxValue = currentMax;
            healthSlider.value = Player.instance.health;
            Debug.Log("<color=cyan>UI HealthBar Synced with Singleton Player</color>");
        }
    }

    void Update()
    {
        // 1. ตรวจสอบ Player.instance ตลอดเวลา (เพราะตัวแปร Singleton อาจมีการ Update ตอนข้ามฉาก)
        if (Player.instance == null || healthSlider == null) return;

        // 2. อัปเดต MaxHealth เผื่อมีการอัปเกรด (เช่น เลเวลอัป)
        float currentMax = Player.instance.maxHealth + Player.instance.bonusHealth;
        if (!Mathf.Approximately(healthSlider.maxValue, currentMax))
        {
            healthSlider.maxValue = currentMax;
        }

        // 3. อัปเดตค่าเลือด (Lerp เพื่อความสวยงาม)
        if (Mathf.Abs(healthSlider.value - Player.instance.health) > 0.01f)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, Player.instance.health, Time.deltaTime * lerpSpeed);
        }
        else
        {
            healthSlider.value = Player.instance.health;
        }
    }
}