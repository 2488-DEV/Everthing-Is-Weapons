using UnityEngine;
using TMPro;

public class CatGuide : MonoBehaviour
{
    [SerializeField] private Transform catStopPoint;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stopDistance = 0.1f;
    public TextMeshProUGUI text;
    private SpriteRenderer spriteRenderer;
    public bool hasReachedStopPoint;
    private bool playerInRange;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (catStopPoint == null)
        {
            hasReachedStopPoint = true;
        }
        else 
        {
            hasReachedStopPoint = false;
        }
    }

    void Update()
    {   
        if (hasReachedStopPoint)
        {
            if (InGameScript.currentStage == 1)
            {
                text.text = "Every time you hit you'll lose your sanity so be careful";
            } 
            else if (InGameScript.currentStage == 4)
            {
                text.text = "The next room is stronger one";
            } 
            else if (InGameScript.currentStage == 9)
            {
                text.text = "It stronger";
            }
            else if (InGameScript.currentStage == 24)
            {
                text.text = "This is it";
            }
        }   
        if (hasReachedStopPoint || catStopPoint == null || !playerInRange)
            return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            catStopPoint.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, catStopPoint.position) <= stopDistance)
        {
            hasReachedStopPoint = true;
            if (spriteRenderer != null)
                spriteRenderer.flipX = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

}
