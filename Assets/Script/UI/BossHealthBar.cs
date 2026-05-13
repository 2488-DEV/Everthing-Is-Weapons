using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    private BossEnemy bossScript; 
    public Slider slider;
    public TextMeshProUGUI bossLabel;
    public GameObject healthUI;

    void Update()
    {
        if (InGameScript.currentStage == 5 || InGameScript.currentStage == 10 || InGameScript.currentStage == 15 || InGameScript.currentStage == 20 || InGameScript.currentStage == 25)
        {
            healthUI.SetActive(true);
        }
        // 1. ถ้ายังไม่มีตัวแปรบอส ให้พยายามหา (ใช้ Tag "Boss" ตามที่เจมส์ตั้งไว้)
        if (bossScript == null)
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
            if (bossObj != null)
            {
                bossScript = bossObj.GetComponent<BossEnemy>();
                
                // เซตค่า MaxValue ของ Slider ครั้งเดียวตอนเจอบอส
                if (bossScript != null && bossScript.enemyInfo != null)
                {
                    slider.maxValue = bossScript.maxHealth; 
                }
            }
            return; // ถ้ายังไม่เจอก็ไม่ต้องรันบรรทัดล่าง
        }

        // 2. ถ้ามีตัวแปรบอสแล้ว ให้อัปเดตเลือดตลอดเวลา
        slider.value = bossScript.health;
        bossLabel.text = bossScript.enemyInfo.enemyName;

        // 3. ถ้าบอสตาย (SetActive false) ให้ปิดหลอดเลือดไปเลย
        if (!bossScript.gameObject.activeInHierarchy)
        {
            healthUI.SetActive(false);
        }
    }
}