using System.Security.Cryptography.X509Certificates;
using System;
using UnityEngine;

public class DetectionArea : MonoBehaviour
{
    public bool playerDetect;
    public bool handDetect;
    public bool isExit;
    private float timer;
    void Start()
    {
        timer = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isExit)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (timer <= 0)
            {
                playerDetect = false;
                isExit = false;
                timer = 1f;
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=Red>Player entered detectionArea!</color>");
            playerDetect = true;
            handDetect = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (transform.parent.name.Contains("BlackGuy")) 
            {
                isExit = true;
            }
        }
    }
}
