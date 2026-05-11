using UnityEngine;
using TMPro;

public class LevelLabel : MonoBehaviour
{
    public TextMeshProUGUI levelLabel;
    private Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        levelLabel.text = "Level : " + player.level;
    }
}
