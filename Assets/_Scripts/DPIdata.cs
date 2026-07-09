using UnityEngine;

[CreateAssetMenu(fileName = "DPIdata", menuName = "Scriptable Objects/DPIdata")]
public class DPIdata : ScriptableObject
{
    public string nomeDpi;
    [TextArea] public string descrizioneSpiegazione;
    //public AudioClip audioSpiegazione;
    //public Video videoSpiegazione;
    public bool isFondamentale;
}
