using UnityEngine;

public class HitboxController : MonoBehaviour
{
    [Header("Attack Data")]
    public float damage = 10f;
    public float knockbackForce = 1.0f;
    public float hitStunDuration = 0.3f;

    public System.Action onSuccessfulHit;

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

    void OnTriggerEnter2D(Collider2D other) => ProcessHit(other);

    // OnTriggerStay2D handles the case where the opponent is already overlapping
    // when the hitbox is re-enabled (e.g. chained attacks), since Unity won't
    // re-fire OnTriggerEnter2D in that scenario.
    void OnTriggerStay2D(Collider2D other) => ProcessHit(other);

    private void ProcessHit(Collider2D other)
    {
        if (hasHit) return;
        Debug.Log($"[Hitbox] ProcessHit: {other.gameObject.name}, tag: {other.tag}, owner: {owner.name}");

        if (other.gameObject == owner) return;
        if (other.transform.root.gameObject == owner) return;

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[Hitbox] Ignored {other.gameObject.name} - not tagged Player");
            return;
        }

        FighterHealth health = other.transform.root.GetComponent<FighterHealth>();
        PlayerMovementAir   airController   = other.transform.root.GetComponent<PlayerMovementAir>();
        PlayerMovementEarth earthController = other.transform.root.GetComponent<PlayerMovementEarth>();
        PlayerMovementFire  fireController  = other.transform.root.GetComponent<PlayerMovementFire>();

        Debug.Log($"[Hitbox] health={health}, airController={airController}, earthController={earthController}, fireController={fireController}");

        if (health != null)
            health.TakeDamage(damage, transform.root.position);

        if (airController != null)
            airController.ApplyHitStun(hitStunDuration, knockbackForce, transform.root.position);
        else if (earthController != null)
            earthController.ApplyHitStun(hitStunDuration, knockbackForce, transform.root.position);
        else if (fireController != null)
            fireController.ApplyHitStun(hitStunDuration, knockbackForce, transform.root.position);

        // Disable after landing hit (prevents multi-hit on same swing)
        hasHit = true;
        DisableHitbox();
        onSuccessfulHit?.Invoke();
    }
}
