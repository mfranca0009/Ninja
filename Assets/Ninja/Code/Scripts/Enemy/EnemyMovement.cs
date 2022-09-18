using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public class WaypointInfo
{
    // Currently using the indices of the array to retrieve the proper waypoints in order. We may not need waypoint IDs.
    // The only reason we would need this is if we went out of order when inputting waypoints within the array
    // in the editor at the moment.
    // public int waypointId;
    
    public Vector2 waypointPosition;
    public float delay;
    public bool shouldRun;
    
    public bool WaypointReached { get; set; }
}


// NORMAL FUNCTIONALITY
// TODO: Implement sight detection to track when player is in front
// TODO: Implement chase behavior when sight detection is complete
// TODO: Implement "return to pathing" behavior to stop tracking player when they've left sight range
// TODO: Implement combat capability (health/death, attacking, etc.)

// EXTRA FEATURES
// TODO: Implement sound detection based on player footsteps?
// TODO: Implement capability of jumping?

public class EnemyMovement : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Movement Speeds")]
    
    [Tooltip("How fast the enemy AI will walk")]
    [SerializeField] private float walkSpeed = 70f;
    
    [Tooltip("How fast the enemy AI will run")]
    [SerializeField] private float runSpeed = 100f;

    [Header("Movement Type")]
    
    [Tooltip("Enable waypoint movement")]
    [SerializeField] private bool applyWaypointMovement;
    
    [Tooltip("Enable random movement")]
    [SerializeField] private bool applyRandomMovement = true;

    [Header("Waypoint Movement Settings")] 
    
    [Tooltip("Enable looping of the waypoint path from start-to-finish and finish-to-start")]
    [SerializeField] private bool loopWaypointPath = true;
    
    [Tooltip("An array of waypoints that can create a waypoint path")]
    [SerializeField] private WaypointInfo[] waypoints;
    
    [Tooltip("The amount of tolerance allowed for the AI to reach its waypoint position")]
    [SerializeField] private float waypointReachedTolerance = 0.03f;

    [Header("Random Movement Settings")]
    
    [Tooltip("Enable randomized speed to determine walking and running at random during random movement")]
    [SerializeField] private bool randomizeRunWalk;
    
    [Tooltip("Enable only run speed during random movement")]
    [SerializeField] private bool alwaysRun;

    [Tooltip("Enable only walk speed during random movement")] 
    [SerializeField] private bool alwaysWalk = true;
    
    [Tooltip("Enable the option to never have a delay during random movement when reaching destination positions")]
    [SerializeField] private bool noRandomDelay;
    
    [Tooltip("Set the max distance the AI can travel from its home/spawn position")]
    [SerializeField] private float maxTravelDistance = 3f;
    
    [Tooltip("The amount of tolerance allowed for the AI to reach its destination position")]
    [SerializeField] private float randomPointReachedTolerance = 0.03f;
    
    [Tooltip("The minimum delay to stop the AI for when reaching a destination position")]
    [SerializeField] private float minDelay = 1.5f;
    
    [Tooltip("The maximum delay to stop the AI for when reaching a destination position")]
    [SerializeField] private float maxDelay = 3f;

    #endregion
    
    #region Private Fields
    private Rigidbody2D _rigidBody;
    private Animator _animator;
    private Vector2 _homePos;
    private bool _isGrounded;
    
    // Enemy Scripts
    private Health _health;
    private EnemyCombat _enemyCombat;

    // Random Movement System
    private Vector2 _destPos;
    private bool _isHome;
    private bool _destReached;
    private bool _randomMoveDelayed;
    private float _randMoveDelayTimer;

    // Waypoint Movement System
    private int _currentWpId;
    private bool _returningToFirstPoint;
    private bool _waypointPathDelayed;
    private float _waypointDelayTimer;

    #endregion
    
    #region Unity Events
    
    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _enemyCombat = GetComponent<EnemyCombat>();
        
        // Only one movement type can be active at a time, default to random movement.
        if (applyWaypointMovement && applyRandomMovement)
            applyWaypointMovement = false;

        _currentWpId = 0;
    }

    // Update is called once per frame
    private void Update()
    {
        GroundCheckUpdate();

        if (_isGrounded && _homePos == Vector2.zero)
            _homePos = _rigidBody.position;
        
        _animator.SetFloat("VelocityX", _rigidBody.velocity.x);
        _animator.SetFloat("VelocityY", _rigidBody.velocity.y);
        _animator.SetBool("HasVelocityX", _rigidBody.velocity.x != 0);

        // Delay timer updates
        WaypointDelayUpdate();
        RandomMoveDelayUpdate();
    }

    private void FixedUpdate()
    {
        RandomMovementFixedUpdate();
        WaypointMovementFixedUpdate();
        ChaseMovementFixedUpdate();
    }

    #endregion
    
    #region Fixed Update Methods
    
    /// <summary>
    /// AI random movement update, if random movement type is enabled.
    /// AI will move randomly based on a set maximum distance it can travel from its home/spawn position.
    /// </summary>
    private void RandomMovementFixedUpdate()
    {
        if (!applyRandomMovement || !_isGrounded || _randomMoveDelayed || _enemyCombat.ChaseTarget ||
            _enemyCombat.InCombat)
            return;
        
        Vector2 currentPos = _rigidBody.position;
        
        if (_destReached || _destPos == Vector2.zero)
        {
            float randomX = Random.Range(_homePos.x - maxTravelDistance, _homePos.x + maxTravelDistance);
            _destPos = new Vector2(randomX + maxTravelDistance, _homePos.y);

            if (noRandomDelay)
                return;
            
            _rigidBody.velocity = Vector2.zero;
            _randMoveDelayTimer = Random.Range(minDelay, maxDelay);
            _randomMoveDelayed = true;
        }
        else
        {
            FlipSprite();
            
            float currentSpeed = alwaysWalk switch
            {
                true => walkSpeed,
                false when alwaysRun => runSpeed,
                false when randomizeRunWalk => Random.Range(1, 100) > 50 ? walkSpeed : runSpeed,
                _ => walkSpeed
            };

            _rigidBody.velocity = new Vector2((_destPos.x < currentPos.x ? Vector2.left.x : Vector2.right.x)
                                              * currentSpeed, 0f) * Time.deltaTime;
        }
        
        _destReached = Vector2.Distance(currentPos, _destPos) <= randomPointReachedTolerance;
    }
    
    /// <summary>
    /// AI waypoint movement update, if waypoint movement type is enabled.
    /// AI will move along a waypoint path that has been created through the Unity Editor.
    /// </summary>
    private void WaypointMovementFixedUpdate()
    {
        // Check conditions before proceeding with waypoint movement.
        if (!applyWaypointMovement || waypoints.Length == 0 || !_isGrounded ||
            _waypointPathDelayed || (_currentWpId == waypoints.Length && !loopWaypointPath) ||
            _enemyCombat.ChaseTarget || _enemyCombat.InCombat)
            return;

        // Looping waypoint from reverse or beginning, this will initially correct the direction
        // and reset all waypoints that were previously reached.
        if (_currentWpId == waypoints.Length || _currentWpId == 0)
        {
            if (_currentWpId == waypoints.Length)
            {
                _currentWpId--;
                _returningToFirstPoint = true;
            }
            else
                _returningToFirstPoint = false;

            foreach (WaypointInfo wp in waypoints)
                wp.WaypointReached = false;
        }

        // Retrieve current AI position and waypoint we are on.
        Vector2 currentPos = _rigidBody.position;
        WaypointInfo waypoint = waypoints[_currentWpId];

        // Check if the waypoint position and AI's current position is within range of the reached tolerance to trigger
        // the waypoint has been reached. The tolerance is added since floats are never always exact when comparing,
        // if the correct amount of tolerance is not used then the AI will never stop at the waypoint.
        if (Vector2.Distance(waypoint.waypointPosition, currentPos) < waypointReachedTolerance)
        {
            waypoint.WaypointReached = true;
            
            // If the waypoint had a delay, then stop the AI and trigger the delayed state so it will tick the
            // given delay time.
            if (waypoint.delay > 0f)
            {
                _rigidBody.velocity = Vector2.zero;
                _waypointPathDelayed = true;
                _waypointDelayTimer = waypoint.delay;
            }
        }
        
        // Update AI movement if the waypoint has not been reached..
        if (!waypoint.WaypointReached)
        {
            FlipSprite();
            float currentSpeed = waypoint.shouldRun ? runSpeed : walkSpeed;

            _rigidBody.velocity =
                new Vector2((waypoint.waypointPosition.x < currentPos.x ? Vector2.left.x : Vector2.right.x)
                            * currentSpeed, 0f) * Time.deltaTime;
        }
        // Waypoint reached, now update the current waypoint we are on..
        else
        {
            if (_returningToFirstPoint)
                _currentWpId--;
            else
                _currentWpId++;
        }
    }

    private void ChaseMovementFixedUpdate()
    {
        if (!_enemyCombat.ChaseTarget || _enemyCombat.InCombat)
            return;
        
        FlipSprite();
        
        Vector2 currentPos = _rigidBody.position;
        // float currentSpeed = waypoint.shouldRun ? runSpeed : walkSpeed;

        _rigidBody.velocity =
            new Vector2((_enemyCombat.Target.transform.position.x < currentPos.x ? Vector2.left.x : Vector2.right.x)
                        * runSpeed, 0f) * Time.deltaTime;
    }
    
    #endregion

    #region Update Methods
    
    /// <summary>
    /// Update the timer for a waypoint delay that was triggered
    /// </summary>
    private void WaypointDelayUpdate()
    {
        if (!_waypointPathDelayed)
            return;

        if (_waypointDelayTimer <= 0f)
        {
            _waypointPathDelayed = false;
            _waypointDelayTimer = 0f;
        }
        else
            _waypointDelayTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Update the timer for a random movement position delay that was triggered
    /// </summary>
    private void RandomMoveDelayUpdate()
    {
        if (!_randomMoveDelayed)
            return;

        if (_randMoveDelayTimer <= 0f)
        {
            _randomMoveDelayed = false;
            _randMoveDelayTimer = 0f;
        }
        else
            _randMoveDelayTimer -= Time.deltaTime;
    }
    
    /// <summary>
    /// Update check to determine if AI is on ground or in-air/falling
    /// </summary>
    private void GroundCheckUpdate()
    {
        // Retrieve initial ground check
        _isGrounded = _rigidBody.IsTouchingLayers(LayerMask.GetMask("Ground"));
        _animator.SetBool("IsGrounded", _isGrounded);
    
        // if (!_isGrounded)
        //     _inAir = true;
        //
        // // Update box collider bounds, then proceed to raycast landing check.
        // _boxBounds = _boxCollider.bounds;
        // RaycastHit2D hitResult = Physics2D.BoxCast(_boxBounds.center, _boxBounds.size,
        //     0f, Vector2.down, landingTolerance, LayerMask.NameToLayer("Ground"));
        //     
        // if (hitResult.collider != null)
        //     _isLanding = true;
        //
        // // Update animator landing state
        // _animator.SetBool("IsLanding", _isLanding);
        //
        // if (!_isGrounded)
        //     return;
        //
        // // From this point onward, execute clean-up from a complete jump (jump, in-air, land).
        //
        // // Clear horizontal velocity when landing from a jump.
        // if (_inAir && _isLanding)
        //     _rigidBody.velocity = new Vector2(0, _rigidBody.velocity.y);
        //
        // // Reset
        // _inAir = false;
        // _jumpCount = 0;
        // _isLanding = false;
    }

    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Flip sprite based on current facing direction compared to the rigidbody's velocity if needed
    /// </summary>
    private void FlipSprite()
    {
        bool isFacingLeft = transform.localScale.x < 0;
        if (_rigidBody.velocity.x > 0 && !isFacingLeft || _rigidBody.velocity.x < 0 && isFacingLeft ||
            _rigidBody.velocity.x == 0)
            return;

        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
    }

    /// <summary>
    /// Checks whether the AI can proceed with waypoint movement or not
    /// </summary>
    /// <returns>Returns true if AI can continue waypoint movement, otherwise false.</returns>
    private bool AllowWaypointMovement()
    {
        // TODO
        return false;
    }

    /// <summary>
    /// Checks whether the AI can proceed with random movement or not
    /// </summary>
    /// <returns>Returns true if AI can continue random movement, otherwise false.</returns>
    private bool AllowRandomMovement()
    {
        // TODO
        return false;
    }
    
    #endregion
}
