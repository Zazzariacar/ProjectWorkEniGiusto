using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// Gestisce il dizionario dei DPI (Dispositivi di Protezione Individuale)
/// e l'aggiornamento della UI 2D che mostra i loro nomi.
///
/// ATTENZIONE - da verificare/allineare con il codice del collaboratore:
/// - Si assume che il componente si chiami "ScriptUI" con:
///     - un campo pubblico "bool fondamentale"
///     - un evento pubblico "event Action<DPIScriptableObject> OnIndossa"
/// - Si assume che lo ScriptableObject si chiami "DPIScriptableObject" e abbia
///   un campo/proprietà pubblica "string nomeOggetto" col nome del DPI.
/// Se i nomi reali sono diversi, basta rinominare qui (compilatore darà errore
/// puntuale su dove correggere).
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

    // Teniamo traccia degli ScriptUI a cui ci siamo iscritti, per poterci
    // disiscrivere correttamente (evita doppie iscrizioni o memory leak)
    private List<ScriptUI> scriptUIIscritti = new List<ScriptUI>();

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

        // Se il metodo viene richiamato più volte, ci disiscriviamo prima dai vecchi eventi
        DisiscrivitiDaTutti();
        dpiDictionary.Clear();

        foreach (GameObject dpi in dpiList)
        {
            if (dpi == null) continue;

            ScriptUI scriptUI = dpi.GetComponent<ScriptUI>();
            if (scriptUI == null)
            {
                Debug.LogWarning($"[ScriptManager] Nessuno ScriptUI trovato su '{dpi.name}', elemento saltato.");
                continue;
            }

            var chiave = (dpi, dpi.name);

            if (dpiDictionary.ContainsKey(chiave))
            {
                Debug.LogWarning($"[ScriptManager] Chiave duplicata per '{dpi.name}', elemento saltato.");
                continue;
            }

            // "fondamentale" è già corretto al momento del posizionamento in scena
            // e non cambierà più: lo leggiamo una volta sola qui.
            // "indossato" parte sempre da false, verrà impostato a true dall'evento del collaboratore.
            bool fondamentale = scriptUI.fondamentale;
            dpiDictionary.Add(chiave, new List<bool> { false, fondamentale });

            // Ci iscriviamo all'evento "indossa" di QUESTO specifico DPI
            scriptUI.OnDpiEquipped += GestisciIndossato;
            scriptUIIscritti.Add(scriptUI);
        }

        UpdateUI();
    }

    /// <summary>
    /// Chiamato quando un DPI viene indossato (evento lanciato dallo ScriptUI del DPI).
    /// Riceve lo ScriptableObject del collaboratore, ne legge il nome e aggiorna
    /// il bool "indossato" del DPI corrispondente nel dizionario.
    /// </summary>
    private void GestisciIndossato(DPIScriptableObject datiDpi)
    {
        if (datiDpi == null)
        {
            Debug.LogWarning("[ScriptManager] Evento OnIndossa ricevuto con dati null.");
            return;
        }

        // NB: adattare "nomeOggetto" al nome effettivo del campo/proprietà
        // presente nello ScriptableObject del collaboratore.
        string stringNomeDpi = datiDpi.nomeDpi;

        bool trovato = false;
        foreach (var chiave in new List<(GameObject, string)>(dpiDictionary.Keys))
        {
            if (chiave.Item2 == stringNomeDpi)
            {
                dpiDictionary[chiave][INDICE_INDOSSATO] = true;
                trovato = true;
                break;
            }
        }

        if (!trovato)
        {
            Debug.LogWarning($"[ScriptManager] Nessun DPI trovato nel dizionario con nome '{stringNomeDpi}'.");
        }

        UpdateUI();
    }

    /// <summary>
    /// Rimuove le iscrizioni agli eventi OnIndossa di tutti gli ScriptUI tracciati.
    /// </summary>
    private void DisiscrivitiDaTutti()
    {
        foreach (ScriptUI scriptUI in scriptUIIscritti)
        {
            if (scriptUI != null)
            {
                scriptUI.OnIndossa -= GestisciIndossato;
            }
        }
        scriptUIIscritti.Clear();
    }

    private void OnDestroy()
    {
        DisiscrivitiDaTutti();
    }

    /// <summary>
    /// Aggiorna la casella di testo del Canvas stampando i nomi dei DPI
    /// presenti nel dizionario, uno per riga.
    /// </summary>
    public void UpdateUI()
    {
        if (textBox == null)
        {
            Debug.LogWarning("[ScriptManager] Nessun TMP_Text assegnato in Inspector.");
            return;
        }

        StringBuilder sb = new StringBuilder();

        foreach (var coppia in dpiDictionary)
        {
            string stringNomeDpi = coppia.Key.Item2;
            sb.AppendLine(stringNomeDpi);
        }

        textBox.text = sb.ToString();
    }
}