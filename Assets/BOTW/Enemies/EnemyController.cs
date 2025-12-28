using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{ 
    enum State { Patrol, Chase, Circle }

    [SerializeField]
    private GameObject playerObject;
    [SerializeField]
    private Transform player; 
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Animator anim;

    [Header("Detection Settings")]
    public float detectionRange = 30f;       
    public float fieldOfView = 60f;           
    public float loseSightTime = 3f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    private bool isAttacking = false;
    private float attackTimer = 0f;

    [Header("Circling (waypoint orbit)")]
    public float circleRadius = 5f;
    public int orbitPointCount = 10;
    public float rebuildOrbitThreshold = 1.5f;
    public float minPointDistance = 0.5f;
    public float orbitPointSampleDistance = 1.5f; 
    private List<Vector3> orbitPoints = new List<Vector3>();
    private int currentOrbitIndex = 0;
    private Vector3 lastPlayerPos = Vector3.zero;


    [Header("Patrol Settings")]
    public Transform[] patrolPoints;         
    private int currentPatrolIndex;
    public float waypointTolerance = 1f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private bool isWaiting = false;
    private float waitTimer = 0f;

    private float timeSinceLastSeen = Mathf.Infinity;
    private State state = State.Patrol;
    private bool chasingPlayer = false;
    private bool isCircling;

    void Start()
    {
        playerObject = GameObject.Find("Player");
        player = playerObject.GetComponent<Transform>();  
        anim = GetComponent<Animator>();
      
        agent = GetComponent<NavMeshAgent>();
        PopulatePatrolPoints();

        if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

   

    void Update()
    {
        // Safety
        if (player == null) return;

        // sight logic
        if (PlayerInSight())
        {
            chasingPlayer = true;
            agent.speed = 5f;
            timeSinceLastSeen = 0f;
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
            if (timeSinceLastSeen > loseSightTime)
                chasingPlayer = false;
            agent.speed = 3.5f;
        }

        if (isWaiting == true) 

        {
            anim.SetBool("IsIdle", true);

        }

        if (isWaiting == false)
        {
            anim.SetBool("IsIdle", false);
        }

        // attack cooldown
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                isAttacking = false;
        }

        // State transitions
        switch (state)
        {
            case State.Patrol:

                
                if (chasingPlayer)
                {
                    state = State.Chase;
                }
                PatrolUpdate();
                break;

            case State.Chase:

                
                if (!chasingPlayer)
                {
                    state = State.Patrol;
                    if (patrolPoints != null && patrolPoints.Length > 0)
                        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }
                else
                {
                    float dist = Vector3.Distance(transform.position, player.position);
                    if (dist <= attackRange)
                    {
                        state = State.Circle;
                        BuildOrbitPoints(); 
                        if (orbitPoints.Count > 0)
                        {
                            agent.SetDestination(orbitPoints[currentOrbitIndex]);
                        }
                    }
                    else
                    {
                        agent.SetDestination(player.position);
                    }
                }
                break;

            case State.Circle:
                float distance = Vector3.Distance(transform.position, player.position);

                if (distance > attackRange + 1.5f)
                {
                    state = State.Chase;
                    agent.SetDestination(player.position);
                    isCircling = false;
                    return;
                }

                if (distance > attackRange)
                {
                    Vector3 dir = (player.position - transform.position).normalized;
                    Vector3 approachPoint = player.position - dir * (attackRange * 0.9f);
                    agent.SetDestination(approachPoint);
                    FacePlayerSmooth();
                    return;
                }

                BeginCircleMovement();

                if (!isAttacking)
                {
                    PerformAttack();
                }

                FacePlayerSmooth();
                break;
        }

        UpdateAnimationState();

    }

    void PatrolUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                int nextIndex;
                do
                {
                    nextIndex = Random.Range(0, patrolPoints.Length);
                } while (nextIndex == currentPatrolIndex && patrolPoints.Length > 1);

                currentPatrolIndex = nextIndex;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < waypointTolerance)
        {
            isWaiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            agent.ResetPath();
        }
    }
    bool PlayerInSight()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > fieldOfView / 2f)
            return false;

        
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, detectionRange))
        {
            if (hit.transform == player)
            {
                return true;
            }
        }

        return false;
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
               
                int nextIndex;

              
                do
                {
                    nextIndex = Random.Range(0, patrolPoints.Length);
                }
                while (nextIndex == currentPatrolIndex && patrolPoints.Length > 1);

                currentPatrolIndex = nextIndex;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            return;
        }

      
        if (!agent.pathPending && agent.remainingDistance < waypointTolerance)
        {
            
            isWaiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);

            
            agent.ResetPath();
        }
    }

    void BuildOrbitPoints()
    {
        orbitPoints.Clear();
        lastPlayerPos = player.position;

        float angleStep = 360f / Mathf.Max(1, orbitPointCount);

        for (int i = 0; i < orbitPointCount; i++)
        {
            float angleDeg = angleStep * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 rawPos = player.position + new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad)) * circleRadius;

            if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, orbitPointSampleDistance, NavMesh.AllAreas))
            {
                if (orbitPoints.Count == 0 || Vector3.Distance(hit.position, orbitPoints[orbitPoints.Count - 1]) > minPointDistance)
                {
                    if (Vector3.Distance(hit.position, player.position) > 0.5f)
                        orbitPoints.Add(hit.position);
                }
            }
        }

        if (orbitPoints.Count < 3)
        {
            float fallbackRadius = circleRadius * 0.8f;
            for (int i = 0; i < orbitPointCount && orbitPoints.Count < 3; i++)
            {
                float angleDeg = (360f / orbitPointCount) * i;
                float angleRad = angleDeg * Mathf.Deg2Rad;
                Vector3 rawPos = player.position + new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad)) * fallbackRadius;

                if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, orbitPointSampleDistance, NavMesh.AllAreas))
                {
                    if (orbitPoints.Count == 0 || Vector3.Distance(hit.position, orbitPoints[orbitPoints.Count - 1]) > minPointDistance)
                        orbitPoints.Add(hit.position);
                }
            }
        }

        if (orbitPoints.Count == 0)
        {
            Debug.LogWarning("EnemyController: Could not create orbit points around player. Circling disabled until player moves or NavMesh allows points.");
            return;
        }

        currentOrbitIndex = Random.Range(0, orbitPoints.Count);
        agent.SetDestination(orbitPoints[currentOrbitIndex]);
    }

    void BeginCircleMovement()
    {
        if (orbitPoints == null || orbitPoints.Count == 0)
        {
            BuildOrbitPoints();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.6f)
        {
            currentOrbitIndex = (currentOrbitIndex + 1) % orbitPoints.Count;
            agent.SetDestination(orbitPoints[currentOrbitIndex]);
        }
    }


    void PerformAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        Debug.Log("Enemy attacks the player!");

    }

        void PopulatePatrolPoints()
    {
        //Distance for navpoints
        float searchRadius = 50f;

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);


        var points = new System.Collections.Generic.List<Transform>();

        foreach (Collider c in hits)
        {
            if (c.CompareTag("NavPoint"))
            {
                points.Add(c.transform);
            }
        }

        patrolPoints = points.ToArray();
    }

    void FacePlayerSmooth()
    {
        if (player == null) return;
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 7f);
        }
    }

    void UpdateAnimationState()
    {
        bool isPatrolling = state == State.Patrol;
        bool isChasing = state == State.Chase;
        bool isCirclingState = state == State.Circle;

        // walking: only while patrolling
        anim.SetBool("IsWalking", isPatrolling);

        // running: only while chasing
        anim.SetBool("IsRunning", isChasing);

        // circling: special animation
        anim.SetBool("IsCircling", isCirclingState);

        // attacking
        anim.SetBool("IsAttacking", isAttacking);
    }

    void OnDrawGizmosSelected()
    {
       //Escape range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        //NavPoint detection
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 50f);

        //Orbit points
        if (orbitPoints != null && orbitPoints.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (var p in orbitPoints)
                Gizmos.DrawSphere(p, 0.12f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(orbitPoints[currentOrbitIndex], 0.18f);
        }


        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
        Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
    }
}
        
 
