using UnityEngine;

public class VisieraToggle : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private GameObject visiera; // trascina qui l'oggetto "visiera"

    [Header("Impostazioni")]
    [SerializeField] private string handTag = "PlayerHand";
    [SerializeField] private float cooldown = 1f;

    private float lastToggleTime = -999f;

   private void OnTriggerEnter(Collider other)
{
    Debug.Log("Trigger enter da: " + other.gameObject.name + " tag: " + other.tag);
  //  if (!other.CompareTag(handTag))
    //    return;

    if (Time.time - lastToggleTime < cooldown)
    {
        Debug.Log("Bloccato dal cooldown");
        return;
    }

    lastToggleTime = Time.time;
    visiera.SetActive(!visiera.activeSelf);
    Debug.Log("Visiera ora è: " + visiera.activeSelf);
}
}