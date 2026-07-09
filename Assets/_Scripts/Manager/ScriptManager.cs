using System.Collections.Generic;
using System.Text;
using System.Linq; // Aggiunto per usare LINQ (rende il controllo snellissimo)
using UnityEngine;
using UnityEngine.UI; // Aggiunto per gestire il Button standard di Unity
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
    [SerializeField] private Button fineEsperienzaButton; // Il bottone da attivare alla fine

    // Chiave: (GameObject del dpi, nome del dpi)
    // Valore: lista di bool -> [0] indossato, [1] fondamentale
    private Dictionary<(GameObject, string), List<bool>> dpiDictionary =
        new Dictionary<(GameObject, string), List<bool>>();

    private const int INDICE_INDOSSATO = 0;
    private const int INDICE_FONDAMENTALE = 1;

    private int punteggio;

    private void OnEnable()
    {
        ScriptUI.OnDpiEquipped += GestisciIndossato;
    }

    private void OnDisable()
    {
        ScriptUI.OnDpiEquipped -= GestisciIndossato;
    }

    private void Start()
    {
        // Ci assicuriamo che il bottone parta disattivato e gli assegniamo la funzione di chiusura
        if (fineEsperienzaButton != null)
        {
            fineEsperienzaButton.gameObject.SetActive(false);
            fineEsperienzaButton.onClick.AddListener(ChiudiApplicazione);
        }
    }

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

            string nomePulito = dpi.name.Replace("(Clone)", "").Trim();

            var chiave = (dpi, dpi.name);

            if (dpiDictionary.ContainsKey(chiave))
            {
                Debug.LogWarning($"[ScriptManager] Chiave duplicata per '{dpi.name}', elemento saltato.");
                continue;
            }

            dpiDictionary.Add(chiave, new List<bool> { false, false });
            Debug.Log($"[ScriptManager] Chiave creata per '{dpi.name}'")
        }

        UpdateUI();
    }

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

        // CONTROLLO SNELLO: Controlla se tutti i DPI fondamentali sono stati indossati
        VerificaDpiFondamentali();
    }

    /// <summary>
    /// Controlla se nel dizionario esistono DPI segnati come fondamentali [1] ma NON ancora indossati [0].
    /// Se non ce ne sono, attiva il bottone di fine esperienza.
    /// </summary>
    private void VerificaDpiFondamentali()
    {
        // Se non ci sono elementi nel dizionario, evitiamo di attivare il bottone per errore
        if (dpiDictionary.Count == 0) return;

        // LINQ: Cerca se c'è ALMENO UN DPI che è fondamentale (true) MA non è indossato (false)
        bool mancanoFondamentali = dpiDictionary.Values.Any(valori => 
            valori[INDICE_FONDAMENTALE] == true && valori[INDICE_INDOSSATO] == false);

        // Se NON mancano fondamentali, allora sono tutti indossati!
        if (!mancanoFondamentali && fineEsperienzaButton != null)
        {
            fineEsperienzaButton.gameObject.SetActive(true);
            Debug.Log("[ScriptManager] Tutti i DPI fondamentali sono stati indossati. Bottone attivato!");
        }
    }

    public void UpdateUI()
    {
        if (textBox == null)
        {
            Debug.LogWarning("[ScriptManager] Nessun TMP_Text assegnato in Inspector.");
            return;
        }

        // CORREZIONE: Resettiamo il punteggio a 0 prima del calcolo, altrimenti aumenta all'infinito ogni volta che si aggiorna la UI
        punteggio = 0; 

        StringBuilder sb = new StringBuilder();
        
        // Temporaneamente lasciamo il segnaposto del punteggio, lo calcoliamo prima nel ciclo e lo inseriamo dopo
        StringBuilder righeDpi = new StringBuilder();

        foreach (var coppia in dpiDictionary)
        {
            string nomeDpi = coppia.Key.Item2;
            bool indossato = coppia.Value[INDICE_INDOSSATO];
            bool fondamentale = coppia.Value[INDICE_FONDAMENTALE];

            if (!indossato)
            {
                righeDpi.AppendLine($"☐\tIndossa \"{nomeDpi}\"");
            }
            else
            {
                string colore = fondamentale ? "green" : "orange";
                punteggio += fondamentale ? 30 : 20;
                righeDpi.AppendLine($"<color={colore}><s>☑\tIndossato \"{nomeDpi}\"</s></color>");
            }
        }

        // Costruiamo la stringa finale con il punteggio corretto in cima
        sb.AppendLine($"Punteggio: {punteggio}");
        sb.Append(righeDpi.ToString());

        textBox.text = sb.ToString();
    }

    /// <summary>
    /// Gestisce la chiusura totale dell'applicazione VR nella build finale.
    /// </summary>
    private void ChiudiApplicazione()
    {
        Debug.Log("[ScriptManager] Chiusura dell'applicazione nella build...");
        
        // Chiude l'applicazione (funziona su build PC, standalone Oculus/Meta Quest, ecc.)
        Application.Quit();
    }
}