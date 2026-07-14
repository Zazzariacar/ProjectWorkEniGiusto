 using System.Collections;
using System.Collections.Generic;
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


    [Header("Apertura coperchio (tip)")]
    public Vector3 assoRotazione = Vector3.right; // asse locale su cui ruota il coperchio
    public float angoloApertura = 100f;            // gradi di apertura
    public float durataApertura = 0.6f;

    [Header("Spawn oggetti")]
    public GameObject[] prefabDaSpawnare;
    public Vector3 posizioneBase = new Vector3(107.19f, 0.49f, 48.134f);
    public float zMinimo = 45f;
    public float ritardoTraSpawn = 0.15f;

    [Header("Collegamento ScriptManager")]
    public ScriptManager scriptManager;

    private bool giaAperto = false;
    private Renderer[] renderers;
    private XRSimpleInteractable interactable;
    private Collider colliderCoperchio;
    private Quaternion rotazioneIniziale;
    public AudioManager audioManager;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliderCoperchio = GetComponent<Collider>();
        interactable = GetComponent<XRSimpleInteractable>();
        rotazioneIniziale = transform.localRotation;

        // Apertura cassa:
        audioManager.RiproduciAperturaCassa();
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
         Debug.Log("Il coperchio è stato toccato/pinchato!"); // <--- AGGIUNGI QUESTO
        if (giaAperto) return;
        giaAperto = true;
        StartCoroutine(ApriCassa());
    }

    private IEnumerator ApriCassa()
    {
        colliderCoperchio.enabled = false; // evita altri select durante l'animazione

        // 1) Il coperchio si "ribalta" (tip) verso l'alto
        yield return StartCoroutine(RuotaCoperchio());

        // 2) Sparisce
        SetVisibile(false);
        yield return new WaitForSeconds(durataSparizione);

        // 3) Si riposiziona nel punto di atterraggio (es. accanto alla cassa, a terra)
        transform.position = puntoAtterraggio.position;
        transform.rotation = puntoAtterraggio.rotation;

        SetVisibile(true);

        yield return StartCoroutine(EffettoAtterraggio());

        // 4) Una volta atterrato il coperchio, tirano fuori il contenuto
        yield return StartCoroutine(SpawnaContenuto());
    }

    private IEnumerator RuotaCoperchio()
    {
        Quaternion partenza = transform.localRotation;
        Quaternion arrivo = rotazioneIniziale * Quaternion.AngleAxis(angoloApertura, assoRotazione);

        float t = 0f;
        while (t < durataApertura)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(partenza, arrivo, t / durataApertura);
            yield return null;
        }
        transform.localRotation = arrivo;
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

    private IEnumerator SpawnaContenuto()
    {
        // Quante posizioni entrano in una "riga" prima di dover tornare a Z iniziale
        // Es: base.z = 48.134, zMinimo = 45 -> passi possibili: 48.134, 47.134, 46.134, 45.134 (4 posizioni)
        int posizioniPerRiga = Mathf.FloorToInt(posizioneBase.z - zMinimo) + 1;
        if (posizioniPerRiga < 1) posizioniPerRiga = 1;

        List<GameObject> oggettiSpawnati = new List<GameObject>();

        for (int i = 0; i < prefabDaSpawnare.Length; i++)
        {
            int colonna = i / posizioniPerRiga;   // quante volte abbiamo "sforato" lo zMinimo -> +1 su X
            int riga = i % posizioniPerRiga;       // posizione dentro la riga corrente -> -1 su Z

            Vector3 pos = new Vector3(
                posizioneBase.x + colonna,
                posizioneBase.y,
                posizioneBase.z - riga
            );

            if (prefabDaSpawnare[i] != null)
            {
                GameObject istanza = Instantiate(prefabDaSpawnare[i], pos, Quaternion.identity);
                oggettiSpawnati.Add(istanza);
            }

            yield return new WaitForSeconds(ritardoTraSpawn);
        }

        // Passa la lista completa degli oggetti spawnati a ScriptManager
        if (scriptManager != null)
        {
            scriptManager.ListToDizionario(oggettiSpawnati);
            scriptManager.UpdateUI();
        }
        else
        {
            Debug.LogWarning("ScriptManager non assegnato su CoperchioCassa: impossibile chiamare ListToDizionario.");
        }
    }

    private void SetVisibile(bool visibile)
    {
        foreach (var r in renderers)
            r.enabled = visibile;
    }
}