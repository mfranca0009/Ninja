using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("The camera that will follow the target")]
    [SerializeField] private new Camera camera;

    [Tooltip("Enable camera smoothing")]
    [SerializeField] private bool applySmoothing = true;
    
    [Tooltip("Enable X-axis lock\nCamera will only follow the X-axis.")]
    [SerializeField] private bool applyXAxisLock;
    
    [Tooltip("Vertical offset from the target")]
    [SerializeField] private float verticalOffset = 1.5f;
    
    [Tooltip("Horizontal offset from the target")]
    [SerializeField] private float horizontalOffset = 2f;

    [Tooltip("Smoothing time for camera smoothing")]
    [SerializeField] private float smoothTime = 0.4f;
    
    [Tooltip("Max speed the camera is allowed to accelerate up to during smoothing")]
    [SerializeField] private float maxSmoothSpeed = 50f;
    #endregion
    
    #region Private Fields
    private Rigidbody2D _rigidbody;
    private UpdateMode _updateMode;
    private Vector3 _currentPlayerPosition;
    private Vector3 _currentCameraPosition;
    private Vector3 _smoothVelocity;
    private bool _positionChanged;
    #endregion
    
    #region Unity Events
    private void Start()
    {
        _smoothVelocity = Vector2.zero;
        _updateMode = applySmoothing ? UpdateMode.FixedUpdate : UpdateMode.Update;
        _currentPlayerPosition = transform.position;
        camera.transform.position = _currentCameraPosition = new Vector3(_currentPlayerPosition.x + horizontalOffset,
            _currentPlayerPosition.y + verticalOffset, camera.transform.position.z);
    }
    
    // Update is called once per frame
    private void Update()
    {
        CheckPositionUpdate();
        
        if (_updateMode == UpdateMode.Update)
            CameraUpdate();
    }

    private void FixedUpdate()
    {
        if (_updateMode == UpdateMode.FixedUpdate)
            CameraUpdate();
    }
    #endregion

    #region Update Methods
    /// <summary>
    /// Update target position and determine if camera position is off.
    /// </summary>
    private void CheckPositionUpdate()
    {
        _currentPlayerPosition = transform.position;
        
        if (_currentPlayerPosition == _currentCameraPosition)
            return;

        _positionChanged = true;
    }
    #endregion

    #region Variable Update Methods
    /// <summary>
    /// Update camera position to follow target.
    /// </summary>
    private void CameraUpdate()
    {
        if (!_positionChanged)
            return;
        
        // Default target position to be used before assessing features enabled.
        Vector3 targetPosition = new Vector3(_currentPlayerPosition.x + horizontalOffset,
            _currentPlayerPosition.y + verticalOffset,
            _currentCameraPosition.z);

        switch (applySmoothing)
        {
            case true when applyXAxisLock:
            case true when !applyXAxisLock:
            {
                Vector3 xAxisLockPosition = new Vector3(_currentPlayerPosition.x + horizontalOffset,
                    _currentCameraPosition.y, _currentCameraPosition.z);
                
                camera.transform.position = _currentCameraPosition = Vector3.SmoothDamp(_currentCameraPosition,
                    applyXAxisLock ? xAxisLockPosition : targetPosition,
                    ref _smoothVelocity, smoothTime, maxSmoothSpeed);
                break;   
            }
            case false when applyXAxisLock:
            {
                targetPosition = new Vector3(_currentPlayerPosition.x + horizontalOffset, _currentCameraPosition.y,
                    _currentCameraPosition.z);
                
                camera.transform.position = _currentCameraPosition = targetPosition;
                break;   
            }
            default:
            {
                camera.transform.position = _currentCameraPosition = targetPosition;
                break;   
            }
        }

        _positionChanged = false;
    }
    #endregion
}