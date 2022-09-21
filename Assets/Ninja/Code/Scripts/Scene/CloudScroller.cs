using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudScroller : MonoBehaviour
{

    [Tooltip("This value deternmines how fast the object scrolls accross the screen. Negitive values moves it right.")]
    public float Speed = 0.0f;

    [Tooltip("Player is the object the clouds should be \"orbiting\"")]
    public GameObject player;

    private float step = 0.0f;
    private Vector3 targetPoint; 

    // Start is called before the first frame update
    void Start()
    {
        step = Speed * Time.deltaTime;
        targetPoint = new Vector3(-99, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, step);

        //If off the left side of the screen, move cloud to off the right side of the screen. 
        if(transform.position.x < player.transform.position.x - 15)
        {
            transform.position = new Vector3(
                player.transform.position.x + 15, 
                transform.position.y, 
                transform.position.z);
        }
        else if (transform.position.x > player.transform.position.x + 16)
        {
            transform.position = new Vector3(
                player.transform.position.x - 14,
                transform.position.y,
                transform.position.z);
        }
    }
}
