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

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<Player>();
        selectionUI.SetActive(false);
    }

    public void ShowCard()
    {
        selectionUI.SetActive(true);

        // สร้าง List เพื่อจำว่ารอบนี้ "สุ่มได้ใบไหนไปบ้างแล้ว"
        List<UpgradeCard> chosenCards = new List<UpgradeCard>();

        for (int i = 0; i < cardSlots.Length; i++)
        {
            UpgradeCard selectedSO = null;
            bool isDuplicate = true;
            int safetyNet = 0; // กันเผื่อกรณีการ์ดใน Array มีไม่พอ แล้วมันจะ Loop ค้าง

            // วนลูปสุ่มใหม่จนกว่าจะได้ใบที่ไม่ซ้ำกับที่เคยสุ่มได้ในรอบนี้
            while (isDuplicate && safetyNet < 50) 
            {
                float rand = UnityEngine.Random.value;
                
                if (rand < 0.6f) selectedSO = GetRandomCardFromArray(oneStarCard);
                else if (rand < 0.9f) selectedSO = GetRandomCardFromArray(twoStarCard);
                else selectedSO = GetRandomCardFromArray(threeStarCard);

                // เช็กว่าใบที่สุ่มได้ (selectedSO) มีอยู่ใน chosenCards หรือยัง
                if (!chosenCards.Contains(selectedSO))
                {
                    isDuplicate = false; // ไม่ซ้ำ! หลุดลูปไปแสดงผลได้
                }
                safetyNet++;
            }

            if (selectedSO != null)
            {
                chosenCards.Add(selectedSO); // บันทึกไว้ว่าใบนี้ถูกเลือกแล้วนะ
                UpdateCardUI(i, selectedSO); // แยกฟังก์ชันอัปเดต UI ออกมาให้โค้ดสะอาดขึ้น
            }
        }
    }

    // ฟังก์ชันช่วยอัปเดต UI (เอาโค้ดเดิมมาใส่ตรงนี้)
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
}