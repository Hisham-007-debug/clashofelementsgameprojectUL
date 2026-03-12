using UnityEngine;

public class HitboxController : MonoBehaviour
{
    [Header("Attack Data")]
    public float damage = 10f;
    public float knockbackForce = 5f;
    public float hitStunDuration = 0.3f;

    private Collider2D col;
    private GameObject owner; // the fighter this hitbox belongs to

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;

        // Owner is the root fighter object
        owner = transform.root.gameObject;
    }

    // Called by Animation Events
    public void EnableHitbox()
    {
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        col.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit yourself
        if (other.gameObject == owner) return;
        if (other.transform.root.gameObject == owner) return;

        // Only hit fighters tagged "Player"
        if (!other.CompareTag("Player")) return;

        FighterHealth health = other.transform.root.GetComponent<FighterHealth>();
        PlayerMovementAir controller = other.transform.root.GetComponent<PlayerMovementAir>();

        if (health != null)
            health.TakeDamage(damage);

        if (controller != null)
            controller.ApplyHitStun(hitStunDuration, knockbackForce, transform.root.position);

        // Disable after landing hit (prevents multi-hit on same swing)
        DisableHitbox();
    }
}
