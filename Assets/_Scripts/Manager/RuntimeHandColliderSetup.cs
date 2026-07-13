using UnityEngine;

public class RuntimeHandColliderSetup : MonoBehaviour
{
    [SerializeField] private string attachObjectName = "Attach";
    [SerializeField] private string handTag = "PlayerHand";
    [SerializeField] private float colliderRadius = 0.04f;
    [SerializeField] private float retryInterval = 0.5f;

    private void Start()
    {
        // Riprova ogni "retryInterval" secondi finché non trova l'oggetto giusto
        InvokeRepeating(nameof(TrySetupCollider), retryInterval, retryInterval);
    }

    private void TrySetupCollider()
    {
        Transform attachPoint = FindDeepChild(transform, attachObjectName);

        if (attachPoint == null)
        {
            // Non trovato ancora, il prossimo Invoke riproverà
            return;
        }

        if (attachPoint.GetComponent<SphereCollider>() == null)
        {
            SphereCollider col = attachPoint.gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = colliderRadius;
            attachPoint.gameObject.tag = handTag;

            Debug.Log("Collider aggiunto su: " + attachPoint.name);
        }

        // Trovato e configurato: fermiamo i tentativi  
        CancelInvoke(nameof(TrySetupCollider));
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name) && child.gameObject.activeInHierarchy)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}