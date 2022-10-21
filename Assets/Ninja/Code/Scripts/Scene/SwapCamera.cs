using UnityEngine;

public class SwapCamera : MonoBehaviour
{
    #region Public Fields

    [Header("Swap Camera Settings")]
    
    [Tooltip("The transitioning speed when swapping cameras")]
    public float cameraTransitioningSpeed = 8f;
    
    [Tooltip("The main camera's camera component")]
    public Camera mainCamera;
    
    [Tooltip("The secondary camera's camera component")]
    public Camera secondaryCamera;
    
    [Tooltip("The player's transform within the current scene")]
    public Transform playerTransform;

    [Tooltip("The y-axis level to trigger swapping of cameras")]
    public float yLevel;
    
    [Tooltip("The x-axis level to trigger swapping of cameras")]
    public float xLevel = 150.0f;
    
    #endregion

    [Tooltip("Tolerance level to know when to swap camera control")]
    [SerializeField] public float cameraSwapTolerance = 0.25f;

    #region Private Fields
    
    // Positions
    private Vector3 _mainCameraPosition;
    private Vector3 _secondaryCameraPosition;
    private Vector3 _secretCameraLocation;

    #endregion

    #region Unity Events
    
    // Start is called before the first frame update
    private void Start()
    {
        _secretCameraLocation = secondaryCamera.transform.position;
    }

    private void Update()
    {
        _mainCameraPosition = mainCamera.transform.position;
        _secondaryCameraPosition = secondaryCamera.transform.position;
        
        //If arena cam is active, move it to be centered in the arena. 
        if (secondaryCamera.isActiveAndEnabled && playerTransform.position.y < yLevel)
        {
            secondaryCamera.transform.position = Vector3.MoveTowards(_secondaryCameraPosition, _secretCameraLocation,
                cameraTransitioningSpeed * Time.deltaTime);
        }

        //If main camera is not active, return control to main camera.
        if (mainCamera.isActiveAndEnabled || !(playerTransform.transform.position.y >= yLevel) ||
            !(playerTransform.transform.position.x <= xLevel))
            return;

        ReturnCameraToMain();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || secondaryCamera.isActiveAndEnabled)
            return;

        secondaryCamera.transform.position = mainCamera.transform.position;
        mainCamera.enabled = false;
        secondaryCamera.enabled = true;
    }
    
    #endregion

    #region Private Helper Function
    
    /// <summary>
    /// Sets main camera's position to the same as the secondary camera, then relinquishes control back to main camera.
    /// </summary>
    private void ReturnCameraToMain()
    {
        secondaryCamera.transform.position = Vector3.MoveTowards(_secondaryCameraPosition, _mainCameraPosition,
            cameraTransitioningSpeed * Time.deltaTime);

        //Using distance formula to determine if cameras are close enough to each other to swap.
        float cameraDistance = Vector2.Distance(_secondaryCameraPosition, _mainCameraPosition);
        if (_secondaryCameraPosition != _mainCameraPosition && cameraDistance >= cameraSwapTolerance)
            return;

        mainCamera.enabled = true;
        secondaryCamera.enabled = false;
    }

    #endregion
}
