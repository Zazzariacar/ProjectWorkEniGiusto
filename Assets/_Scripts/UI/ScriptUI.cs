using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScriptUi : MonoBehaviour
{
    [Header("Configurazione")]
    [SerializeField] private DPIdata config;

    [Header("Riferimenti UI")]
    [SerializeField] private GameObject pannelloUiSpaziale;
    [SerializeField] private TextMeshProUGUI testoTitolo;
    [SerializeField] private TextMeshProUGUI testoDescrizione;
    [SerializeField] private Button bottoneIndossa;

    public bool isFondamentale;
    public static event Action<DPIdata> OnDpiEquipped;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(pannelloUiSpaziale != null)
            pannelloUiSpaziale.SetActive(false);

        if(bottoneIndossa != null)
            bottoneIndossa.onClick.AddListener(IndossaOggetto);
    }

    public void OnPinchStart()
    {
        testoTitolo.text = config.nomeDpi;
        testoDescrizione.text = config.descrizioneSpiegazione;
        isFondamentale = config.isFondamentale;

        if (isFondamentale)
        {
            testoTitolo.text += " (obbligatorio)";
        }

        pannelloUiSpaziale.SetActive(true);
    }

    public void IndossaOggetto()
    {
        OnDpiEquipped?.Invoke(config);
        gameObject.SetActive(false);
    }
}
