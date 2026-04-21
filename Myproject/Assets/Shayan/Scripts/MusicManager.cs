using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip mainTheme;

    private AudioSource audioSource;

    void Awake()
    {
        // Singleton: destroy any duplicate that loads in a later scene
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip        = mainTheme;
        audioSource.loop        = true;
        audioSource.playOnAwake = false;

        PlayTheme();
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "FightScene")
            StopTheme();
        else
            PlayTheme();
    }

    private void PlayTheme()
    {
        if (mainTheme == null || audioSource.isPlaying) return;
        audioSource.clip = mainTheme;
        audioSource.Play();
    }

    private void StopTheme()
    {
        audioSource.Stop();
    }
}
