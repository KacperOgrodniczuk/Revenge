using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerLockOnManager : MonoBehaviour
{
    public PlayerManager PlayerManager { get; private set; }

    [Header("Detection Settings")]
    [Tooltip("The radius in which the player can lock onto enemies (Indicated by a blue wiresphere")]
    public float LockOnRadius = 20f;
    [Tooltip("The radius in which nearby enemies will override the default lock on calculation (Indicated by a red wiresphere")]
    public float SafetyNetRadius = 3f;
    [SerializeField] private LayerMask _detectionLayer;
    Camera _camera;

    [Header("Lock-On Targets")]
    public Transform CurrentHardLockOnTarget { get; private set; }   // Target used for hard lock-on (holding a button)
    List<Collider> _detectedTargets = new List<Collider>();

    [Header("Input Values")]
    bool _lockOnInput;
    bool _nextTargetInput;
    bool _previousTargetInput;


    private void Awake()
    {
        PlayerManager = GetComponent<PlayerManager>();
        _camera = Camera.main;
    }

    private void Update()
    {
        GetLockOnInput();
        HandleLockOn();
    }

    void GetLockOnInput()
    {
        _lockOnInput = PlayerInputManager.Instance.LockOnInput;
        _nextTargetInput = PlayerInputManager.Instance.NextTargetInput;
        _previousTargetInput = PlayerInputManager.Instance.PreviousTargetInput;

        PlayerInputManager.Instance.NextTargetInput = false;
        PlayerInputManager.Instance.PreviousTargetInput = false;
    }

    void HandleLockOn()
    {
        if (_lockOnInput)
        {
            // If we dont have a current target, find one.
            if (CurrentHardLockOnTarget == null)
            {
                // Grab all the enemies within the lock on radius and camera's frustum.
                _detectedTargets = Physics.OverlapSphere(transform.position, LockOnRadius, _detectionLayer).ToList();
                Plane[] frustumPlance = GeometryUtility.CalculateFrustumPlanes(_camera);
                Transform bestTarget = null;
                float highestScore = -1f;

                foreach (Collider enemy in _detectedTargets)
                {
                    bool isInViewFrustum = GeometryUtility.TestPlanesAABB(frustumPlance, enemy.bounds);
                    float distanceToPlayer = Vector3.Distance(transform.position, enemy.transform.position);
                    bool isInSafetyNet = distanceToPlayer <= SafetyNetRadius;

                    // Score which mandates the prioroty of which target should be targeted first.
                    float score = CalculateLockOnScore(enemy.transform, isInViewFrustum, isInSafetyNet, distanceToPlayer);

                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestTarget = enemy.transform;
                    }
                }

                CurrentHardLockOnTarget = bestTarget;
            }

            // If we have a target, and either of the switch inputs are pressed. 
            else if (CurrentHardLockOnTarget != null && (_nextTargetInput || _previousTargetInput))
            {
                // Sort by angle around the player (0 to 360) so cycling is predictable.
                var sortedTargets = _detectedTargets.Select(c => c.transform).OrderBy(t =>
                {
                    Vector3 dir = (t.position - transform.position).normalized;
                    return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                }).ToList();

                // Identify the index of the current target.
                int currentIndex = sortedTargets.IndexOf(CurrentHardLockOnTarget);

                // If we have a target and want to toggle between targets. 
                if (_nextTargetInput)
                {
                    int nextIndex = currentIndex + 1;
                    if (nextIndex >= sortedTargets.Count)
                        nextIndex = 0;

                    CurrentHardLockOnTarget = sortedTargets[nextIndex];
                }
                else if (_previousTargetInput)
                {
                    int nextIndex = currentIndex - 1;
                    if (nextIndex < 0)
                        nextIndex = sortedTargets.Count - 1;
                    CurrentHardLockOnTarget = sortedTargets[nextIndex];
                }
            }
        }
        else
        {
            CurrentHardLockOnTarget = null;
            _detectedTargets.Clear();
        }

        _nextTargetInput = false;
        _previousTargetInput = false;
    }

    float CalculateLockOnScore(Transform enemyTransform, bool isInViewFrustum, bool isInSafetyNet, float distanceFromPlayer)
    {
        float score = 0f;

        //if the enemy is within the safety net, give it a big score boost.
        if (isInSafetyNet)
        {
            score = 20f + GetDistanceScore(distanceFromPlayer, SafetyNetRadius);
        }
        // If the enemy is within the view frustum, bias score towards the centre of the screen.
        else if (isInViewFrustum)
        {
            Vector3 screenPoint = _camera.WorldToViewportPoint(enemyTransform.position);
            Vector2 screenCentre = new Vector2(0.5f, 0.5f);
            float distanceToCentre = Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), screenCentre);

            score = 10f + (1f - Mathf.Clamp01(distanceToCentre));
        }
        // the rest of the enemies are scored based on distance only.
        else
        {
            score = GetDistanceScore(distanceFromPlayer, LockOnRadius);
        }

        return score;
    }

    // Helper function to make distance scoring cleaner and consistent.
    float GetDistanceScore(float currentDistance, float maxDistance)
    {
        // Inverse percentage score based on distance. e.g. Closer targets get higher scores. Scores are between 0 and 1.
        return 1f - Mathf.Clamp01(currentDistance / maxDistance);
    }

    private void OnDrawGizmos()
    {
        // Visualise lock-on and safety net radius in editor.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, LockOnRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SafetyNetRadius);
    }
}
