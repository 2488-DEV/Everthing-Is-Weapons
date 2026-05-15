using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float slowMultiplier = 0.8f;
    public float slowDuration = 3f;
    public GameObject webVisualPrefab;
    private Vector2 moveDirection;
    private float lifetimeTimer;

    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.simulated = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        if (GetComponent<SpriteRenderer>() == null)
            gameObject.AddComponent<SpriteRenderer>();
    }

    void Start()
    {
        lifetimeTimer = 5f;
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Web hit: {collision.name}, tag: {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player == null) player = collision.GetComponentInParent<Player>();

            if (player != null)
            {
                Debug.Log("Web: Applying slow to player");
                player.ApplyWebDebuff(slowMultiplier, slowDuration, webVisualPrefab);
            }
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Web collision (non-trigger): {collision.gameObject.name}");
    }
}
