using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer_Trap : MonoBehaviour
{

    [Tooltip("The object that triggers the trap")]
    public GameObject presurePlate;

    [Tooltip("The ammount of damage to be dealt to the affected enemy.")]
    public float damage = 20.0f;

    [Tooltip("The ammount of knockback provided to the entity colliding with the swinging trap.")]
    public Vector2 knockback = Vector2.one;
    
    [Tooltip("How many frames to wait before reenabling player movement.")]
    [SerializeField]private int frameDelay = 10;
    
    [Tooltip("Trap hit sound effect")] 
    [SerializeField] private AudioClip trapHitSoundClip;
    
    private int frameCount = 0;
    private SwingingTrap sTrap;
    private GameObject foreignEntity = null;
    private bool hasHit;
    private bool wait;
    private bool trapTriggered;
    
    // Sound Effects / Music
    private SoundManager _soundManager;
    
    

    void Start()
    {
        //set sTrap equal to the SwingingTrap component on the presure plate
        sTrap = presurePlate.GetComponent<SwingingTrap>();
        _soundManager = FindObjectOfType<SoundManager>();
    }

    private void Update()
    {
        if (!foreignEntity)
        {
            return;
        }

        if (wait && !trapTriggered)
        {
            foreignEntity.GetComponent<Rigidbody2D>().AddRelativeForce(knockback, ForceMode2D.Impulse);
            trapTriggered = true;
        }
        else
        {
            wait = true;
        }

        if (frameCount >= frameDelay)
        {
            foreignEntity.GetComponent<PlayerMovement>().IncomingKnockback = false;
            foreignEntity = null;
            wait = false;
        }
        else
        {
            frameCount++;
            return;
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
                
                // Trap hit sound effect
                if (_soundManager)
                    _soundManager.PlaySoundEffect(AudioSourceType.DamageEffects, trapHitSoundClip);
                
                //then set the foreignEntity to whichever was hit, damge them, and knock them back.
                foreignEntity = collision.gameObject;
                foreignEntity.GetComponent<Health>().DealDamage(damage, this.gameObject);
                foreignEntity.GetComponent<PlayerMovement>().IncomingKnockback = true;


                //Finally, set hasHit to true so that it doesn't activate additional times.
                hasHit = true;
            }
        }
    }
}
