using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public bool hitArea;
    
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
            hitArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            hitArea = false;
        }
    }
}
