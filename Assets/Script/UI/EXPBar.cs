using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EXPBar : MonoBehaviour
{
    private Player player;
    public Slider slider;
    public Image fillImage;

    [Header("Smooth Settings")]
    public float lerpSpeed = 5f;

    [Header("Level Up Effect")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.3f;
    private Color originalColor;
    private bool isFlashing = false;

    // ทำให้หลอดเต็มโชว์ในหน้า Editor เหมือน HP
    void OnValidate()
    {
        if (slider != null) slider.value = slider.maxValue;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
        }

        if (player != null && slider != null)
        {
            slider.maxValue = player.maxEXP;
            slider.value = player.currentEXP; // วาร์ปไปที่ค่าปัจจุบันทันทีตอนเริ่ม
            if (fillImage != null) originalColor = fillImage.color;
        }
    }

    void Update()
    {
        if (player == null || slider == null) return;

        // --- ระบบเช็ก Level Up ---
        // ถ้าค่า maxEXP ใน Player เปลี่ยน (เลเวลเพิ่ม) ให้รีเซ็ตหลอด
        if (!Mathf.Approximately(slider.maxValue, player.maxEXP))
        {
            // ถ้าค่าใหม่มากกว่าค่าเก่า (Level Up)
            if (player.maxEXP > slider.maxValue)
            {
                slider.value = 0; // ให้หลอดเริ่มวิ่งใหม่จาก 0
                StartCoroutine(LevelUpFlash());
            }
            slider.maxValue = player.maxEXP;
        }

        // --- การไหลของหลอด EXP ---
        if (!Mathf.Approximately(slider.value, player.currentEXP))
        {
            slider.value = Mathf.Lerp(slider.value, player.currentEXP, Time.deltaTime * lerpSpeed);
        }
    }

    IEnumerator LevelUpFlash()
    {
        if (isFlashing || fillImage == null) yield break;
        isFlashing = true;

        // วาบสีขาว + ขยายหลอด (Punch Effect)
        transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        fillImage.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        fillImage.color = originalColor;
        transform.localScale = Vector3.one;
        isFlashing = false;
    }
}