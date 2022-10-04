using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingTrap_MiniBoss : MonoBehaviour
{
    #region Varriables
    [Tooltip("When this entity is dead, stop the trap")]
    public GameObject miniboss;
    private Health _minibossHealth;

    [Tooltip("The blade child of the trap")]
    public GameObject blade;

    [Tooltip("How fast the blade rotates")]
    public float bladeRotationSpeed = 360.0f;
    
    [Tooltip("The ammount of damage to be dealt to the affected enemy.")]
    public float damage = 20.0f;
    
    [Tooltip("The ammount of knockback provided to the entity colliding with the swinging trap.")]
    public Vector2 knockback = Vector2.one;
    
    [Tooltip("How many Seconds to wait before reenabling player movement.")]
    [SerializeField] private float timeDelay = 1.0f;
    private float timer = 0f;
    
    
    [Tooltip("Rotation speed of the trap as a whole")]
    [SerializeField] private float trapRotationSpeed = -10.0f;
    
    //What got hit?
    private GameObject foreignEntity = null;
    
    //How long to delay before the player can be hit again
    private bool delayHit;

    //A 1 frame delay to ensure the player isn't moving before dealing the Knockback
    private bool wait;

    private bool doKnockback = true;

    //object's renderer for post fight stopping of trap
    private Renderer _renderer;
    #endregion

    void Start()
    {
        _minibossHealth = miniboss.GetComponent<Health>();
        _renderer = blade.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_minibossHealth.Dead && !_renderer.isVisible)
        {
            gameObject.SetActive(false);
        }

        //Rotate Trap and Blade
        transform.Rotate(0.0f, 0.0f, trapRotationSpeed * Time.deltaTime);
        blade.transform.Rotate(0.0f, 0.0f, bladeRotationSpeed);

        //If no foreignEntity, don't do anything
        if (!foreignEntity)
        {
            return;
        }
        //Debug.Log($"Wait: {wait};   doKnockback: {doKnockback};  franeCount: {frameCount}");
        //Otherwise, wait one frame, then apply knockback
        if (wait)
        {
            //Do knockback onces
            if (doKnockback)
            {
                foreignEntity.GetComponent<Rigidbody2D>().AddRelativeForce(knockback, ForceMode2D.Impulse);
                doKnockback = false;
            }


            //Reset the trap after enough time has passed, otherwise do nothing.
            timer += Time.deltaTime;
            if (timer < timeDelay)
            {
                return;
            }
            foreignEntity.GetComponent<PlayerMovement>().IncomingKnockback = false;
            foreignEntity = null;
            wait = false;
            timer = 0;
            delayHit = false;
            doKnockback = true;
        }
        else
        {
            wait = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If the trap hasn't hit anyone and has been activated
        if (delayHit || collision.gameObject.tag != "Player")
        {
            return;
        }
        
        //then set the foreignEntity to who got hit, deal them damage, and prep them for knockback.
        foreignEntity = collision.gameObject;
        foreignEntity.GetComponent<Health>().DealDamage(damage, gameObject);
        foreignEntity.GetComponent<PlayerMovement>().IncomingKnockback = true;

        //Finally, set hasHit to true so that it doesn't activate additional times.
        delayHit = true;
    }
}
