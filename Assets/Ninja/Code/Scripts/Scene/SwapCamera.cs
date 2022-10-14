using UnityEngine;

public class SwapCamera : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject secCamera;
    public GameObject player;
    public float yLevel = 0.0f;
    public float xLevel = 150.0f;

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (mainCamera.activeInHierarchy || !(player.transform.position.y >= yLevel) ||
            !(player.transform.position.x <= xLevel))
            return;

        mainCamera.SetActive(true);
        secCamera.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;
        
        secCamera.SetActive(true);
        mainCamera.SetActive(false);
    }
}
