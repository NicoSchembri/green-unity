using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip backgroundTrack;
    [Range(0f, 1f)] public float volume = 0.5f;
    public bool loop = true;
    public bool persistBetweenScenes = true;

    [Tooltip("Start playback at this time (in seconds).")]
    public float startTime = 0f; 

    private AudioSource audioSource;
    private static BackgroundMusic instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (persistBetweenScenes)
            DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.volume = volume;

        if (backgroundTrack != null)
        {
            audioSource.clip = backgroundTrack;

            startTime = Mathf.Clamp(startTime, 0f, backgroundTrack.length - 0.1f);

            audioSource.time = startTime;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"{name}: No background music track assigned!");
        }
    }
}
