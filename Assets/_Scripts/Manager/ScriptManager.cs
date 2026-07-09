using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// Gestisce il dizionario dei DPI (Dispositivi di Protezione Individuale)
/// e l'aggiornamento della UI 2D che mostra i loro nomi.
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

            // Entrambi false di default: indossato e fondamentale li gestiremo in seguito
            dpiDictionary.Add(chiave, new List<bool> { false, false });
        }

        UpdateUI();
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
            string nomeDpi = coppia.Key.Item2;
            sb.AppendLine(nomeDpi);
        }

        textBox.text = sb.ToString();
    }
}