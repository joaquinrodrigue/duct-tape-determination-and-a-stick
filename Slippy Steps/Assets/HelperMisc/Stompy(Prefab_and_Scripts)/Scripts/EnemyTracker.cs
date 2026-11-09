using UnityEngine;
using System.Collections.Generic;

public class EnemyTracker : MonoBehaviour
{
    [Header("Tracking Information")]
    public int enemyAmountThreshold = 2;
    public Vector3 averageEnemyPosition;
    public List<GameObject> enemiesInRange = new List<GameObject>();

    [Header("Helper AI Reference")]
    public GameObject helperAIObject; // Reference to the GameObject that has the HelperAI script
    public HelperAI helperAI;

    private void Awake()
    {
        if (helperAIObject != null)
        {
            helperAI = helperAIObject.GetComponent<HelperAI>();
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other.gameObject);
            Debug.Log("Enemy added: " + other.gameObject.name);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.gameObject);
            Debug.Log("Enemy removed: " + other.gameObject.name);
        }
    }

    public Vector3 GetAverageEnemyPosition()
    {
        if (enemiesInRange.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 totalPosition = Vector3.zero;
        int validEnemyCount = 0;

        foreach (GameObject enemy in enemiesInRange)
        {
            if (enemy != null)
            {
                totalPosition += enemy.transform.position;
                validEnemyCount++;
            }
        }

        if (validEnemyCount == 0)
        {
            return Vector3.zero;
        }
        
        Debug.Log("Average enemy position calculated: " + (totalPosition / validEnemyCount));
        return totalPosition / validEnemyCount;
    }

    private void FixedUpdate()
    {
        if (enemiesInRange.Count > enemyAmountThreshold)
        {
            // Call the EnemiesInRange method on the HelperAI script attached to this GameObject
            if (helperAI != null)
            {
                averageEnemyPosition = GetAverageEnemyPosition();
                helperAI.EnemiesInRange(averageEnemyPosition);

            }
        }
        else
        {
            return;

        }
    }
    
    public void StompEnemies(float stompForce)
    {
        foreach (GameObject enemy in enemiesInRange)
        {
            if (enemy != null)
            {
                // Apply a upward force to the enemy's Rigidbody to simulate a stomp effect
                Rigidbody enemyRigidbody = enemy.GetComponent<Rigidbody>();
                if (enemyRigidbody != null)
                {
                    enemyRigidbody.AddForce(Vector3.up * stompForce, ForceMode.Impulse);
                }
                PenguinAI enemyAI = enemy.GetComponent<PenguinAI>();
                if (enemyAI != null)
                {
                    enemyAI.SetState(PenguinAI.State.Crash, enemyAI.crashTime);
                }
            }
        }
    }
    
}
