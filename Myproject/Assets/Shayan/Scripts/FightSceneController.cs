using UnityEngine;

public class FightSceneController : MonoBehaviour
{
    [Header("Background")]
    public SpriteRenderer fightBackground;
    public Sprite fireArena;
    public Sprite iceArena;
    public Sprite airArena;
    public Sprite earthArena;

    [Header("Character Prefabs")]
    public GameObject firePrefab;   // index 0
    public GameObject icePrefab;    // index 1
    public GameObject airPrefab;    // index 2
    public GameObject earthPrefab;  // index 3

    [Header("Spawn Points")]
    public Transform p1SpawnPoint;
    public Transform p2SpawnPoint;

    private void Start()
    {
        ApplyBackground();
        SpawnCharacters();
    }

    private void ApplyBackground()
    {
        if (fightBackground == null)
        {
            Debug.LogError("FightBackground is not assigned.");
            return;
        }

        int index = PlayerPrefs.GetInt("SelectedArenaIndex", -1);
        Sprite chosen = index switch
        {
            0 => fireArena,
            1 => iceArena,
            2 => airArena,
            3 => earthArena,
            _ => null
        };

        if (chosen == null)
        {
            Debug.LogError("No valid arena index or sprite: " + index);
            return;
        }

        fightBackground.sprite = chosen;
        FitBackgroundToCamera();
    }

    private void FitBackgroundToCamera()
    {
        Camera cam = Camera.main;
        if (cam == null || fightBackground.sprite == null) return;

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;
        Vector2 spriteSize = fightBackground.sprite.bounds.size;

        fightBackground.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 1f);
        fightBackground.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
        fightBackground.sortingOrder = -100;
    }

    private void SpawnCharacters()
    {
        SpawnCharacter(CharacterSelectionData.P1Index, p1SpawnPoint, false);
        SpawnCharacter(CharacterSelectionData.P2Index, p2SpawnPoint, true);
    }

    private void SpawnCharacter(int index, Transform spawnPoint, bool flipX)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn point not assigned.");
            return;
        }

        GameObject prefab = index switch
        {
            0 => firePrefab,
            1 => icePrefab,
            2 => airPrefab,
            3 => earthPrefab,
            _ => null
        };

        if (prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for character index {index}.");
            return;
        }

        GameObject character = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        if (flipX)
        {
            Vector3 scale = character.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            character.transform.localScale = scale;
        }
    }
}
