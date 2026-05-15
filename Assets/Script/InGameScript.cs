using UnityEngine;
using UnityEngine.SceneManagement;

public static class InGameScript
{
    public static int currentStage = 0;
    public static int enemyCount = 0;

    // ตั้งชื่อให้ตรงกับ Scene ใน Build Settings ของนาย
    private static string[] stageScenes = { "inGame_1", "inGame_2", "inGame_3", "inGame_4", "inGame_5" };

    public static void NextStage()
    {
        currentStage++;

        // 1. เช็กจบเกมที่ด่าน 25
        if (currentStage > 25)
        {
            Debug.Log("<color=gold>จบเกม! นายคือผู้ชนะ</color>");
            ResetGame();
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // 2. เพิ่มความยาก
        enemyCount = Mathf.CeilToInt(enemyCount * 1.1f) + 1;

        // 3. เลือกฉากถัดไปตาม Logic 1-5
        string nextSceneName = "";

        if (currentStage == 1 || (currentStage - 1) % 5 == 0)
        {
            nextSceneName = stageScenes[0]; // กลับไป inGame_1
        }
        else if (currentStage % 5 == 0)
        {
            nextSceneName = stageScenes[4]; // ไปห้องบอส inGame_5
            Debug.Log("<color=red>BOSS STAGE!</color>");
        }
        else
        {
            // สุ่มด่าน 2, 3, 4
            int randomIndex = Random.Range(1, 4);
            nextSceneName = stageScenes[randomIndex];
        }

        SceneManager.LoadScene(nextSceneName);
    }

    public static void ResetGame()
    {
        currentStage = 1;
        enemyCount = 3;
    }
}