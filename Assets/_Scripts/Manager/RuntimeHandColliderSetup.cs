using UnityEngine;

/// <summary>
/// Va messo UNA VOLTA SOLA su un parent comune (es. "XR Origin Hands (XR Rig)").
/// Cerca ricorsivamente tutti gli oggetti "Attach" generati a runtime da XRI
/// (sia per controller che per mani, sia sinistra che destra) e installa su
/// ciascuno un piccolo componente switcher che sposta il collider di gesto
/// sulla sorgente attiva in quel momento (controller o mano, a seconda di
/// cosa l'utente sta usando in quell'istante).
/// </summary>
public class RuntimeHandColliderSetup : MonoBehaviour
{
    [Header("Ricerca oggetti Attach")]
    [SerializeField] private string attachObjectName = "Attach";
    [SerializeField] private float retryInterval = 0.5f;

    [Header("Collider di gesto")]
    [SerializeField] private string handTag = "PlayerHand";
    [SerializeField] private float colliderRadius = 0.04f;

    private static GameObject sharedLeftHolder;
    private static GameObject sharedRightHolder;

    private void Start()
    {
        InvokeRepeating(nameof(TryInstall), retryInterval, retryInterval);
    }

    private void TryInstall()
    {
        bool foundAny = false;

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (!child.name.Contains(attachObjectName)) continue;
            if (child.GetComponent<AttachGestureSwitcher>() != null) continue; // già installato

            bool isLeft = child.name.Contains("Left");
            var side = isLeft ? AttachGestureSwitcher.Side.Left : AttachGestureSwitcher.Side.Right;

            var switcher = child.gameObject.AddComponent<AttachGestureSwitcher>();
            switcher.Configure(side, handTag, colliderRadius, this);

            Debug.Log("[RuntimeHandColliderSetup] Installato switcher su: " + child.name + " (side: " + side + ")");
            foundAny = true;
        }

        if (foundAny)
            CancelInvoke(nameof(TryInstall));
    }

    // Chiamato dallo switcher quando il suo Attach viene attivato
    public void AttachToSource(Transform source, AttachGestureSwitcher.Side side, string tag, float radius)
    {
        GameObject holder = side == AttachGestureSwitcher.Side.Left ? sharedLeftHolder : sharedRightHolder;

        if (holder == null)
        {
            holder = new GameObject("PlayerHandTrigger_" + side);

            SphereCollider col = holder.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;
            holder.tag = tag;

            Rigidbody rb = holder.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            if (side == AttachGestureSwitcher.Side.Left) sharedLeftHolder = holder;
            else sharedRightHolder = holder;
        }

        holder.transform.SetParent(source);
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;

        Debug.Log("[RuntimeHandColliderSetup] Collider gesto (" + side + ") passato a: " + source.name);
    }

    // Piccola classe interna: nessun file separato, resta tutto in un solo script
    public class AttachGestureSwitcher : MonoBehaviour
    {
        public enum Side { Left, Right }

        private Side side;
        private string handTag;
        private float colliderRadius;
        private RuntimeHandColliderSetup manager;

        public void Configure(Side newSide, string tag, float radius, RuntimeHandColliderSetup owner)
        {
            side = newSide;
            handTag = tag;
            colliderRadius = radius;
            manager = owner;
        }

        private void OnEnable()
        {
            manager.AttachToSource(transform, side, handTag, colliderRadius);
        }
    }
}