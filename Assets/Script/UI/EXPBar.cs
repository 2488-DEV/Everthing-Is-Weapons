using UnityEngine;
using UnityEngine.UI;

public class EXPBar : MonoBehaviour
{
    private Player player;
    public Slider slider;
    public float maxValue;
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
        }
    }

    void Update()
    {
        slider.maxValue = player.maxEXP;
        slider.value = player.currentEXP;
    }
}
