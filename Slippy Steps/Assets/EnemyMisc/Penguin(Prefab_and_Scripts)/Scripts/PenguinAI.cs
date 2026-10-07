using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FloorCheck))]
public class PenguinAI : MonoBehaviour
{
    private enum State { Wander, Charging, Sliding, Braking, Cooldown }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float turnSpeed = 360f;         // how fast it tracks the target while charging
    [SerializeField] private float facingLineLength = 1f;    // debug gizmo only

    [Header("Perception")]
    [SerializeField] private float perceptionRadius = 8f;

    [Header("Wander")]
    [SerializeField] private float wanderSpeed = 1.5f;
    [SerializeField] private float wanderTurnSpeed = 120f;
    [SerializeField] private Vector2 wanderRetargetTime = new Vector2(2f, 5f);

    [Header("Attack")]
    [SerializeField] private float chargeTime = 1.5f;
    [SerializeField] private float launchSpeed = 12f;
    [SerializeField] private float safetyMargin = 0.15f;      // smaller = slidier, stops closer to the edge
    [SerializeField] private float stopSpeed = 0.1f;
    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private float cooldownTurnSpeed = 180f;  // finishes the 180 if braking didn't

    private Rigidbody body;
    private FloorCheck floorCheck;

    private State state;
    private float stateTimer;

    private Vector3 wanderDir;

    private Vector3 slideDir;
    private float brakeAcceleration;
    private float brakeTurnSpeed;
    private Quaternion turnAroundRotation;   // the "facing away" goal, set at launch

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        floorCheck = GetComponent<FloorCheck>();
    }

    private void Start()
    {
        SetState(State.Wander);
    }

    private void OnDrawGizmos()
    {
        Vector3 facingEnd = transform.position + transform.forward * facingLineLength;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, facingEnd);
        Gizmos.DrawSphere(facingEnd, 0.05f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, perceptionRadius);
    }

    // ---------- State machine core ----------

    private void SetState(State newState, float timer = 0f)
    {
        state = newState;
        stateTimer = timer;

        if (newState == State.Wander) PickWanderDirection();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Wander:   UpdateWander();   break;
            case State.Charging: UpdateCharging(); break;
            case State.Sliding:  UpdateSliding();  break;
            case State.Braking:  UpdateBraking();  break;
            case State.Cooldown: UpdateCooldown(); break;
        }
    }

    // ---------- Target helpers ----------

    // Direction to target, flattened onto the ground plane
    private Vector3 GetFlatDirectionToTarget()
    {
        if (target == null) return Vector3.zero;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        return dir.normalized;
    }

    // Ground-plane distance, so height differences don't affect perception
    private float FlatDistanceToTarget()
    {
        if (target == null) return Mathf.Infinity;
        Vector3 offset = target.position - transform.position;
        offset.y = 0f;
        return offset.magnitude;
    }

    private void FaceTarget()
    {
        Vector3 dir = GetFlatDirectionToTarget();
        if (dir == Vector3.zero) return;

        Quaternion goal = Quaternion.LookRotation(dir, Vector3.up);
        body.MoveRotation(Quaternion.RotateTowards(
            body.rotation, goal, turnSpeed * Time.fixedDeltaTime));
    }

    // ---------- Wander ----------

    private void UpdateWander()
    {
        if (FlatDistanceToTarget() <= perceptionRadius && floorCheck.IsGrounded())
        {
            StopMoving();
            SetState(State.Charging, chargeTime);
            return;
        }

        stateTimer -= Time.fixedDeltaTime;
        if (stateTimer <= 0f) PickWanderDirection();

        Quaternion goal = Quaternion.LookRotation(wanderDir, Vector3.up);
        body.MoveRotation(Quaternion.RotateTowards(
            body.rotation, goal, wanderTurnSpeed * Time.fixedDeltaTime));

        Vector3 forward = FlatForward();
        if (Vector3.Angle(forward, wanderDir) > 5f)
        {
            StopMoving();
            return;
        }

        if (!floorCheck.IsGrounded())
        {
            StopMoving();
            TurnAround(forward);
            return;
        }

        SetFlatVelocity(forward * wanderSpeed);
    }

    private void PickWanderDirection()
    {
        float angle = Random.Range(0f, 360f);
        wanderDir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        stateTimer = Random.Range(wanderRetargetTime.x, wanderRetargetTime.y);
    }

    private void TurnAround(Vector3 currentForward)
    {
        wanderDir = Quaternion.Euler(0f, Random.Range(-60f, 60f), 0f) * -currentForward;
        stateTimer = Random.Range(wanderRetargetTime.x, wanderRetargetTime.y);
    }

    // ---------- Charging ----------

    private void UpdateCharging()
    {
        StopMoving();
        FaceTarget();                        // the telegraph: track the player while winding up
        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer <= 0f) Launch();
    }

    private void Launch()
    {
        Vector3 dir = GetFlatDirectionToTarget();

        if (dir == Vector3.zero || !floorCheck.IsGrounded())
        {
            SetState(State.Wander);
            return;
        }

        slideDir = dir;
        turnAroundRotation = Quaternion.LookRotation(-slideDir, Vector3.up); // the 180 goal, set once
        transform.rotation = Quaternion.LookRotation(slideDir, Vector3.up);
        body.linearVelocity = slideDir * launchSpeed;   // body.velocity on older Unity
        SetState(State.Sliding);
    }

    // ---------- Sliding ----------

    private void UpdateSliding()
    {
        float speed = FlatVelocity().magnitude;

        if (speed < stopSpeed)
        {
            SetState(State.Cooldown, cooldownTime);
            return;
        }

        if (!floorCheck.IsGrounded())
        {
            BeginBraking(speed, floorCheck.ForwardOffset);
        }
    }

    // ---------- Braking ----------

    private void BeginBraking(float speed, float edgeDistance)
    {
        // The edge could be up to one physics step closer than the probe suggests
        float stepTravel = speed * Time.fixedDeltaTime;
        float stopDistance = Mathf.Max(edgeDistance - stepTravel - safetyMargin, 0.1f);

        brakeAcceleration = (speed * speed) / (2f * stopDistance);   // a = v^2 / 2d, exact
        float stopTime = speed / brakeAcceleration;                  // t = v / a

        float angle = Quaternion.Angle(body.rotation, turnAroundRotation);
        brakeTurnSpeed = angle / stopTime;

        SetState(State.Braking);
    }

    private void UpdateBraking()
    {
        body.MoveRotation(Quaternion.RotateTowards(
            body.rotation, turnAroundRotation, brakeTurnSpeed * Time.fixedDeltaTime));

        if (Vector3.Dot(FlatVelocity(), slideDir) <= stopSpeed)
        {
            StopMoving();
            SetState(State.Cooldown, cooldownTime);
            return;
        }

        body.AddForce(-slideDir * brakeAcceleration, ForceMode.Acceleration);
    }

    // ---------- Cooldown ----------

    private void UpdateCooldown()
    {
        StopMoving();

        // Finish the 180 if braking (or friction) left it short
        body.MoveRotation(Quaternion.RotateTowards(
            body.rotation, turnAroundRotation, cooldownTurnSpeed * Time.fixedDeltaTime));

        stateTimer -= Time.fixedDeltaTime;
        if (stateTimer <= 0f) SetState(State.Wander);
    }

    // ---------- Movement helpers ----------

    private Vector3 FlatVelocity()
    {
        Vector3 v = body.linearVelocity;
        return new Vector3(v.x, 0f, v.z);
    }

    private void SetFlatVelocity(Vector3 flat)
    {
        body.linearVelocity = new Vector3(flat.x, body.linearVelocity.y, flat.z);
    }

    private void StopMoving() => SetFlatVelocity(Vector3.zero);

    private Vector3 FlatForward()
    {
        Vector3 f = body.rotation * Vector3.forward;
        f.y = 0f;
        return f.normalized;
    }
}