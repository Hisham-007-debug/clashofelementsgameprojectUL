using UnityEngine;

public class HitboxController : MonoBehaviour
{
    [Header("Attack Data")]
    public float damage = 10f;
    public float knockbackForce = 2.5f;
    public float hitStunDuration = 0.3f;

    private Collider2D col;
    private GameObject owner; // the fighter this hitbox belongs to
    private bool hasHit;

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
        hasHit = false;
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        col.enabled = false;
        hasHit = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        Debug.Log($"[Hitbox] OnTriggerEnter2D hit: {other.gameObject.name}, tag: {other.tag}, owner: {owner.name}");

        // Don't hit yourself
        if (other.gameObject == owner) return;
        if (other.transform.root.gameObject == owner) return;

        // Only hit fighters tagged "Player"
        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[Hitbox] Ignored {other.gameObject.name} - not tagged Player");
            return;
        }

        FighterHealth health = other.transform.root.GetComponent<FighterHealth>();
        PlayerMovementAir airController = other.transform.root.GetComponent<PlayerMovementAir>();
        PlayerMovementEarth earthController = other.transform.root.GetComponent<PlayerMovementEarth>();

        Debug.Log($"[Hitbox] health={health}, airController={airController}, earthController={earthController}");

        if (health != null)
            health.TakeDamage(damage);

        if (airController != null)
            airController.ApplyHitStun(hitStunDuration, knockbackForce, transform.root.position);
        else if (earthController != null)
            earthController.ApplyHitStun(hitStunDuration, knockbackForce, transform.root.position);

        // Disable after landing hit (prevents multi-hit on same swing)
        hasHit = true;
        DisableHitbox();
    }
}
