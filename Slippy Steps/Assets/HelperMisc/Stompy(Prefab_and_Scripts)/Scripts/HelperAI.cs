using UnityEngine;
using System.Collections;

public class HelperAI : MonoBehaviour
{
    public enum State { Looking, Stomping, Cooldown }

    [Header("Target")]
    public Transform target;
    [SerializeField] private float targetHeight = 10f;         

    [Header("State")]
    public State state;

    [Header("Stomp")]
    [SerializeField] private bool canStomp = true;
    [SerializeField] private float stompForce = 10f;
    [SerializeField] private float stompChargeTime = 1f;
    [SerializeField] private float stompSpeed = 5f;
    [SerializeField] private float stompCooldown = 5f;

    [Header("Enemy Tracker")]
    [SerializeField] private GameObject enemyTrackerObject;
    [SerializeField] private EnemyTracker enemyTracker;
    [SerializeField] private Vector3 enemyPosition;

    private void Awake()
    {
        if (enemyTrackerObject != null)
        {
            enemyTracker = enemyTrackerObject.GetComponent<EnemyTracker>();
            Debug.Log("HelperAI: EnemyTracker component found.");
        }
        SetState(State.Looking);
    }

    public void SetState(State newState)
    {
        state = newState;
        switch (state)
        {
            case State.Looking:
                // Logic for Looking state
                StartCoroutine(LookCoroutine());
                break;
            case State.Stomping:
                StartCoroutine(StompCoroutine(enemyPosition));
                break;
            case State.Cooldown:
                canStomp = false;
                StartCoroutine(CooldownCoroutine());
                break;
        }
    }

    public void EnemiesInRange(Vector3 averageEnemyPosition)
    {
        if (state == State.Looking && canStomp)
        {
            enemyPosition = averageEnemyPosition;
            SetState(State.Stomping);
        }
    }

    private IEnumerator StompCoroutine(Vector3 targetPosition)
    {
        Debug.Log("Helper is stomping at position: " + targetPosition);
        Vector3 startPosition = transform.position;
        Vector3 chargeTargetPosition = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);

        
        // Charge up the stomp by moving into the target's x/z while keeping the same y.
        Debug.Log("Helper is charging for stomp at position: " + chargeTargetPosition);
        float chargeTimer = 0f;
        while (chargeTimer < stompChargeTime)
        {
            chargeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(chargeTimer / stompChargeTime);
            transform.position = Vector3.Lerp(startPosition, chargeTargetPosition, t);
            yield return null;
        }

        // Once the helper is in position, fall down at stompSpeed and detect when it reaches the ground.
        Debug.Log("Helper is falling for stomp.");
        int floorLayer = LayerMask.NameToLayer("Floor");
        if (floorLayer == -1)
        {
            Debug.LogError("HelperAI cannot stomp because the Floor layer is missing.", this);
            yield break;
        }

        int floorMask = 1 << floorLayer;
        Collider helperCollider = GetComponent<Collider>();
        float groundOffset = helperCollider != null ? helperCollider.bounds.extents.y : 0f;
        while (true)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - stompSpeed * Time.deltaTime, transform.position.z);

            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 1.5f, floorMask))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + groundOffset, transform.position.z);
                break;
            }

            yield return null;
        }

        Debug.Log("Helper has stomped enemies.");
        // apply stomp
        if (enemyTracker != null)
        {
            enemyTracker.StompEnemies(stompForce);
        }

        SetState(State.Cooldown);
    }

    private IEnumerator CooldownCoroutine()
    {
        Debug.Log("Helper is in cooldown.");
        Vector3 groundPosition = transform.position;

        float cooldownTimer = 0f;
        while (cooldownTimer < stompCooldown)
        {
            cooldownTimer += Time.deltaTime;
            float t = Mathf.Clamp01(cooldownTimer / stompCooldown);
            Vector3 playerPosition = target.position;
            Vector3 riseTargetPosition = new Vector3(playerPosition.x, targetHeight, playerPosition.z);
            transform.position = Vector3.Lerp(groundPosition, riseTargetPosition, t);
            yield return null;
        }

        Vector3 finalPlayerPosition = target.position;
        transform.position = new Vector3(finalPlayerPosition.x, targetHeight, finalPlayerPosition.z);
        canStomp = true;
        SetState(State.Looking);
    }
    private IEnumerator LookCoroutine()
    {
        Debug.Log("Helper is looking for enemies.");
        while (state == State.Looking)
        {
            Vector3 playerPosition = target.position;
            transform.position = new Vector3(playerPosition.x, targetHeight, playerPosition.z);
            yield return null;
        }
    }
}
