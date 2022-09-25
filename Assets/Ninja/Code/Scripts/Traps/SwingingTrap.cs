using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingTrap : MonoBehaviour
{
    public GameObject trap;

    private bool shouldRotate = false;
    private bool hasActivated = false;
    private float step = -4.0f;

    // Update is called once per frame
    void Update()
    {
        if (shouldRotate)
        {
            trap.transform.Rotate(0.0f, 0.0f, step * Time.deltaTime);
            step -= 1.0f;
            
            if (trap.transform.rotation.z <= -0.92f) //Bassed on the z value of the rotation Quaterion
            {
                shouldRotate = false;
            }
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && hasActivated == false)
        {
            shouldRotate = true;
            hasActivated = true;
        }
    }
}
