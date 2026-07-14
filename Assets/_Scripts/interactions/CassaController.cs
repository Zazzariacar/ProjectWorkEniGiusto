using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Questo script va sull'oggetto PADRE "Cassa" (quello con lo XRGrabInteractable)
[RequireComponent(typeof(XRGrabInteractable))]
public class CassaController : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    [Header("Effetto Illuminazione")]
    [Tooltip("Trascina qui il figlio 'Illumina'")]
    public GameObject oggettoIllumina; 
    
    [Tooltip("Colore dell'emissione (Supporta HDR)")]
    [ColorUsage(true, true)] public Color coloreEmissione = Color.cyan; 
    
    public float velocitaPulsazione = 3f;
    public float intensitaMinima = 0.5f;
    public float intensitaMassima = 2.5f;

    private Material illuminaMaterial;
    private bool isPulsing = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Recuperiamo il materiale dell'oggetto Illumina all'avvio
        if (oggettoIllumina != null)
        {
            Renderer renderer = oggettoIllumina.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Usiamo .material per creare un'istanza indipendente
                // così non modifichiamo il materiale originale nel progetto
                illuminaMaterial = renderer.material; 
                isPulsing = true;
            }
        }
    }

    void Update()
    {
        // Se c'è un materiale valido e la cassa non è ancora stata presa, pulsa
        if (isPulsing && illuminaMaterial != null)
        {
            // Usiamo Mathf.Sin per creare un'onda che sale e scende morbidamente (tra 0 e 1)
            float t = (Mathf.Sin(Time.time * velocitaPulsazione) + 1f) / 2f;
            
            // Calcoliamo l'intensità attuale basandoci sul min e max che abbiamo scelto
            float intensitaAttuale = Mathf.Lerp(intensitaMinima, intensitaMassima, t);
            
            // Impostiamo il colore di emissione (URP usa la keyword "_EmissionColor")
            illuminaMaterial.SetColor("_EmissionColor", coloreEmissione * intensitaAttuale);
        }
    }

    void OnEnable()
    {
        grabInteractable.selectExited.AddListener(OnRilasciato);
        
        // Aggiungiamo l'ascoltatore per quando AFFERRIAMO la cassa
        grabInteractable.selectEntered.AddListener(OnAfferrato); 
    }

    void OnDisable()
    {
        grabInteractable.selectExited.RemoveListener(OnRilasciato);
        grabInteractable.selectEntered.RemoveListener(OnAfferrato);
    }

    private void OnAfferrato(SelectEnterEventArgs args)
    {
        // Appena l'utente prende la cassa, spegniamo il Game Object "Illumina"
        if (oggettoIllumina != null)
        {
            isPulsing = false;
            oggettoIllumina.SetActive(false);
        }
    }

   private void OnRilasciato(SelectExitEventArgs args)
{
    // Verifica se l'interactor che ha completato l'azione è un XRSocketInteractor
    if (args.interactorObject is XRSocketInteractor)
    {
        Debug.Log("[CassaController] Cassa inserita nel socket. Disabilitazione componenti...");

        // 1. Disabilita l'interazione di presa
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // 2. Disabilita i collider della cassa stessa (o dei suoi figli) 
        // per evitare che le mani ci sbattano contro o che facciano collisioni strane
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            // Opzionale: se il socket ha bisogno del collider per "tenerla", 
            // potresti voler mantenere solo il trigger, ma solitamente si disabilitano tutti.
            col.enabled = false;
        }

        // 3. (Opzionale) Se vuoi che la cassa sia "fissa" nel socket, 
        // disabilita anche il Rigidbody se presente
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 4. Abilita il coperchio ora che la cassa è ferma
        CoperchioCassa coperchio = GetComponentInChildren<CoperchioCassa>();
        if (coperchio != null)
        {
            coperchio.enabled = true;
            // Assicuriamoci che il collider del coperchio sia riattivato
            Collider colCoperchio = coperchio.GetComponent<Collider>();
            if (colCoperchio != null) colCoperchio.enabled = true;
        }
    }
 }
}