using UnityEngine;

public class HelperAI : MonoBehaviour
{
    public enum State { Looking, Stomping, Cooldown }

    [Header("Target")]
    public Transform target;

    [Header("State")]
    public State state;
}
