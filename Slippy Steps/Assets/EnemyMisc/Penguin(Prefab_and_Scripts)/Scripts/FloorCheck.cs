using UnityEngine;

public class FloorCheck : MonoBehaviour
{
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private float forwardOffset = 0.5f;
    [SerializeField] private float floorClearance = 0.05f;
    [SerializeField] private float checkDistance = 0.1f;

    // Lets other scripts (PenguinAI) read how far ahead the probe sits
    public float ForwardOffset => forwardOffset;

    public bool IsGrounded()
    {
        return Physics.Raycast(GetCheckOrigin(), Vector3.down, checkDistance, floorLayer);
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = GetCheckOrigin();
        Vector3 end = origin + Vector3.down * checkDistance;
        bool isGrounded = Physics.Raycast(origin, Vector3.down, checkDistance, floorLayer);

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(origin, 0.025f);
        Gizmos.DrawWireSphere(end, 0.025f);
    }

    private Vector3 GetCheckOrigin()
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        float floorLevel = transform.position.y;

        Collider objectCollider = GetComponent<Collider>();
        if (objectCollider != null)
        {
            floorLevel = objectCollider.bounds.min.y;
        }

        return transform.position + forward * forwardOffset + Vector3.up * (floorLevel - transform.position.y + floorClearance);
    }
}