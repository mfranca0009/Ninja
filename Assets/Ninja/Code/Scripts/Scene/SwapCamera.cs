using UnityEngine;

public class SwapCamera : MonoBehaviour
{
    public Camera mainCamera;
    public Camera secCamera;
    public GameObject player;
    public float yLevel = 0.0f;
    public float xLevel = 150.0f;

    [Tooltip("Tolerance level to know when to swap camera control")]
    [SerializeField] public float cameraSwapTolerence = 0.25f;

    private Vector3 secretCameraLocation;
    //The Speed at which the camera moves into position.
    private float cameraMoveSpeed;

    // Start is called before the first frame update
    private void Start()
    {
        secretCameraLocation = secCamera.transform.position;

    }

    void Update()
    {
        cameraMoveSpeed = 8 * Time.deltaTime;

        //If arena cam is active, move it to be centered in the arena. 
        if (secCamera.isActiveAndEnabled && player.transform.position.y < yLevel)
        {
            secCamera.transform.position = Vector3.MoveTowards(secCamera.transform.position, secretCameraLocation, cameraMoveSpeed);
        }

        //If maincamera is not active, return control to main cam.
        if (mainCamera.gameObject.activeInHierarchy || !(player.transform.position.y >= yLevel) ||
            !(player.transform.position.x <= xLevel))
            return;

        ReturnCameraToMain();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || secCamera.isActiveAndEnabled)
            return;

        secCamera.transform.position = mainCamera.transform.position;
        mainCamera.gameObject.SetActive(false);
        secCamera.gameObject.SetActive(true);
    }

    #region Private Helper Function
    /// <summary>
    /// Set's main camera's position to the same as the arena cam, then relinquishes control back to main cam.
    /// </summary>
    private void ReturnCameraToMain()
    {
        secCamera.transform.position = Vector3.MoveTowards(secCamera.transform.position, mainCamera.transform.position, cameraMoveSpeed);

        //Using distance formula to determine if cameras are close enough to each other to swap.

        if (secCamera.transform.position != mainCamera.transform.position)
        {
            if (Vector2.Distance(secCamera.transform.position, mainCamera.transform.position) > cameraSwapTolerence)
                return;
        }
        mainCamera.gameObject.SetActive(true);
         secCamera.gameObject.SetActive(false);
    }

    #endregion
}
