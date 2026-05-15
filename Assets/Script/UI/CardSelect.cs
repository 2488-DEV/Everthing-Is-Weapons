using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // ถ้าใช้ Text ธรรมดาให้ลบบรรทัดนี้แล้วเปลี่ยน TextMeshProUGUI เป็น Text

public class CardSelect : MonoBehaviour
{
    // --- ระบบเก็บ Reference UI ของแต่ละใบ ---
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
    public CardUIElements[] cardSlots; // ปรับใน Inspector เป็น 3 ช่อง แล้วลากใส่ได้เลย

    [Header("Settings")]
    public UpgradeCard[] oneStarCard;
    public UpgradeCard[] twoStarCard;
    public UpgradeCard[] threeStarCard;
    
    private Player player;
    private UpgradeCard[] currentlyDisplayedCards = new UpgradeCard[3];

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<Player>();
        selectionUI.SetActive(false);

        for (int i = 0; i < cardSlots.Length; i++)
        {
            int index = i; // สำคัญมาก: ต้องสร้างตัวแปรมาพักค่า i ไว้ ไม่งั้นพอกดปุ่มมันจะจำแต่ค่าสุดท้าย
            cardSlots[index].cardButton.onClick.AddListener(() => OnClickCard(index));
        }
    }

    public void ShowCard()
    {
        selectionUI.SetActive(true);
        List<UpgradeCard> chosenCards = new List<UpgradeCard>();

        for (int i = 0; i < cardSlots.Length; i++)
        {
            UpgradeCard selectedSO = null;
            bool isDuplicate = true;
            int safetyNet = 0;

            while (isDuplicate && safetyNet < 50) 
            {
                float rand = UnityEngine.Random.value;
                if (rand < 0.6f) selectedSO = GetRandomCardFromArray(oneStarCard);
                else if (rand < 0.9f) selectedSO = GetRandomCardFromArray(twoStarCard);
                else selectedSO = GetRandomCardFromArray(threeStarCard);

                if (!chosenCards.Contains(selectedSO)) isDuplicate = false;
                safetyNet++;
            }

            if (selectedSO != null)
            {
                chosenCards.Add(selectedSO);
                currentlyDisplayedCards[i] = selectedSO; // เก็บค่าไว้ดึงมาดูตอนกดปุ่ม
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

        cardSlots[index].star1.SetActive(selectedSO.starCount == 1);
        cardSlots[index].star2.SetActive(selectedSO.starCount == 2);
        cardSlots[index].star3.SetActive(selectedSO.starCount == 3);
    }

    private UpgradeCard GetRandomCardFromArray(UpgradeCard[] cardArray)
    {
        if (cardArray == null || cardArray.Length == 0) return null;
        return cardArray[UnityEngine.Random.Range(0, cardArray.Length)];
    }

    public void OnClickCard(int index)
    {
        if (currentlyDisplayedCards[index] != null)
        {
            UpgradeCard selected = currentlyDisplayedCards[index]; 

            if (selected != null) // เช็กว่ามีการ์ดในช่องนี้จริงๆ
            {
                Debug.Log("เลือกการ์ด: " + selected.cardName);

                switch (selected.cardName)
                {
                    case "AttackBoost":
                        Debug.Log("เพิ่มดาเมจแล้วจ้า");
                        player.bonusAttackDamage += currentlyDisplayedCards[index].additivePercent;
                        break;
                    case "AttackSpeed":
                        Debug.Log("เพิ่มความเร็วแล้วจ้า");
                        player.bonusAttackSpeed += currentlyDisplayedCards[index].additivePercent;
                        player.attackTime = player.weaponHandlerScript.currentWeapon.attackSpeed - (player.weaponHandlerScript.currentWeapon.attackSpeed * player.bonusAttackSpeed/100);
                        break;
                    case "HealthBoost":
                        Debug.Log("เพิ่มเลือดแล้วจ้า");
                        player.maxHealth += player.maxHealth * (currentlyDisplayedCards[index].additivePercent/100);
                        break;
                    case "KMITL Heart":
                        Debug.Log("หัวใจลาดกระบัง!");
                        player.health += currentlyDisplayedCards[index].additivePercent;
                        break;
                    default:
                        Debug.Log("การ์ดใบนี้ยังไม่ได้ตั้งความสามารถ");
                        break;
                }

                if (player != null)
                {
                    player.isSelecting = false;
                }
            }
        }
    }
}