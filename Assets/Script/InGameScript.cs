using UnityEngine;

public class InGameScript
{
    public static int currentStage = 0; 
    public static int enemyCount = 0;

    public static void NextStage()
    {
        if (currentStage != 0)
        {
            enemyCount = (int)(enemyCount * 1.05f);
            currentStage += 1;
            Debug.Log("Stage : " + currentStage);
        }
        else
        {
            currentStage += 1;
            enemyCount = 3;
            Debug.Log("Stage : " + currentStage);
        }
    } 
    public static void ResetGame()
    {
        currentStage = 0;
        enemyCount = 0;
    }
}
