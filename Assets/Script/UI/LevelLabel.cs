using UnityEngine;
using TMPro;
using System.Collections;

public class LevelLabel : MonoBehaviour
{
    public TextMeshProUGUI levelLabel;
    private Player player;

    private int lastLevel; // เอาไว้เช็กว่าเลเวลเปลี่ยนหรือยัง

    [Header("Level Up Animation")]
    public Color levelUpColor = Color.yellow;
    public float punchScale = 1.3f;
    public float duration = 0.4f;

    private Color originalColor;
    private Vector3 originalScale;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
            lastLevel = player.level;
        }

        if (levelLabel != null)
        {
            originalColor = levelLabel.color;
            originalScale = levelLabel.transform.localScale;
            UpdateLabel();
        }
    }

    void Update()
    {
        if (player == null) return;

        // เช็กว่าเลเวลปัจจุบันต่างจากเลเวลล่าสุดที่จำได้ไหม
        if (player.level != lastLevel)
        {
            lastLevel = player.level;
            UpdateLabel();
            StopAllCoroutines();
            StartCoroutine(LevelUpReaction());
        }
    }

    void UpdateLabel()
    {
        levelLabel.text = "Level : " + player.level;
    }

    IEnumerator LevelUpReaction()
    {
        // 1. เด้งตัวเลขขึ้นมาและเปลี่ยนสี
        levelLabel.transform.localScale = originalScale * punchScale;
        levelLabel.color = levelUpColor;

        // 2. ค่อยๆ คืนค่ากลับเป็นปกติ (Smooth Return)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            levelLabel.transform.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, t);
            levelLabel.color = Color.Lerp(levelUpColor, originalColor, t);

            yield return null;
        }

        // คืนค่าแม่นยำตอนจบ
        levelLabel.transform.localScale = originalScale;
        levelLabel.color = originalColor;
    }
}