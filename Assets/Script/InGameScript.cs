using UnityEngine;
using UnityEngine.SceneManagement;

public static class InGameScript
{
    public static int currentStage = 0;
    public static int enemyCount = 0;

    // ตั้งชื่อให้ตรงกับ Scene ใน Build Settings ของนาย

    public static void NextStage()
    {
        if (currentStage == 0)
        {
            currentStage = 1;
            enemyCount = 3;
        }
        else
        {
            currentStage++;
            enemyCount = Mathf.CeilToInt(enemyCount * 1.1f) + 1;
        }
    }

    public static void ResetGame()
    {
        currentStage = 1;
        enemyCount = 3;
    }
}