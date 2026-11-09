using UnityEngine;

public class CrashCheck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Penguin collided with " + other.gameObject.name);
            PenguinAI penguinAI = GetComponentInParent<PenguinAI>();
            if (penguinAI != null)
            {
                penguinAI.SetState(PenguinAI.State.Crash, penguinAI.crashTime);
            }
        }
    }
}
