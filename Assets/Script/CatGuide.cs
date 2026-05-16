using UnityEngine;

public class CatGuide : MonoBehaviour
{
    [SerializeField] private Transform catStopPoint;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stopDistance = 0.1f;

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
