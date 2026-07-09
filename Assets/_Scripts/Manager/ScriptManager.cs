using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// Gestisce il dizionario dei DPI (Dispositivi di Protezione Individuale)
/// e l'aggiornamento della UI 2D che mostra i loro nomi.
///
/// Si integra con lo script del collaboratore "ScriptUI":
/// - "OnDpiEquipped" è un evento STATICO di tipo Action<DPIdata>: ci iscriviamo
///   una sola volta (OnEnable/OnDisable), non per ogni singolo DPI.
/// - "DPIdata" è lo ScriptableObject passato dall'evento: da lì leggiamo sia
///   "nomeDpi" (per trovare la chiave nel dizionario) sia "isFondamentale".
/// - "indossato" e "fondamentale" vengono impostati insieme, nel momento in cui
///   arriva l'evento OnDpiEquipped: prima di allora entrambi partono da false.
/// </summary>
public class ScriptManager : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private TMP_Text textBox;

    // Chiave: (GameObject del dpi, nome del dpi)
    // Valore: lista di bool -> [0] indossato, [1] fondamentale
    private Dictionary<(GameObject, string), List<bool>> dpiDictionary =
        new Dictionary<(GameObject, string), List<bool>>();

    // Indici usati per leggere/scrivere la lista di bool in modo leggibile
    private const int INDICE_INDOSSATO = 0;
    private const int INDICE_FONDAMENTALE = 1;

    private int punteggio;

    // L'evento OnDpiEquipped è statico: iscrizione unica per tutta la classe,
    // gestita in OnEnable/OnDisable (non serve iscriversi per ogni singolo DPI).
    private void OnEnable()
    {
        ScriptUI.OnDpiEquipped += GestisciIndossato;
    }

    private void OnDisable()
    {
        ScriptUI.OnDpiEquipped -= GestisciIndossato;
    }

    /// <summary>
    /// Popola il dizionario a partire dalla lista di GameObject (i DPI usciti dalla cassa).
    /// Pensato per essere chiamato dallo script del collaboratore che gestisce la cassa,
    /// passando la lista dei DPI generati/disponibili in quel momento.
    /// </summary>
    public void ListToDizionario(List<GameObject> dpiList)
    {
        if (dpiList == null)
        {
            Debug.LogWarning("[ScriptManager] La lista passata a ListToDizionario è null.");
            return;
        }

        dpiDictionary.Clear();

        foreach (GameObject dpi in dpiList)
        {
            if (dpi == null) continue;

            var chiave = (dpi, dpi.name);

            if (dpiDictionary.ContainsKey(chiave))
            {
                Debug.LogWarning($"[ScriptManager] Chiave duplicata per '{dpi.name}', elemento saltato.");
                continue;
            }

            // Sia "indossato" che "fondamentale" partono da false: verranno impostati
            // insieme quando arriverà l'evento OnDpiEquipped per questo DPI.
            dpiDictionary.Add(chiave, new List<bool> { false, false });
        }

        UpdateUI();
    }

    /// <summary>
    /// Chiamato quando un DPI viene indossato (evento statico OnDpiEquipped di ScriptUI,
    /// lanciato da IndossaOggetto). Riceve il DPIdata del DPI indossato: da qui leggiamo
    /// sia il nome (per trovare la chiave nel dizionario) sia "isFondamentale", e
    /// impostiamo entrambi i bool ("indossato" e "fondamentale") in un colpo solo.
    /// </summary>
    private void GestisciIndossato(DPIdata config)
    {
        if (config == null)
        {
            Debug.LogWarning("[ScriptManager] Evento OnDpiEquipped ricevuto con dati null.");
            return;
        }

        string nomeDpi = config.nomeDpi;

        bool trovato = false;
        foreach (var chiave in new List<(GameObject, string)>(dpiDictionary.Keys))
        {
            if (chiave.Item2 == nomeDpi)
            {
                dpiDictionary[chiave][INDICE_INDOSSATO] = true;
                dpiDictionary[chiave][INDICE_FONDAMENTALE] = config.isFondamentale;
                trovato = true;
                break;
            }
        }

        if (!trovato)
        {
            Debug.LogWarning($"[ScriptManager] Nessun DPI trovato nel dizionario con nome '{nomeDpi}'.");
        }

        UpdateUI();
    }

    /// <summary>
    /// Aggiorna la casella di testo del Canvas stampando, per ogni DPI del dizionario,
    /// una riga con un quadrato/checkbox iniziale, un tab e la frase, in base allo stato:
    /// - non indossato: '☐ [tab] Indossa "nomeDpi"' (testo normale)
    /// - indossato: '☑ [tab] Indossato "nomeDpi"' barrato, verde se fondamentale, arancione se no
    /// </summary>
    public void UpdateUI()
    {
        if (textBox == null)
        {
            Debug.LogWarning("[ScriptManager] Nessun TMP_Text assegnato in Inspector.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Punteggio: {punteggio}");
        foreach (var coppia in dpiDictionary)
        {
            string nomeDpi = coppia.Key.Item2;
            bool indossato = coppia.Value[INDICE_INDOSSATO];
            bool fondamentale = coppia.Value[INDICE_FONDAMENTALE];

            
            if (!indossato)
            {
                sb.AppendLine($"☐\tIndossa \"{nomeDpi}\"");
            }
            else
            {
                string colore = fondamentale ? "green" : "orange";
                punteggio += fondamentale ? 30 : 20;
                sb.AppendLine($"<color={colore}><s>☑\tIndossato \"{nomeDpi}\"</s></color>");
            }
        }

        textBox.text = sb.ToString();
    }
}