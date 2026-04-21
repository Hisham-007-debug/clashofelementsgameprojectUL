using UnityEngine;

public class BloodParticleManager : MonoBehaviour
{
    [Header("Particle System")]
    [SerializeField] private ParticleSystem bloodParticleSystem;

    [Header("Blood Particle Settings")]
    [SerializeField] private int minParticles = 8;
    [SerializeField] private int maxParticles = 80;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 18f;
    [SerializeField] private float particleLifetime = 1.4f;
    [SerializeField] private Color bloodColor = new Color(0.6f, 0f, 0f, 1f);
    [SerializeField] private float particleSize = 0.1f;

    [Header("Splatter Decals (Optional)")]
    [SerializeField] private GameObject bloodSplatterPrefab;
    [SerializeField] private float splatterLifetime = 5f;

    private void Awake()
    {
        if (bloodParticleSystem == null)
            CreateBloodParticleSystem();
    }

    private void CreateBloodParticleSystem()
    {
        GameObject psObject = new GameObject("BloodParticles");
        psObject.transform.SetParent(transform);
        psObject.transform.localPosition = Vector3.zero;

        bloodParticleSystem = psObject.AddComponent<ParticleSystem>();

        var main = bloodParticleSystem.main;
        main.playOnAwake = false;
        main.startLifetime = particleLifetime;
        main.startSpeed = minSpeed;
        main.startSize = particleSize;
        main.startColor = bloodColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.25f;

        var emission = bloodParticleSystem.emission;
        emission.rateOverTime = 0;

        var shape = bloodParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 90f;
        shape.radius = 0.15f;

        var colorOverLifetime = bloodParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(bloodColor, 0f),
                new GradientColorKey(bloodColor, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = bloodParticleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0.3f);

        var renderer = psObject.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = 10;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = bloodColor;
    }

    /// <summary>
    /// Main method - call this from FighterHealth
    /// </summary>
    public void SpawnBlood(float currentHealth, float maxHealth, Vector2 hitDirection, float damageAmount)
    {
        if (bloodParticleSystem == null) return;

        // Lower health = more blood
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        float intensity = 1f - healthPercent;

        // Bigger hits = more blood
        float damageRatio = Mathf.Clamp01(damageAmount / maxHealth);

        // Combine both factors
        float combinedIntensity = Mathf.Clamp01(intensity + damageRatio);

        // Scale particle count
        int particleCount = Mathf.RoundToInt(
            Mathf.Lerp(minParticles, maxParticles, combinedIntensity));

        // Scale speed
        float speed = Mathf.Lerp(minSpeed, maxSpeed, combinedIntensity);

        // Point particles in hit direction
        var shape = bloodParticleSystem.shape;
        float angle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;
        shape.rotation = new Vector3(0, 0, angle);
        shape.angle = Mathf.Lerp(80f, 150f, intensity);

        // Apply speed and size
        var main = bloodParticleSystem.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(
            particleSize * 0.5f,
            particleSize * (1f + intensity));

        // Emit!
        bloodParticleSystem.Emit(particleCount);

        // Optional splatter decals
        if (bloodSplatterPrefab != null)
        {
            int splatterCount = Mathf.RoundToInt(Mathf.Lerp(0, 3, combinedIntensity));
            for (int i = 0; i < splatterCount; i++)
                SpawnSplatter(hitDirection, combinedIntensity);
        }
    }

    /// <summary>
    /// Simplified version - no direction, sprays upward
    /// </summary>
    public void SpawnBlood(float currentHealth, float maxHealth, float damageAmount)
    {
        SpawnBlood(currentHealth, maxHealth, Vector2.up, damageAmount);
    }

    private void SpawnSplatter(Vector2 hitDirection, float intensity)
    {
        Vector2 offset = (hitDirection.normalized * Random.Range(0.5f, 2f))
            + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));

        Vector3 spawnPos = transform.position + (Vector3)offset;

        GameObject splatter = Instantiate(bloodSplatterPrefab, spawnPos, Quaternion.identity);
        splatter.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        float scale = Random.Range(0.3f, 1f) * (1f + intensity);
        splatter.transform.localScale = Vector3.one * scale;

        Destroy(splatter, splatterLifetime);
    }
}