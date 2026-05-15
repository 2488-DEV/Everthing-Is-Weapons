using UnityEngine;

public class ThrowableProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage;
    public float knockbackForce = 3f;
    public float knockbackDuration = 0.15f;
    public float lifetime = 3f;

    private Vector2 moveDirection;
    private float timer;
    private float colliderEnableTime = 0.1f;
    private Collider2D myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        Debug.Log("[Projectile] Alive at " + transform.position);
    }

    public void Setup(Vector2 direction, float speed, float damage, float knockbackForce,
        float knockbackDuration, float lifetime)
    {
        this.speed = speed;
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.knockbackDuration = knockbackDuration;
        this.lifetime = lifetime;

        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        if (myCollider != null && !myCollider.enabled)
        {
            colliderEnableTime -= Time.deltaTime;
            if (colliderEnableTime <= 0)
            {
                myCollider.enabled = true;
                Debug.Log("[Projectile] Collider enabled");
            }
        }

        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        Debug.Log("[Projectile] Destroyed at " + transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("[Projectile] TriggerEnter with: " + collision.name + " tag=" + collision.tag);

        if (collision.CompareTag("Player") || collision.CompareTag("Hand") || collision.CompareTag("Weapon") || collision.CompareTag("PlayerAttackHitBox"))
        {
            Debug.Log("[Projectile] Ignored (player-related)");
            return;
        }

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
        }

        Debug.Log("[Projectile] Destroyed by: " + collision.name + " tag=" + collision.tag);
        Destroy(gameObject);
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
