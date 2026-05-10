using UnityEngine;

public class DetectionArea : MonoBehaviour
{
    public bool playerDetect;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=Red>Player entered detectionArea!</color>");
            playerDetect = true;
        }
    }
}
