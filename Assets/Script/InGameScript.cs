using UnityEngine;

public class InGameScript
{
    public static int currentStage = 5; 
    public static int enemyCount = 0;

    public static void NextStage()
    {
        if (currentStage != 0)
        {
            enemyCount = (int)(enemyCount * 1.05f);
            currentStage += 1;
        }
        else
        {
            currentStage += 1;
            enemyCount = 3;
        }
    } 
    public static void ResetGame()
    {
        currentStage = 0;
        enemyCount = 0;
    }
}
