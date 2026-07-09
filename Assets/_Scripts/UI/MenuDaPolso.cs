using UnityEngine;

/// <summary>
/// Controlla la visibilità di un Canvas attaccato al polso in VR,
/// mostrandolo solo quando l'utente lo guarda (ruotando il polso verso il viso).
/// </summary>
public class MenuDaPolso : MonoBehaviour
{
    [Header("Riferimenti")]
    [Tooltip("Il transform della telecamera VR (Main Camera)")]
    [SerializeField] private Transform telecameraVR;
    
    [Tooltip("Il Canvas o l'oggetto UI da mostrare/nascondere")]
    [SerializeField] private GameObject canvasPolso;

    [Header("Impostazioni")]
    [Tooltip("Angolo massimo in gradi entro cui la UI si attiva (es. 45)")]
    [SerializeField] private float angoloAttivazione = 45f;

    private void Start()
    {
        // Se non è stata assegnata a mano, cerca la telecamera principale
        if (telecameraVR == null && Camera.main != null)
        {
            telecameraVR = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (telecameraVR == null || canvasPolso == null) return;

        // 1. Calcoliamo la direzione dal polso verso la testa (telecamera)
        Vector3 direzioneVersoTesta = telecameraVR.position - transform.position;

        // 2. Calcoliamo l'angolo tra il "Sopra" del polso e la direzione verso la testa
        // NOTA: A seconda di come è orientato il tuo controller in scena,
        // potresti dover cambiare transform.up con transform.right o transform.forward.
        float angolo = Vector3.Angle(transform.up, direzioneVersoTesta);

        // 3. Mostra o nascondi il Canvas in base all'angolo
        if (angolo < angoloAttivazione)
        {
            if (!canvasPolso.activeSelf) canvasPolso.SetActive(true);
        }
        else
        {
            if (canvasPolso.activeSelf) canvasPolso.SetActive(false);
        }
    }
}