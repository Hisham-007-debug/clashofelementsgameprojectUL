using UnityEngine;
using UnityEngine.UI;

public class FighterHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Slider healthBar;

    public bool IsDead { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthBar();

        if (currentHealth <= 0)
            OnKO();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = currentHealth / maxHealth;
    }

    void OnKO()
    {
        IsDead = true;
        GetComponent<Animator>().SetTrigger("KO");
        GetComponent<PlayerMovementAir>().enabled = false;
    }
}