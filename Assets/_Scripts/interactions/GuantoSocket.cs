using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class GuantoSocket : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private bool posizionato = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (posizionato) return;

        // Si attiva solo se chi ha "afferrato" è il Socket, non la mano
        if (args.interactorObject is XRSocketInteractor)
        {
            posizionato = true;
            BloccaGuanto();
        }
    }

    private void BloccaGuanto()
    {
        rb.isKinematic = true;              // niente più fisica
        grabInteractable.enabled = false;   // non più afferrabile da nessuno
    }
}