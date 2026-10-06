using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubberbandController : MonoBehaviour
{
    [Header("Spring Settings")]
    [SerializeField] private float baseStiffness = 15f;
    [SerializeField] private float maxStiffness = 600f;
    [SerializeField] private float maxStretch = 12f;
    [SerializeField] private float springDamping = 4f;
    [SerializeField] private float springRestLength = 1.5f;

    [SerializeField] private List<Rigidbody> playerRbs = new();
    private List<PlayerMovement> playerMovementScripts = new();

    private void FixedUpdate()
    {
        if(playerRbs == null || playerRbs.Count == 0)  return;

        Vector3 center = Vector3.zero;
        int activeCount = 0;
        foreach (Rigidbody rb in playerRbs)
        {
            if(rb != null)
            {
                if(!playerMovementScripts.Contains(rb.GetComponent<PlayerMovement>()))
                    playerMovementScripts.Add(rb.GetComponent<PlayerMovement>());
                center += rb.position;
                activeCount++;
            }
        }
        if(activeCount == 0) return;
        center /= activeCount;

        transform.position = center;

        foreach(Rigidbody rb in playerRbs)
        {
            if(playerMovementScripts[playerRbs.IndexOf(rb)] == null) continue;
            PlayerMovement playerMovement = playerMovementScripts[playerRbs.IndexOf(rb)];
            
            Vector3 toCenter = center - rb.position;
            float distance = toCenter.magnitude;
            float displacement = distance - springRestLength;
            
            if(displacement > 0)
            {
                float stretchRatio = Mathf.Clamp01(displacement / maxStretch);
                float powerCurve = stretchRatio * stretchRatio;

                Vector3 springForce = toCenter.normalized * maxStiffness * powerCurve;

                Debug.Log("AccelerationEvent: " + springForce);

                Vector3 dampingForce = springDamping * rb.linearVelocity;
                rb.AddForce(springForce - dampingForce, ForceMode.Acceleration);
            }
        }
    }
}
