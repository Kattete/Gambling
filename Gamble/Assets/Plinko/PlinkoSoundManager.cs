using UnityEngine;

public class PlinkoSoundManager : MonoBehaviour
{
    public static PlinkoSoundManager Instance { get; private set; }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] pegHitSound;
    [SerializeField] private AudioClip[] collectionSound;

    [Header("Audio Settings")]
    [SerializeField, Range(0.0f, 1.0f)] private float pegHitVolume = 0.3f;
    [SerializeField, Range(0.0f, 1.0f)] private float collectionVolume = 0.5f;
    [SerializeField] private float minPitchVariation = 0.95f;
    [SerializeField] private float maxPitchVariation = 1.05f;

    // Pool of audio sources for multiple simultaneous sounds
    private AudioSource[] audioSources;
    private int currentAudioSourceIndex = 0;
    private const int AUDIO_SOURCE_POOL_SIZE = 5;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        // Create a pool of audio sources for handling multiple collisions
        audioSources = new AudioSource[AUDIO_SOURCE_POOL_SIZE];
        for (int i = 0; i < AUDIO_SOURCE_POOL_SIZE; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].spatialBlend = 0f; // Set to 2D audio
        }
    }

    // Play a random peg hit sound with slight pitch variation
    public void PlayPegHit()
    {
        if (pegHitSound == null || pegHitSound.Length == 0) return;

        AudioSource source = GetNextAudioSource();
        source.clip = pegHitSound[Random.Range(0, pegHitSound.Length)];
        source.volume = pegHitVolume;
        source.pitch = Random.Range(minPitchVariation, maxPitchVariation);
        source.Play();
    }

    // Play collection sound based on multiplier value
    public void PlayCollection(float multiplier)
    {
        if (collectionSound == null || collectionSound.Length == 0) return;

        // Select sound based on multiplier (higher multiplier = more exciting sound)
        int soundIndex = Mathf.Clamp(
            Mathf.FloorToInt(multiplier) - 1,
            0,
            collectionSound.Length - 1
        );

        AudioSource source = GetNextAudioSource();
        source.clip = collectionSound[soundIndex];
        source.volume = collectionVolume;
        source.pitch = 1f; // No pitch variation for collection sounds
        source.Play();
    }

    private AudioSource GetNextAudioSource()
    {
        // Cycle through audio sources
        AudioSource source = audioSources[currentAudioSourceIndex];
        currentAudioSourceIndex = (currentAudioSourceIndex + 1) % AUDIO_SOURCE_POOL_SIZE;
        return source;
    }
}
