using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
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
        slider.maxValue = player.maxHealth;
        slider.value = player.health;
    }
}
