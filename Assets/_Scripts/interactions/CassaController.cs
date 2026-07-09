using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Questo script va sull'oggetto PADRE "Cassa" (quello con lo XRGrabInteractable)
[RequireComponent(typeof(XRGrabInteractable))]
public class CassaController : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        grabInteractable.selectExited.AddListener(OnRilasciato);
    }

    void OnDisable()
    {
        grabInteractable.selectExited.RemoveListener(OnRilasciato);
    }

    private void OnRilasciato(SelectExitEventArgs args)
    {
        // Se l'interactor che ha "rilasciato" (in realtà agganciato) è uno Socket,
        // significa che la cassa è stata posizionata correttamente
        if (args.interactorObject is XRSocketInteractor)
        {
            // Disattiva il grab del padre: da qui in poi la cassa non si muove più
            grabInteractable.enabled = false;
        }
    }
}