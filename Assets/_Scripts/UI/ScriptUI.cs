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
    [SerializeField] private Transform spawnPoint;       // dove istanziarlo (opzionale, altrimenti usa transform)

    [Header("Nomi dei figli nel prefab del canvas")]
    [SerializeField] private string nomeTitolo = "Titolo";
    [SerializeField] private string nomeDescrizione = "Descrizione";
    [SerializeField] private string nomeBottone = "BottoneIndossa";

    public static event Action<DPIdata> OnDpiEquipped;

    private GameObject pannelloUiSpaziale;
    private TextMeshProUGUI testoTitolo;
    private TextMeshProUGUI testoDescrizione;
    private Button bottoneIndossa;

    public void OnPinchStart()
    {
        // Istanzia il canvas solo la prima volta
        if (pannelloUiSpaziale == null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            pannelloUiSpaziale = Instantiate(canvasPrefab, pos, rot);

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
        OnDpiEquipped?.Invoke(config);
        pannelloUiSpaziale.SetActive(false);
        gameObject.SetActive(false);
    }
}