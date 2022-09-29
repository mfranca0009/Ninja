using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damagePlayer_Trap : MonoBehaviour
{

    [Tooltip("The object that triggers the trap")]
    public GameObject presurePlate;

    [Tooltip("The ammount of damage to be dealt to the affected enemy.")]
    public float damage = 20.0f;

    [Tooltip("The ammount of knockback provided to the entity colliding with the swinging trap.")]
    public Vector2 knockback = Vector2.one;
    
    private SwingingTrap sTrap;
    private GameObject foreignEntity = null;
    private bool hasHit = false;
    private bool wait = false;
    private int frameDelay = 10;
    private int frameCount = 0;

    void Start()
    {
        //set sTrap equal to the SwingingTrap component on the presure plate
        sTrap = presurePlate.GetComponent<SwingingTrap>();    
    }

    private void Update()
    {
        //If the trap has been triggered, toss the player.
        if (foreignEntity != null)
        {
            if (wait)
            {
                foreignEntity.GetComponent<Rigidbody2D>().AddRelativeForce(knockback, ForceMode2D.Impulse);
                
            }
            else
            {
                wait = true;
            }
        }

        //If trap has been triggered, wait so many frames before allowing player to move again.
        if (wait)
        {
            if (frameCount >= frameDelay)
            {
                foreignEntity.GetComponent<PlayerMovement>().SetIncomingKnockBackEffect(false);
                foreignEntity = null;
                wait = false;
            }
            else
            {
                frameCount++;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If the trap hasn't hit anyone and has been activated
        if (!hasHit && sTrap.ShouldRotate())
        {
            //and the object hit is either the player or an enemy.
            if (collision.gameObject.tag == "Player") // || collision.gameObject.tag == "Enemy")
            {
                //then set the foreignEntity to whichever was hit, damge them, and knock them back.
                foreignEntity = collision.gameObject;
                foreignEntity.GetComponent<Health>().DealDamage(damage, this.gameObject);
                foreignEntity.GetComponent<PlayerMovement>().SetIncomingKnockBackEffect(true);


                //Finally, set hasHit to true so that it doesn't activate additional times.
                hasHit = true;
            }
        }
    }
}
