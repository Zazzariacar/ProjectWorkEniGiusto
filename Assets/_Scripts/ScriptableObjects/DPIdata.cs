using UnityEngine;

[CreateAssetMenu(fileName = "DPIdata", menuName = "Scriptable Objects/DPIdata")]
public class DPIdata : ScriptableObject
{
    public string nomeDpi;
    [TextArea] public string descrizioneSpiegazione;
    public AudioClip audioSpiegazione;
    public bool isFondamentale;

    [Header("Configurazione Socket")]
    [Tooltip("Il Tag del socket dove questo oggetto deve andare (es: Socket_Elmetto)")]
    
    [Header("Configurazione Socket")]
    public bool usaSocket; // Se false, l'oggetto verrà distrutto
    public string tagSocketDestinazione;
}