using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ScriptUI : MonoBehaviour
{
    [Header("Configurazione")]
    [SerializeField] private DPIdata config;


    [Header("Canvas da istanziare")]
    [SerializeField] private GameObject canvasPrefab;   // prefab del pannello spaziale
    private GameObject cassa;       // dove istanziarlo (opzionale, altrimenti usa transform)


    [Header("Nomi dei figli nel prefab del canvas")]
    [SerializeField] private string nomeTitolo = "Titolo";
    [SerializeField] private string nomeDescrizione = "Descrizione";
    [SerializeField] private string nomeBottone = "BottoneIndossa";


    [Header("Audio DPI")]
    [SerializeField] private AudioSource audioSource;   // assegna in Inspector, oppure viene creato automaticamente
    [SerializeField] private float volumeAudioDpi = 1f;


    [Header("Floating DPI")]
    [SerializeField] private float ampiezzaFloating = 0.05f;
    [SerializeField] private float frequenzaFloating = 1f;


    public static event Action<DPIdata> OnDpiEquipped;


    private GameObject pannelloUiSpaziale;
    private TextMeshProUGUI testoTitolo;
    private TextMeshProUGUI testoDescrizione;
    private Button bottoneIndossa;


    private Coroutine floatingCoroutine;
    private Vector3 posizioneIniziale;
    private bool floatingAttivo = false;


    void Awake()
    {
        // Crea un AudioSource dedicato per questo oggetto se non ne è stato assegnato uno
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }


    void Start()
    {
        cassa = GameObject.Find("Cassa");
    }


    public void OnPinchStart()
    {
        PlayAudioDpi();
        AvviaFloatingDpi();

        // Istanzia il canvas solo la prima volta
        if (pannelloUiSpaziale == null)
        {
            Vector3 pos = cassa != null ? cassa.transform.position + new Vector3(0, 3, 0) : transform.position;
            Quaternion rot = cassa != null ? cassa.transform.rotation : transform.rotation;


            pannelloUiSpaziale = Instantiate(canvasPrefab, pos, rot);
            if (!pannelloUiSpaziale)
                Debug.LogWarning("[scriptUI] impossibile spawnare pannello");


            Transform tTitolo = pannelloUiSpaziale.transform.Find(nomeTitolo);
            Transform tDescrizione = pannelloUiSpaziale.transform.Find(nomeDescrizione);
            Transform tBottone = pannelloUiSpaziale.transform.Find(nomeBottone);


            if (tTitolo == null || tDescrizione == null || tBottone == null)
            {
                Debug.LogError($"[ScriptUI] Impossibile trovare uno dei figli nel canvas: " +
                    $"Titolo={tTitolo != null}, Descrizione={tDescrizione != null}, Bottone={tBottone != null}. " +
                    $"Controlla i nomi nel prefab '{canvasPrefab.name}'.");
                return;
            }


            testoTitolo = tTitolo.GetComponent<TextMeshProUGUI>();
            testoDescrizione = tDescrizione.GetComponent<TextMeshProUGUI>();
            bottoneIndossa = tBottone.GetComponent<Button>();


            bottoneIndossa.onClick.AddListener(IndossaOggetto);
        }


        testoTitolo.text = config.isFondamentale
            ? config.nomeDpi + " (obbligatorio)"
            : config.nomeDpi;


        testoDescrizione.text = config.descrizioneSpiegazione;


        pannelloUiSpaziale.SetActive(true);
    }


    public void IndossaOggetto()
    {
        StopAudioDpi();
        OnDpiEquipped?.Invoke(config);
        Destroy(pannelloUiSpaziale);
        Debug.LogWarning("[scriptUI] pannello distrutto");
        gameObject.SetActive(false);
    }


    public void PlayAudioDpi()
    {
        if (config == null)
        {
            Debug.LogWarning("[ScriptUI] Config DPIdata non assegnata.");
            return;
        }

        if (config.audioSpiegazione == null)
        {
            Debug.LogWarning("[ScriptUI] Nessun audioSpiegazione assegnato nel DPIdata.");
            return;
        }

        audioSource.clip = config.audioSpiegazione;
        audioSource.volume = volumeAudioDpi;
        audioSource.Play();
    }


    public void StopAudioDpi()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }


    public void AvviaFloatingDpi()
    {
        if (floatingAttivo) return;

        posizioneIniziale = transform.position;
        floatingCoroutine = StartCoroutine(FloatingLoop());
    }


    public void FermaFloatingDpi()
    {
        if (floatingCoroutine != null)
        {
            StopCoroutine(floatingCoroutine);
            floatingCoroutine = null;
        }

        floatingAttivo = false;
        transform.position = posizioneIniziale;
    }


    private System.Collections.IEnumerator FloatingLoop()
    {
        floatingAttivo = true;

        while (floatingAttivo)
        {
            Vector3 pos = posizioneIniziale;
            pos.y = 1;
            pos.y += Mathf.Sin(Time.time * frequenzaFloating) * ampiezzaFloating;
            transform.position = pos;
            yield return null;
        }
    }
}