using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damagePlayer_Trap : MonoBehaviour
{
    //Presure Plate - Used to prevent excess hits on an entity when hit.
    [Tooltip("The object that triggers the trap")]
    public GameObject presurePlate;
    //The Swinging Trap Component of the presure plate
    private SwingingTrap sTrap;

    [Tooltip("The ammount of damage to be dealt to the affected enemy.")]
    public float damage = 20.0f;

    [Tooltip("The ammount of knockback provided to the entity colliding with the swinging trap.")]
    public Vector2 knockback = Vector2.one;

    //Varriable to store the GameObject hit by the trap.
    private GameObject foreignEntity;

    //Determines if the trap has hit anyone
    private bool hasHit = false;

    void Start()
    {
        //set sTrap equal to the SwingingTrap component on the presure plate
        sTrap = presurePlate.GetComponent<SwingingTrap>();    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If the trap hasn't hit anyone and has been activated
        if (!hasHit && sTrap.ShouldRotate())
        {
            //and the object hit is either the player or an enemy.
            if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Enemy")
            {
                //then set the foreignEntity to whichever was hit, damge them, and knock them back.
                foreignEntity = collision.gameObject;
                foreignEntity.GetComponent<Health>().DealDamage(damage, this.gameObject);

                /***PROBLEM: If the player is holding a direction when hit, they ignore the knockback. ***/

                // Failed attempt to prevent movement -> foreignEntity.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                foreignEntity.GetComponent<Rigidbody2D>().AddRelativeForce(knockback, ForceMode2D.Impulse);
                
                //Finally, set hasHit to true so that it doesn't activate additional times.
                hasHit = true;
            }
        }
    }
}
