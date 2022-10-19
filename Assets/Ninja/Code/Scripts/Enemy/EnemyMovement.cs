using System;
using UnityEngine;
using UnityEngine.UI;
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

// POTENTIAL EXTRA FEATURES
// TODO: Implement sound detection based on player footsteps?
// TODO: Implement capability of jumping?

public class EnemyMovement : MonoBehaviour
{
    #region Public Properties

    public bool ShouldPlayWalkRunSFX { get; set; }

    #endregion
    
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
    [SerializeField]
    private bool loopWaypointPath;
    
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

    [Header("Audio Source Settings")]
    
    [Tooltip("Dedicated movement audio source for walk/run loop")]
    [SerializeField] private AudioSource movementAudioSource;

    [Tooltip("Normal pitch amount during walk speed for walk/run sound effect")] 
    [SerializeField] private float normalWalkPitch = 1f;

    [Tooltip("Fast pitch amount during run speed for walk/run sound effect")] 
    [SerializeField] private float fastRunPitch = 1.25f;
    
    [Tooltip("Dedicated movement audio source for simultaneous one shot sound effects")] 
    [SerializeField] private AudioSource movementOneShotAudioSource;
    
    [Header("Sound Effect Settings")]

    [Tooltip("Walk/Run sound effect to play")]
    [SerializeField] private AudioClip walkRunSoundClip;
    
    #endregion
    
    #region Private Fields
    
    // Rigidbody / Physics
    private Rigidbody2D _rigidBody2D;
    
    // Animator / Animation
    private Animator _animator;

    // Random Movement System
    private Vector2 _homePos;
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
    
    // Enemy Scripts
    private Health _health;
    private EnemyCombat _enemyCombat;

    #endregion
    
    #region Unity Events
    
    private void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _enemyCombat = GetComponent<EnemyCombat>();
        
        // Only one movement type can be active at a time, default to random movement.
        if (applyWaypointMovement && applyRandomMovement)
            applyWaypointMovement = false;

        // Override default walk/run sound effect clip if one is present
        if (walkRunSoundClip)
            movementAudioSource.clip = walkRunSoundClip;
        
        _currentWpId = 0;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_health.Dead || Time.timeScale == 0f)
        {
            movementAudioSource.Stop();
            return;
        }

        GroundCheckUpdate();

        if (IsGrounded() && _homePos == Vector2.zero)
            _homePos = _rigidBody2D.position;

        _animator.SetFloat("VelocityX", _rigidBody2D.velocity.x);
        _animator.SetFloat("VelocityY", _rigidBody2D.velocity.y);
        _animator.SetBool("HasVelocityX", _rigidBody2D.velocity.x != 0);
        _animator.SetBool("HasVelocityY", _rigidBody2D.velocity.y != 0);
        
        UpdateWalkRunSfx();

        // Delay timer updates
        WaypointDelayUpdate();
        RandomMoveDelayUpdate();
    }

    private void FixedUpdate()
    {
        if (_health.Dead)
            return;
        
        RandomMovementFixedUpdate();
        WaypointMovementFixedUpdate();
        ChaseMovementFixedUpdate();
        AdvanceOnTargetMovementFixedUpdate();
        InvestigateMovementFixedUpdate();
    }

    #endregion
    
    #region Fixed Update Methods
    
    /// <summary>
    /// AI random movement update, if random movement type is enabled.
    /// AI will move randomly based on a set maximum distance it can travel from its home/spawn position.
    /// </summary>
    private void RandomMovementFixedUpdate()
    {
        if (!AllowRandomMovement())
            return;

        Vector2 currentPos = _rigidBody2D.position;
        
        if (_destReached || _destPos == Vector2.zero)
        {
            float randomX = Random.Range(_homePos.x - maxTravelDistance, _homePos.x + maxTravelDistance);
            _destPos = new Vector2(randomX + maxTravelDistance, _homePos.y);

            if (noRandomDelay)
                return;
            
            _rigidBody2D.velocity = Vector2.zero;
            _randMoveDelayTimer = Random.Range(minDelay, maxDelay);
            _randomMoveDelayed = true;
        }
        else
        {
            FlipSprite();
            FlipHealthUI();

            float currentSpeed = alwaysWalk switch
            {
                true => walkSpeed,
                false when alwaysRun => runSpeed,
                false when randomizeRunWalk => Random.Range(1, 100) > 50 ? walkSpeed : runSpeed,
                _ => walkSpeed
            };

            UpdateWalkRunSfxPitch(currentSpeed);
            
            _rigidBody2D.velocity = new Vector2((_destPos.x < currentPos.x ? Vector2.left.x : Vector2.right.x)
                                                * currentSpeed, 0f) * Time.deltaTime;
        }

        _destReached = Mathf.Abs(Vector2.Distance(currentPos, _destPos)) <= randomPointReachedTolerance;
    }
    
    /// <summary>
    /// AI waypoint movement update, if waypoint movement type is enabled.
    /// AI will move along a waypoint path that has been created through the Unity Editor.
    /// </summary>
    private void WaypointMovementFixedUpdate()
    {
        if (!AllowWaypointMovement())
            return;

            // Looping waypoint from reverse or beginning, this will initially correct the direction
        // and reset all waypoints that were previously reached.
        switch (_currentWpId == waypoints.Length)
        {
            case true:
                _currentWpId--;
                _returningToFirstPoint = true;
                ResetWaypoints();
                break;
            case false when _currentWpId == 0:
                _returningToFirstPoint = false;
                ResetWaypoints();
                break;
        }

            // Retrieve current AI position and waypoint we are on.
        Vector2 currentPos = _rigidBody2D.position;
        WaypointInfo waypoint = waypoints[_currentWpId];

        // Check if the waypoint position and AI's current position is within range of the reached tolerance to trigger
        // the waypoint has been reached. The tolerance is added since floats are never always exact when comparing,
        // if the correct amount of tolerance is not used then the AI will never stop at the waypoint.
        if (Mathf.Abs(Vector2.Distance(waypoint.waypointPosition, currentPos)) < waypointReachedTolerance)
        {
            waypoint.WaypointReached = true;
            
            // If the waypoint had a delay, then stop the AI and trigger the delayed state so it will tick the
            // given delay time.
            if (waypoint.delay > 0f)
            {
                _rigidBody2D.velocity = Vector2.zero;
                _waypointPathDelayed = true;
                _waypointDelayTimer = waypoint.delay;
            }
        }
        
        // Update AI movement if the waypoint has not been reached..
        if (!waypoint.WaypointReached)
        {
            FlipSprite();
            FlipHealthUI();

            float currentSpeed = waypoint.shouldRun ? runSpeed : walkSpeed;

            UpdateWalkRunSfxPitch(currentSpeed);
            
            _rigidBody2D.velocity =
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

    /// <summary>
    /// AI chase movement update.
    /// Only used during combat engagement and the target is within sight detection but not within combat reach.
    /// </summary>
    private void ChaseMovementFixedUpdate()
    {
        if (!AllowChaseMovement())
            return;

        FlipSprite();
        FlipHealthUI();
        
        Vector2 currentPos = _rigidBody2D.position;

        UpdateWalkRunSfxPitch(runSpeed);
        
        _rigidBody2D.velocity =
            new Vector2((_enemyCombat.Target.transform.position.x < currentPos.x ? Vector2.left.x : Vector2.right.x)
                        * runSpeed, 0f) * Time.deltaTime;
    }


    /// <summary>
    /// AI advance on target movement update.
    /// Only used during combat engagement and at ranged attack distance, the AI will slowly advance the target between
    /// each ranged attack while the ranged attack timer ticks.
    /// </summary>
    private void AdvanceOnTargetMovementFixedUpdate()
    {
        if (!AllowAdvanceMovement())
            return;

        FlipSprite();
        FlipHealthUI();
        
        Vector2 currentPos = _rigidBody2D.position;
        Vector2 destPos = GetMidPoint(_enemyCombat.Target);

        UpdateWalkRunSfxPitch(walkSpeed);
        
        _rigidBody2D.velocity =
            new Vector2((destPos.x < currentPos.x ? Vector2.left.x : Vector2.right.x)
                        * walkSpeed, 0f) * Time.deltaTime;
    }
    
    /// <summary>
    /// AI investigate movement update.
    /// Only used when combat has been initiated by an invoker from behind and not within sight detection
    /// when AI turns around.
    /// </summary>
    private void InvestigateMovementFixedUpdate()
    {
        if (!AllowInvestigateMovement())
            return;

        FlipSprite();
        FlipHealthUI();
        
        Vector2 currentPos = _rigidBody2D.position;
        Vector2 destPos = _enemyCombat.InvestigateDestPos;

        UpdateWalkRunSfxPitch(walkSpeed);
        
        _rigidBody2D.velocity =
            new Vector2((destPos.x < currentPos.x ? Vector2.left.x : Vector2.right.x)
                        * walkSpeed, 0f) * Time.deltaTime;
    }
    
    #endregion

    #region Update Methods
    
    /// <summary>
    /// Update the timer for a waypoint delay that was triggered.
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
    /// Update the timer for a random movement position delay that was triggered.
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
    /// Update check to determine if AI is on ground or in-air/falling.
    /// </summary>
    private void GroundCheckUpdate()
    {
        _animator.SetBool("IsGrounded", IsGrounded());
    }

    /// <summary>
    /// Update walk/run sound effect to either play or pause depending on conditions.
    /// </summary>
    private void UpdateWalkRunSfx()
    {
        if (ShouldPlayWalkRunSFX && _rigidBody2D.velocity.x != 0f && IsGrounded() && !movementAudioSource.isPlaying)
            movementAudioSource.Play();
        else if (!ShouldPlayWalkRunSFX || _rigidBody2D.velocity.x == 0f || !IsGrounded() && movementAudioSource.isPlaying)
            movementAudioSource.Pause();
    }
    
    #endregion
    
    #region Public Helper Methods
    
    /// <summary>
    /// Checks if the enemy is grounded
    /// </summary>
    /// <returns>Returns true if enemy is grounded, otherwise false.</returns>
    public bool IsGrounded()
    {
        return _rigidBody2D.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }
    
    /// <summary>
    /// Checks if the enemy is jumping (positive velocity on Y axis)
    /// </summary>
    /// <returns>Returns true if player is jumping, otherwise false.</returns>
    public bool IsJumping()
    {
        return _rigidBody2D.velocity.y > 0;
    }

    /// <summary>
    /// Checks if the enemy is falling (negative velocity on Y axis)
    /// </summary>
    /// <returns>Returns true if player is falling, otherwise false.</returns>
    public bool IsFalling()
    {
        return _rigidBody2D.velocity.y < 0;
    }
    
    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// Flip sprite based on current facing direction compared to the rigidbody's velocity if needed.
    /// </summary>
    private void FlipSprite()
    {
        bool isFacingLeft = transform.localScale.x < 0;
        if (_rigidBody2D.velocity.x > 0 && !isFacingLeft || _rigidBody2D.velocity.x < 0 && isFacingLeft ||
            _rigidBody2D.velocity.x == 0)
            return;

        Vector3 enemyScale = transform.localScale;
        transform.localScale = new Vector3(-enemyScale.x, enemyScale.y, enemyScale.z);
    }

    /// <summary>
    /// Flip enemy health bar UI in relation to the enemy's current X-axis value of its local scale.
    /// </summary>
    private void FlipHealthUI()
    {
        if (!_health)
            return;
        
        _health.enemyHealthImage.fillOrigin = transform.localScale.x switch
        {
            > 0  => (int)Image.OriginHorizontal.Left,
            < 0  => (int)Image.OriginHorizontal.Right,
            _ => (int)Image.OriginHorizontal.Left
        };
    }
    
    /// <summary>
    /// Determine pitch of movement sound effect based on current speed.
    /// <param name="currentSpeed">The gameobject's current speed</param>
    /// </summary>
    private void UpdateWalkRunSfxPitch(float currentSpeed)
    {
        if (movementAudioSource.pitch != normalWalkPitch && currentSpeed == walkSpeed)
            movementAudioSource.pitch = normalWalkPitch;
        else if (movementAudioSource.pitch != fastRunPitch && currentSpeed == runSpeed)
            movementAudioSource.pitch = fastRunPitch;
    }

    /// <summary>
    /// Checks whether the AI is allowed to move on ground or not.<br></br><br></br>
    /// Specifically, check if the AI is grounded and not jumping/falling.
    /// </summary>
    /// <returns>Returns true if the AI is allowed to move on ground, otherwise false.</returns>
    private bool AllowGroundMovement()
    {
        return IsGrounded() && !IsJumping() && !IsFalling();
    }
    
    /// <summary>
    /// Checks whether the AI can proceed with waypoint movement or not.
    /// </summary>
    /// <returns>Returns true if AI can continue waypoint movement, otherwise false.</returns>
    private bool AllowWaypointMovement()
    {
        return applyWaypointMovement && waypoints.Length != 0 && !_waypointPathDelayed &&
               (_currentWpId != waypoints.Length || loopWaypointPath) && !_enemyCombat.ChaseTarget &&
               !_enemyCombat.InCombat && !_enemyCombat.InvestigateEngagement && AllowGroundMovement();
    }

    /// <summary>
    /// Checks whether the AI can proceed with random movement or not.
    /// </summary>
    /// <returns>Returns true if AI can continue random movement, otherwise false.</returns>
    private bool AllowRandomMovement()
    {
        return applyRandomMovement && !_randomMoveDelayed && !_enemyCombat.ChaseTarget && !_enemyCombat.InCombat &&
               !_enemyCombat.InvestigateEngagement && AllowGroundMovement();
    }

    /// <summary>
    /// Checks whether the AI can proceed with chase movement or not.
    /// </summary>
    /// <returns>Returns true if AI can continue chase movement, otherwise false.</returns>
    private bool AllowChaseMovement()
    {
        return _enemyCombat.ChaseTarget && !_enemyCombat.InCombat && AllowGroundMovement();
    }

    /// <summary>
    /// Checks whether the AI can proceed with advance on target movement or not.
    /// </summary>
    /// <returns>Returns true if AI can continue advance on target movement, otherwise false.</returns>
    private bool AllowAdvanceMovement()
    {
        return _enemyCombat.AdvanceTarget && AllowGroundMovement();
    }

    /// <summary>
    /// Checks whether the AI can proceed with investigate movement or not.
    /// </summary>
    /// <returns>Returns true if AI can continue investigate movement, otherwise false.</returns>
    private bool AllowInvestigateMovement()
    {
        return _enemyCombat.InvestigateEngagement && !_enemyCombat.Target && AllowGroundMovement();
    }
    
    /// <summary>
    /// Reset all waypoints to be marked as not reached, used primarily when looping the waypoint path
    /// </summary>
    private void ResetWaypoints()
    {
        foreach (WaypointInfo wp in waypoints)
            wp.WaypointReached = false;
    }
    
    /// <summary>
    /// Retrieve a midpoint between this AI gameobject and the target gameobject passed as parameter.
    /// </summary>
    /// <param name="target">The target gameobject that will be used for its current position.</param>
    /// <returns>Returns the midpoint between this AI gameobject and the target gameobject.</returns>
    private Vector2 GetMidPoint(GameObject target)
    {
        Vector2 myPos = transform.position;
        Vector2 targetPos = target.transform.position;

        return new Vector2((myPos.x + targetPos.x) / 2, (myPos.y + targetPos.y) / 2);
    }
    
    #endregion
}