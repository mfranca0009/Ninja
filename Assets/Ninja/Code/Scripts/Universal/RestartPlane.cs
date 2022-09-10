using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartPlane : MonoBehaviour
{
    public int damageDealt = 0;
    public Vector3 respawnLocation= new Vector3(0, 0, 0);
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.transform.position = respawnLocation;
        }
    }
}
