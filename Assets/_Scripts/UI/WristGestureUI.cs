using UnityEngine;

public class WristGestureUI : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform wrist;             // joint del polso (R_Wrist)
    [SerializeField] private GameObject checklistCanvas;   // il tuo Canvas

    [Header("Impostazioni")]
    [SerializeField] private float angleThreshold = 40f;   // quanto "verso l'alto" deve essere il palmo
    [SerializeField] private float checkInterval = 0.1f;

    [Header("Asse del palmo (da tarare)")]
    [SerializeField] private PalmAxis palmAxis = PalmAxis.Up;
    [SerializeField] private bool invertAxis = false;

    private float timer;

    private enum PalmAxis { Up, Down, Right, Left, Forward, Back }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        Vector3 palmDirection = GetPalmDirection();
        float angle = Vector3.Angle(palmDirection, Vector3.up);

        bool palmFacingUp = angle < angleThreshold;
        checklistCanvas.SetActive(palmFacingUp);
    }

    private Vector3 GetPalmDirection()
    {
        Vector3 dir = palmAxis switch
        {
            PalmAxis.Up => wrist.up,
            PalmAxis.Down => -wrist.up,
            PalmAxis.Right => wrist.right,
            PalmAxis.Left => -wrist.right,
            PalmAxis.Forward => wrist.forward,
            PalmAxis.Back => -wrist.forward,
            _ => wrist.up
        };

        return invertAxis ? -dir : dir;
    }
}