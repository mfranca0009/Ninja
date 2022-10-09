using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapCamera : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject secCamera;
    public GameObject player;
    public float yLevel = 0.0f;
    public float xLevel = 150.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera.activeInHierarchy == false)
        {
            if (player.transform.position.y >= yLevel && player.transform.position.x <= xLevel)
            {
                mainCamera.SetActive(true);
                secCamera.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        if (collision.gameObject.tag == "Player")
        {
            secCamera.SetActive(true);
            mainCamera.SetActive(false);
        }
    }
}
