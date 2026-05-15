using UnityEngine;

public class ThrowableProjectile : MonoBehaviour
{
    [Header("Flight")]
    public float speed = 15f;
    public float spinSpeed = 720f;
    public float lifetime = 5f;

    [Header("Damage")]
    public float damage;

    [Header("Bounce & Fade")]
    public float bounceForce = 8f;
    public float fadeDuration = 1.5f;

    private Vector2 moveDirection;
    private float timer;
    private bool hasHit;
    private bool isFading;
    private float fadeTimer;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(Vector2 direction, float speed, float damage)
    {
        this.speed = speed;
        this.damage = damage;
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (!hasHit)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
            transform.Rotate(0, 0, -spinSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            if (timer >= lifetime)
                StartFadeOut();
        }

        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(fadeTimer / fadeDuration);
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            if (fadeTimer >= fadeDuration)
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.CompareTag("Enemy"))
        {
            MonoBehaviour enemy = FindEnemyComponent(collision);
            if (enemy != null)
            {
                var healthField = enemy.GetType().GetField("health");
                if (healthField != null)
                {
                    float currentHealth = (float)healthField.GetValue(enemy);
                    healthField.SetValue(enemy, currentHealth - damage);
                }

                enemy.SendMessage("ApplyKnockback", transform.position, SendMessageOptions.DontRequireReceiver);
            }

            StartBounce();
        }
    }

    void StartBounce()
    {
        hasHit = true;
        col.enabled = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;
        rb.linearVelocity = Vector2.up * bounceForce;
        rb.constraints = RigidbodyConstraints2D.None;

        StartFadeOut();
    }

    void StartFadeOut()
    {
        if (isFading) return;
        isFading = true;
        fadeTimer = 0;
    }

    private MonoBehaviour FindEnemyComponent(Collider2D collision)
    {
        Transform t = collision.transform;
        MonoBehaviour enemy = (MonoBehaviour)t.GetComponent<BaseEnemy>()
            ?? (MonoBehaviour)t.GetComponent<BlackEnemy>()
            ?? (MonoBehaviour)t.GetComponent<BossEnemy>();
        if (enemy != null) return enemy;
        if (t.parent != null)
            enemy = (MonoBehaviour)t.parent.GetComponent<BaseEnemy>()
                ?? (MonoBehaviour)t.parent.GetComponent<BlackEnemy>()
                ?? (MonoBehaviour)t.parent.GetComponent<BossEnemy>();
        return enemy;
    }
}
