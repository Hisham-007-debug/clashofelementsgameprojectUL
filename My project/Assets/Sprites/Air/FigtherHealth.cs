using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FighterHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Image healthFill;       // main coloured bar
    public Image ghostFill;        // delayed white bar behind main
    public Text nameLabel;         // optional player name

    [Header("Ghost Bar")]
    public float ghostDelay  = 0.6f;   // seconds before ghost starts draining
    public float ghostSpeed  = 0.25f;  // fraction per second
    
    [Header("Blood Effects")]
    public BloodParticleManager bloodManager;
    public bool IsDead { get; private set; }

    private float ghostHealth;
    private float timeSinceHit;

    // Gradient: green (100%) → yellow (50%) → red (0%)
    private static readonly Gradient healthGradient = new Gradient
    {
        colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.15f, 0.85f, 0.25f), 1.00f),
            new GradientColorKey(new Color(0.95f, 0.85f, 0.10f), 0.50f),
            new GradientColorKey(new Color(0.90f, 0.15f, 0.10f), 0.00f),
        },
        alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f),
        }
    };

    void Start()
    {
        currentHealth = maxHealth;
        ghostHealth   = maxHealth;
        timeSinceHit  = ghostDelay;

        if (healthFill != null)
        {
            healthFill.type       = UnityEngine.UI.Image.Type.Filled;
            healthFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        }
        if (ghostFill != null)
        {
            ghostFill.type       = UnityEngine.UI.Image.Type.Filled;
            ghostFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        }

        Refresh();
    }

    void Update()
    {
        timeSinceHit += Time.deltaTime;

        if (timeSinceHit >= ghostDelay && ghostHealth > currentHealth)
        {
            ghostHealth = Mathf.Max(currentHealth,
                ghostHealth - ghostSpeed * maxHealth * Time.deltaTime);
            UpdateGhost();
        }
    }

    public void TakeDamage(float amount, Vector3 attackerPosition)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        timeSinceHit = 0f;

        // --- BLOOD EFFECT ---
        if (bloodManager != null)
        {
            Vector2 hitDir = (transform.position - attackerPosition).normalized;
            bloodManager.SpawnBlood(currentHealth, maxHealth, hitDir, amount);
        }

        Refresh();

        if (currentHealth <= 0)
            OnKO();
    }

    // ORIGINAL: Keeps your old code working (sprays blood upward)
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        timeSinceHit = 0f;

        // --- BLOOD EFFECT ---
        if (bloodManager != null)
        {
            bloodManager.SpawnBlood(currentHealth, maxHealth, amount);
        }

        Refresh();

        if (currentHealth <= 0)
            OnKO();
    }

    void Refresh()
    {
        float pct = currentHealth / maxHealth;

        if (healthFill != null)
        {
            healthFill.fillAmount = pct;
            healthFill.color      = healthGradient.Evaluate(pct);
            Debug.Log($"[Health] pct={pct}, fillAmount={healthFill.fillAmount}, type={healthFill.type}");
        }
    }

    void UpdateGhost()
    {
        if (ghostFill != null)
            ghostFill.fillAmount = ghostHealth / maxHealth;
    }

    void OnKO()
    {
        IsDead = true;
        GetComponent<Animator>().SetTrigger("KO");

        // --- DEATH BLOOD BURST ---
        if (bloodManager != null)
        {
            bloodManager.SpawnBlood(0, maxHealth, Vector2.up, maxHealth);
            bloodManager.SpawnBlood(0, maxHealth, Vector2.left, maxHealth);
            bloodManager.SpawnBlood(0, maxHealth, Vector2.right, maxHealth);
        }

        var air = GetComponent<PlayerMovementAir>();
        var earth = GetComponent<PlayerMovementEarth>();
        if (air != null) air.enabled = false;
        if (earth != null) earth.enabled = false;
    }
}
