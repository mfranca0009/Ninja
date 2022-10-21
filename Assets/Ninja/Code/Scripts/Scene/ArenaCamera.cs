using UnityEngine;

/// <summary>
/// The Script is placed on the trigger to control the swaping of camera's for a boss fight.
/// </summary>
public class ArenaCamera : MonoBehaviour
{
    #region Serialized Fields

    [Tooltip("The invisible walls that prevent player from going out of frame.")]
    [SerializeField]public GameObject arenaBoundaries;
   
    [Tooltip("The camera set to be centered over boss arena.")]
    [SerializeField]public Camera arenaCamera;
   
    [Tooltip("The main camera")]
    [SerializeField]public Camera mainCamera;
   
    [Tooltip("The miniboss for the level")]
    [SerializeField]public GameObject miniBoss;

    [Tooltip("Tolerance level to know when to swap camera control")]
    [SerializeField] public float cameraSwapTolerence = 0.25f;

    #endregion

    #region Private Fields

    //Miniboss's health component
    private Health _miniBossHealth;

    //The location for the arenaCamera to be during the fight/when initialized
    private Vector3 arenaCameraLockPosition;

    //The Speed at which the camera moves into position.
    private float cameraMoveSpeed;

    #endregion

    #region Unity Functions

    // Start is called before the first frame update
    void Start()
    {
        _miniBossHealth = miniBoss.GetComponent<Health>();
        arenaCameraLockPosition = arenaCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        cameraMoveSpeed = 8 * Time.deltaTime;

        //If arena cam is active, move it to be centered in the arena. 
        if (arenaCamera.isActiveAndEnabled && !_miniBossHealth.Dead)
        {
            arenaCamera.transform.position = Vector3.MoveTowards(arenaCamera.transform.position, arenaCameraLockPosition, cameraMoveSpeed);
        }

        //When boss dies, deactivate arena boundries
        if (!_miniBossHealth.Dead)
            return;

        arenaBoundaries.SetActive(false);

        //If maincamera is not active, return control to main cam.
        if (!arenaCamera.isActiveAndEnabled)
            return;

        ReturnCameraToMain();
        
    }

    /// <summary>
    ///When player hits the trigger, if the boss isn't dead, active the arena cam and bandaries, and deactive the main cam. 
    /// </summary>
    /// <param name="collision"></param>

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _miniBossHealth.Dead || arenaCamera.isActiveAndEnabled)
            return;

        arenaCamera.transform.position = mainCamera.transform.position;
        mainCamera.gameObject.SetActive(false);
        arenaCamera.gameObject.SetActive(true);
        arenaBoundaries.SetActive(true);
    }

    #endregion

    #region Private Helper Function
    /// <summary>
    /// Set's main camera's position to the same as the arena cam, then relinquishes control back to main cam.
    /// </summary>
    private void ReturnCameraToMain()
    {
        arenaCamera.transform.position = Vector3.MoveTowards(arenaCamera.transform.position, mainCamera.transform.position, cameraMoveSpeed);

        //Using distance formula to determine if cameras are close enough to each other to swap.
        
        if (arenaCamera.transform.position != mainCamera.transform.position)
        {
            if (Vector2.Distance(arenaCamera.transform.position, mainCamera.transform.position) > cameraSwapTolerence)
                return;
        }
        mainCamera.gameObject.SetActive(true);
        arenaCamera.gameObject.SetActive(false);
    }

    #endregion
}
