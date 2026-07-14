using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    // Il Singleton permette di accedere a questo script da chiunque scrivendo AudioManager.Instance
    public static AudioManager Instance { get; private set; }

    [Header("File Audio (AudioClips)")]
    [Tooltip("Trascina qui il suono per quando si indossa un prefab")]
    public AudioClip suonoVestizione;
    
    [Tooltip("Trascina qui il suono per l'apertura della cassa")]
    public AudioClip suonoAperturaCassa;

    private AudioSource globalAudioSource;

    void Awake()
    {
        // Gestione del Singleton per evitare duplicati nella scena
        if (Instance == null)
        {
            Instance = this;
            // Opzionale: decommenta la riga sotto se vuoi che l'audio non si interrompa cambiando scena
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        globalAudioSource = gameObject.AddComponent<AudioSource>();
        globalAudioSource.playOnAwake = false;
    }

    // Metodo per riprodurre il suono dei prefab
    public void RiproduciVestizione()
    {
        if (suonoVestizione != null && globalAudioSource != null)
        {
            globalAudioSource.PlayOneShot(suonoVestizione);
        }
    }

    // Metodo per riprodurre il suono del coperchio
    public void RiproduciAperturaCassa()
    {
        if (suonoAperturaCassa != null && globalAudioSource != null)
        {
            globalAudioSource.PlayOneShot(suonoAperturaCassa);
        }
    }
}