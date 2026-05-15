using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CardSelect : MonoBehaviour
{
    [System.Serializable]
    public class CardUIElements
    {
        public Button cardButton;
        public Image cardImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
        public GameObject star1, star2, star3;
    }

    [Header("UI References")]
    public GameObject selectionUI;
    public CardUIElements[] cardSlots;

    [Header("Card Pools")]
    public UpgradeCard[] oneStarCard;
    public UpgradeCard[] twoStarCard;
    public UpgradeCard[] threeStarCard;

    private Player player;
    private UpgradeCard[] currentlyDisplayedCards = new UpgradeCard[3];

    void Awake()
    {
        // ใช้ Awake เพื่อหา Player ให้เจอก่อนเริ่มเกม
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<Player>();
    }

    void Start()
    {
        selectionUI.SetActive(false);

        for (int i = 0; i < cardSlots.Length; i++)
        {
            int index = i;
            cardSlots[index].cardButton.onClick.AddListener(() => OnClickCard(index));
        }
    }

    public void ShowCard()
    {
        selectionUI.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลาทันทีที่โชว์การ์ด

        List<UpgradeCard> chosenCards = new List<UpgradeCard>();

        for (int i = 0; i < cardSlots.Length; i++)
        {
            UpgradeCard selectedSO = null;
            int safetyNet = 0;

            while (safetyNet < 100)
            {
                float rand = UnityEngine.Random.value;
                if (rand < 0.6f) selectedSO = GetRandomCardFromArray(oneStarCard);
                else if (rand < 0.9f) selectedSO = GetRandomCardFromArray(twoStarCard);
                else selectedSO = GetRandomCardFromArray(threeStarCard);

                // ตรวจสอบว่าใบนี้ถูกเลือกไปหรือยังในรอบนี้
                if (selectedSO != null && !chosenCards.Contains(selectedSO)) break;
                safetyNet++;
            }

            if (selectedSO != null)
            {
                chosenCards.Add(selectedSO);
                currentlyDisplayedCards[i] = selectedSO;
                UpdateCardUI(i, selectedSO);
            }
        }
    }

    private void UpdateCardUI(int index, UpgradeCard selectedSO)
    {
        if (cardSlots[index] == null) return;

        cardSlots[index].cardImage.sprite = selectedSO.cardIMG;
        cardSlots[index].nameText.text = selectedSO.cardName;
        cardSlots[index].descText.text = selectedSO.cardDescription;

        // ปรับให้โชว์ดาวแบบสะสม (1 ดาวเปิดดวงแรก, 3 ดาวเปิดหมด)
        int stars = selectedSO.starCount;
        cardSlots[index].star1.SetActive(stars >= 1);
        cardSlots[index].star2.SetActive(stars >= 2);
        cardSlots[index].star3.SetActive(stars >= 3);
    }

    private UpgradeCard GetRandomCardFromArray(UpgradeCard[] cardArray)
    {
        if (cardArray == null || cardArray.Length == 0) return null;
        return cardArray[UnityEngine.Random.Range(0, cardArray.Length)];
    }

    public void OnClickCard(int index)
    {
        UpgradeCard selected = currentlyDisplayedCards[index];
        if (selected == null || player == null) return;

        Debug.Log($"<color=green>Activated:</color> {selected.cardName}");

        // ใช้การเช็กแบบอิงตามความสามารถใน SO จะยืดหยุ่นกว่าเช็กชื่อ string
        ApplyUpgrade(selected);

        // ปิด UI และคืนสถานะให้ Player
        selectionUI.SetActive(false);
        player.isSelecting = false;

        // หมายเหตุ: Time.timeScale จะถูกตั้งกลับเป็น 1f ใน Player.cs ผ่าน Coroutine อยู่แล้ว
    }

    private void ApplyUpgrade(UpgradeCard card)
    {
        float value = card.additivePercent;

        // เช็กชื่อการ์ด (หรือจะเช็ก Enum ใน SO ก็ได้ถ้าทำไว้)
        switch (card.cardName)
        {
            case "AttackBoost":
                player.bonusAttackDamage += value;
                break;

            case "AttackSpeed":
                player.bonusAttackSpeed += value;
                // อัปเดตความเร็วโจมตีปัจจุบันทันที
                if (player.weaponHandlerScript.currentWeapon != null)
                {
                    float baseSpeed = player.weaponHandlerScript.currentWeapon.attackSpeed;
                    player.attackTime = baseSpeed * (1 - (player.bonusAttackSpeed / 100f));
                }
                break;

            case "HealthBoost":
                // เพิ่ม Max Health และเพิ่มเลือดปัจจุบันตามสัดส่วน
                float oldMax = player.maxHealth;
                player.maxHealth += oldMax * (value / 100f);
                player.health += player.maxHealth - oldMax; // แถมเลือดให้ตามที่เพิ่มมา
                break;

            case "KMITL Heart":
                // หัวใจลาดกระบัง ฮีลเลือดดิบๆ แต่ไม่เกิน Max
                player.health = Mathf.Min(player.health + value, player.maxHealth);
                Debug.Log("พลังแห่งลาดกระบังฟื้นฟูเลือด!");
                break;

            default:
                Debug.LogWarning("ยังไม่ได้ตั้ง Logic สำหรับ: " + card.cardName);
                break;
        }
    }
}