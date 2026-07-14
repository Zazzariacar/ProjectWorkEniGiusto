using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using XRGrab = UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable;
using XRSocket = UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor;
using SelectArgs = UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs;

public class ScriptUI : MonoBehaviour
{
    [Header("Configurazione")] [SerializeField] private DPIdata config;
    [Header("Canvas da istanziare")] [SerializeField] private GameObject canvasPrefab;

    [Header("Nomi dei figli nel prefab del canvas")]
    [SerializeField] private string nomeTitolo = "Titolo";
    [SerializeField] private string nomeDescrizione = "Descrizione";
    [SerializeField] private string nomeBottone = "BottoneIndossa";

    [Header("Audio DPI")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float volumeAudioDpi = 1f;

    [Header("Floating DPI")]
    [SerializeField] private float ampiezzaFloating = 0.05f;
    [SerializeField] private float frequenzaFloating = 1f;

    public static event Action<DPIdata> OnDpiEquipped;

    private GameObject cassa, pannelloUiSpaziale;
    private TextMeshProUGUI testoTitolo, testoDescrizione;
    private Button bottoneIndossa;
    private Coroutine floatingCoroutine;
    private Vector3 posizioneIniziale;
    private bool floatingAttivo, giaIndossato, indossaCliccato;
    private XRGrab grabInteractable;
    private XRSocket targetSocket;

    void Awake()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        grabInteractable = GetComponent<XRGrab>();
    }

    void Start()
    {
        cassa = GameObject.Find("Cassa");
        posizioneIniziale = transform.position;
        AvviaFloatingDpi();

        // Troviamo il socket e lo disattiviamo finché non si clicca "Indossa"
        if (TrovaSocket(out targetSocket)) targetSocket.enabled = false;
    }

    void OnEnable() { if (grabInteractable != null) grabInteractable.selectEntered.AddListener(OnSelectEntered); }
    void OnDisable() { if (grabInteractable != null) grabInteractable.selectEntered.RemoveListener(OnSelectEntered); }

    // Cerca (se serve) e restituisce il socket associato a questo DPI
    private bool TrovaSocket(out XRSocket socket)
    {
        socket = targetSocket;
        if (socket != null) return true;
        if (config == null || !config.usaSocket || string.IsNullOrEmpty(config.tagSocketDestinazione)) return false;

        GameObject socketObj = GameObject.FindGameObjectWithTag(config.tagSocketDestinazione);
        socket = socketObj != null ? socketObj.GetComponent<XRSocket>() : null;
        return socket != null;
    }

    public void OnPinchStart()
    {
        // Se già indossato o già cliccato "Indossa" (in trascinamento verso il socket), niente pannello
        if (giaIndossato || indossaCliccato) return;

        PlayAudioDpi();

        if (pannelloUiSpaziale == null)
        {
            Vector3 pos = cassa != null ? cassa.transform.position + new Vector3(0, 3, 0) : transform.position + Vector3.up;
            Quaternion rot = cassa != null ? cassa.transform.rotation : transform.rotation;
            pannelloUiSpaziale = Instantiate(canvasPrefab, pos, rot);

            testoTitolo = pannelloUiSpaziale.transform.Find(nomeTitolo)?.GetComponent<TextMeshProUGUI>();
            testoDescrizione = pannelloUiSpaziale.transform.Find(nomeDescrizione)?.GetComponent<TextMeshProUGUI>();
            bottoneIndossa = pannelloUiSpaziale.transform.Find(nomeBottone)?.GetComponent<Button>();

            if (bottoneIndossa != null)
            {
                bottoneIndossa.onClick.RemoveAllListeners();
                bottoneIndossa.onClick.AddListener(IndossaOggetto);
            }
        }

        if (testoTitolo != null) testoTitolo.text = config.isFondamentale ? config.nomeDpi + " (obbligatorio)" : config.nomeDpi;
        if (testoDescrizione != null) testoDescrizione.text = config.descrizioneSpiegazione;

        pannelloUiSpaziale.SetActive(true);
    }

    public void IndossaOggetto()
    {
        if (indossaCliccato) return;
        indossaCliccato = true;

        StopAudioDpi();
        StopFloatingDpi();
        if (pannelloUiSpaziale != null) Destroy(pannelloUiSpaziale);

        if (config.usaSocket && !string.IsNullOrEmpty(config.tagSocketDestinazione))
        {
            if (TrovaSocket(out targetSocket)) targetSocket.enabled = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
        }
        else
        {
            giaIndossato = true;
            OnDpiEquipped?.Invoke(config);
            AudioManager.Instance.RiproduciVestizione();
            Destroy(gameObject);
        }
    }

    private void OnSelectEntered(SelectArgs args)
    {
        if (giaIndossato) return;

        // Nasconde il pannello solo se già cliccato "Indossa" (trascinamento verso il socket in corso)
        if (indossaCliccato && pannelloUiSpaziale != null && pannelloUiSpaziale.activeSelf) pannelloUiSpaziale.SetActive(false);

        if (config != null && args.interactorObject != null && args.interactorObject.transform.CompareTag(config.tagSocketDestinazione))
        {
            giaIndossato = true;
            OnObjectPlacedInSocket(args.interactorObject.transform);
        }
    }

    private void OnObjectPlacedInSocket(Transform socketTransform)
    {
        AudioManager.Instance.RiproduciVestizione();
        OnDpiEquipped?.Invoke(config);
        StartCoroutine(LockInSocket(socketTransform));
    }

    private IEnumerator LockInSocket(Transform socketTransform)
    {
        yield return new WaitForEndOfFrame();

        if (grabInteractable != null) grabInteractable.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        transform.SetParent(socketTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        enabled = false;
    }

    public void PlayAudioDpi()
    {
        if (config?.audioSpiegazione == null) return;
        audioSource.clip = config.audioSpiegazione;
        audioSource.volume = volumeAudioDpi;
        audioSource.Play();
    }

    public void StopAudioDpi() { if (audioSource != null && audioSource.isPlaying) audioSource.Stop(); }

    public void AvviaFloatingDpi()
    {
        if (floatingAttivo) return;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        posizioneIniziale = transform.position;
        floatingCoroutine = StartCoroutine(FloatingLoop());
    }

    public void StopFloatingDpi()
    {
        floatingAttivo = false;
        if (floatingCoroutine != null) { StopCoroutine(floatingCoroutine); floatingCoroutine = null; }
    }

    private IEnumerator FloatingLoop()
    {
        floatingAttivo = true;
        while (floatingAttivo)
        {
            float yOffset = Mathf.Sin(Time.time * frequenzaFloating) * ampiezzaFloating;
            transform.position = new Vector3(posizioneIniziale.x, posizioneIniziale.y + yOffset, posizioneIniziale.z);
            yield return null;
        }
    }
}