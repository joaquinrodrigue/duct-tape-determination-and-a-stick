using System.Collections;
using UnityEngine;

public class CrashCheck : MonoBehaviour
{
    [SerializeField] private float knockbackForce = 2f;
    public bool isGrounded = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnCollisionEnter(Collision other)
    {
        PenguinAI penguinAI = GetComponentInParent<PenguinAI>();
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerMovement playerMovement = other.gameObject.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    // playerMovement.RagdollWakeup();

                }
            }
            if (other.gameObject.CompareTag("Enemy"))
            {
                PenguinAI enemyAI = other.gameObject.GetComponent<PenguinAI>();
                if (enemyAI != null)
                {
                    // Apply Force to the enemy's Rigidbody to simulate a knockback effect
                    Rigidbody enemyRigidbody = other.gameObject.GetComponent<Rigidbody>();
                    if (enemyRigidbody != null)
                    {
                        penguinAI.StopMoving(); // Stop the penguin's movement before applying knockback
                        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                        Vector3 knockbackWithUpwardForce = knockbackDirection + (Vector3.up * 0.5f); // Add upward force
                        enemyRigidbody.AddForce(knockbackWithUpwardForce * knockbackForce, ForceMode.Impulse);
                        Debug.Log("Knockback applied to enemy: " + other.gameObject.name);
                    }
                }
            }
            Debug.Log("Penguin collided with " + other.gameObject.name);

            if (penguinAI != null)
            {
                StartCoroutine(CheckGroundBeforeCrash(penguinAI));
            }
        }
    }

    private IEnumerator CheckGroundBeforeCrash(PenguinAI penguinAI)
    {
        for (float elapsed = 0f; elapsed < 1f; elapsed += Time.deltaTime)
        {
            if (isGrounded)
            {
                penguinAI.SetState(PenguinAI.State.Crash, penguinAI.crashTime);
                yield break;
            }

            yield return null;
        }
    }
}
