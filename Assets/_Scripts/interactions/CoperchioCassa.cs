using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
public class CoperchioCassa : MonoBehaviour
{
    [Header("Riferimenti")]
    public Transform puntoAtterraggio;
    public float durataSparizione = 0.3f;
    public float durataAtterraggio = 0.5f;

    private bool giaAperto = false;
    private Renderer[] renderers;
    private XRSimpleInteractable interactable;
    private Collider colliderCoperchio;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliderCoperchio = GetComponent<Collider>();
        interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnPinchSelezionato);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnPinchSelezionato);
    }

    private void OnPinchSelezionato(SelectEnterEventArgs args)
    {
        if (giaAperto) return;
        giaAperto = true;
        StartCoroutine(ApriCassa());
    }

    private IEnumerator ApriCassa()
    {
        colliderCoperchio.enabled = false; // evita altri select durante l'animazione
        SetVisibile(false);

        yield return new WaitForSeconds(durataSparizione);

        transform.position = puntoAtterraggio.position;
        transform.rotation = puntoAtterraggio.rotation;

        SetVisibile(true);

        yield return StartCoroutine(EffettoAtterraggio());
    }

    private IEnumerator EffettoAtterraggio()
    {
        Vector3 partenza = transform.position + Vector3.up * 0.3f;
        Vector3 arrivo = transform.position;
        transform.position = partenza;

        float t = 0f;
        while (t < durataAtterraggio)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(partenza, arrivo, t / durataAtterraggio);
            yield return null;
        }
        transform.position = arrivo;
    }

    private void SetVisibile(bool visibile)
    {
        foreach (var r in renderers)
            r.enabled = visibile;
    }
}