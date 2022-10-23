using UnityEngine;

/// <summary>
/// The Script is placed on the trigger to control the swapping of camera's for a boss fight.
/// </summary>
public class ArenaCamera : MonoBehaviour
{
    #region Serialized Fields

    [Header("Arena Camera Settings")]
    
    [Tooltip("The camera set to be centered over boss arena.")]
    [SerializeField] private Camera arenaCameraObject;
   
    [Tooltip("The main camera")]
    [SerializeField] private Camera mainCameraObject;
    
    [Tooltip("The camera's transitioning speed when switching cameras")]
    [SerializeField] private float cameraTransitioningSpeed = 8f;
    
    [Tooltip("Tolerance level to know when to swap camera control")]
    [SerializeField] private float cameraSwapTolerance = 0.25f;

    [Tooltip("The invisible walls that prevent player from going out of frame.")]
    [SerializeField] private GameObject arenaBoundaries;

    [Tooltip("The mini-boss for the level")]
    [SerializeField] private GameObject miniBoss;

    [Tooltip("Mini-boss background music audio clip which will play during mini-boss arena engagement")]
    [SerializeField] private AudioClip miniBossBgMusic;
    
    #endregion

    #region Private Fields

    // Positions
    private Vector3 _arenaCameraPosition;
    private Vector3 _mainCameraPosition;
    private Vector3 _arenaCameraLockPosition;

    // Cameras
    private Camera _mainCamera;
    private Camera _arenaCamera;
    
    // Managers
    private SoundManager _soundManager;
    
    // Scripts
    private Health _playerHealth;
    private Health _miniBossHealth;
    private MenuSounds _menuSounds;

    #endregion

    #region Unity Events

    // Start is called before the first frame update
    private void Start()
    {
        _soundManager = FindObjectOfType<SoundManager>();
        _menuSounds = mainCameraObject.GetComponent<MenuSounds>();
        
        _miniBossHealth = miniBoss.GetComponent<Health>();
        _playerHealth = GameObject.FindWithTag("Player").GetComponent<Health>();
        _mainCamera = mainCameraObject.GetComponent<Camera>();
        _arenaCamera = arenaCameraObject.GetComponent<Camera>();
        _arenaCameraLockPosition = arenaCameraObject.transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        _arenaCameraPosition = arenaCameraObject.transform.position;
        _mainCameraPosition = mainCameraObject.transform.position;
        
        //If arena camera is active, move it to be centered in the arena.
        if (_arenaCamera.isActiveAndEnabled && !_miniBossHealth.Dead)
        {
            arenaCameraObject.transform.position = Vector3.MoveTowards(_arenaCameraPosition,
                _arenaCameraLockPosition, cameraTransitioningSpeed * Time.deltaTime);
        }
        
        ArenaCleanUp();
    }

    /// <summary>
    ///When player hits the trigger, if the boss isn't dead, active the arena camera and boundaries,
    /// and deactivate the main camera.
    /// </summary>
    /// <param name="collision">The colliding object's collider 2D.</param>

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _miniBossHealth.Dead || _arenaCamera.isActiveAndEnabled)
            return;

        arenaCameraObject.transform.position = mainCameraObject.transform.position;
        _mainCamera.enabled = false;
        _arenaCamera.enabled = true;
        arenaBoundaries.SetActive(true);
        _soundManager.PlayMusic(miniBossBgMusic);
    }

    #endregion

    #region Private Helper Function
    
    /// <summary>
    /// Perform arena clean-up if the mini-boss dies or the player dies.<br></br><br></br>
    /// Arena clean-up involves swapping to main camera, disabling arena boundaries, and transitioning music.
    /// </summary>
    private void ArenaCleanUp()
    {
        if (!_arenaCamera.isActiveAndEnabled || (!_miniBossHealth.Dead && !_playerHealth.Dead))
            return;

        arenaCameraObject.transform.position = Vector3.MoveTowards(_arenaCameraPosition,
            _mainCameraPosition, cameraTransitioningSpeed * Time.deltaTime);

        // Using distance formula to determine if both cameras are close enough to each other to swap.
        float cameraDistance = Vector2.Distance(_arenaCameraPosition, _mainCameraPosition);
        if (_arenaCameraPosition != _mainCameraPosition && cameraDistance >= cameraSwapTolerance && !_playerHealth.Dead)
            return;

        _mainCamera.enabled = true;
        _arenaCamera.enabled = false;
        arenaBoundaries.SetActive(false);
        _soundManager.PlayMusic(_menuSounds.bgMusic);
    }

    #endregion
}
