using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerActionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image playerImage;      // ลากรูปตัวละครมาใส่กวัก
    public GameObject fireUI;      // ลาก Object ไอคอนไฟมาใส่กวัก

    [Header("Sprites")]
    public Sprite idleSprite;      // รูปหุบปาก
    public Sprite actionSprite;    // รูปอ้าปาก

    [Header("Settings")]
    public AudioSource actionSound;
    public float duration = 3f;    // ตั้งเวลาได้ตรงนี้กวัก

    private bool isActing = false;

    public void PlayAction()
    {
        if (!isActing)
        {
            StartCoroutine(ActionSequence());
        }
    }

    IEnumerator ActionSequence()
    {
        isActing = true;

        // --- เริ่มต้น: อ้าปาก + ไฟขึ้น + เล่นเสียง ---
        playerImage.sprite = actionSprite;
        if (fireUI != null) fireUI.SetActive(true); // เปิดการแสดงผลไฟกวัก
        if (actionSound != null) actionSound.Play();

        // รอตามเวลาที่ตั้งไว้ (3 วินาที)
        yield return new WaitForSeconds(duration);

        // --- จบ: กลับไปหุบปาก + ปิดไฟ ---
        playerImage.sprite = idleSprite;
        if (fireUI != null) fireUI.SetActive(false); // ปิดการแสดงผลไฟกวัก

        isActing = false;
    }
}