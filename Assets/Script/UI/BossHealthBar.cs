using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    private BossEnemy bossScript;
    public Slider mainSlider;
    public Slider easeSlider;
    public TextMeshProUGUI bossLabel;
    public GameObject healthUI;

    [Header("Health Colors")]
    public Color normalColor = Color.red;
    public Color warningColor = Color.yellow;
    public Color criticalColor = new Color(1f, 0.3f, 0f);
    private Image mainFillImage;

    [Header("Settings")]
    public float lerpSpeed = 0.05f;
    private float findTimer = 0f;

    void Start()
    {
        if (healthUI != null) healthUI.SetActive(false);

        if (mainSlider != null && mainSlider.fillRect != null)
        {
            mainFillImage = mainSlider.fillRect.GetComponent<Image>();
        }
    }

    void Update()
    {
        // --- ส่วนที่แก้ไข: เช็กว่าเป็นด่านบอส (5, 10, 15, 20, 25) และต้องไม่ใช่ด่านแรก ---
        // ใช้ currentStage > 1 เพื่อป้องกันกรณีเริ่มเกมมาที่ด่าน 1 แล้วหลอดบอสเด้ง (ถ้าหารลงตัวพอดี)
        bool isBossStage = (InGameScript.currentStage % 5 == 0 && InGameScript.currentStage > 1);

        if (!isBossStage)
        {
            // ถ้าไม่ใช่ด่านบอส ปิด UI ทันที และเคลียร์ค่าทิ้ง
            if (healthUI != null && healthUI.activeSelf)
                healthUI.SetActive(false);

            bossScript = null;
            return;
        }

        // --- ตั้งแต่ตรงนี้ลงไปจะทำงานเฉพาะในด่านบอสเท่านั้น ---

        if (bossScript == null)
        {
            findTimer += Time.deltaTime;
            if (findTimer >= 0.5f)
            {
                findTimer = 0f;
                GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
                if (bossObj != null)
                {
                    bossScript = bossObj.GetComponent<BossEnemy>();
                    InitializeUI();
                }
            }
            return;
        }

        UpdateHealthVisuals();
        UpdateSliderColor();
    }

    void InitializeUI()
    {
        if (bossScript != null && bossScript.enemyInfo != null)
        {
            healthUI.SetActive(true);
            bossLabel.text = bossScript.enemyInfo.enemyName;

            mainSlider.maxValue = bossScript.maxHealth;
            easeSlider.maxValue = bossScript.maxHealth;

            mainSlider.value = bossScript.health;
            easeSlider.value = bossScript.health;

            if (mainFillImage != null) mainFillImage.color = normalColor;
        }
    }

    void UpdateHealthVisuals()
    {
        if (bossScript == null) return;

        mainSlider.value = bossScript.health;

        if (easeSlider.value != mainSlider.value)
        {
            easeSlider.value = Mathf.Lerp(easeSlider.value, mainSlider.value, lerpSpeed);
        }

        // ถ้าบอสตายหรือถูกทำลาย ให้ปิด UI
        if (!bossScript.gameObject.activeInHierarchy || bossScript.health <= 0)
        {
            Invoke("HideUI", 1.5f);
        }
    }

    void UpdateSliderColor()
    {
        if (mainFillImage == null || bossScript == null) return;

        float healthPercent = (bossScript.health / (float)bossScript.maxHealth) * 100f;

        if (healthPercent <= 20f) mainFillImage.color = criticalColor;
        else if (healthPercent <= 50f) mainFillImage.color = warningColor;
        else mainFillImage.color = normalColor;
    }

    void HideUI()
    {
        if (bossScript == null || bossScript.health <= 0)
        {
            if (healthUI != null) healthUI.SetActive(false);
        }
    }
}