using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartPlane : MonoBehaviour
{
    public int damageDealt = 0;
    public Vector3 respawnLocation= new Vector3(0, 0, 0);
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.transform.position = respawnLocation;
        }

        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.GetComponent<Health>().InstaKill(true);
            collision.gameObject.SetActive(false);
        }
    }
}
