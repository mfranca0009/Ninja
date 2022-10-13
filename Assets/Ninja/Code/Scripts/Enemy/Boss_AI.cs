using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.UI;

public class Boss_AI : MonoBehaviour
{
    //Teleport Logic
    [Tooltip("An array of waypoints that can be teleported to when hit")]
    [SerializeField] public TeleportWaypoint[] waypoints;
    private int teleportLocation;
    private int previousLocation;
    [SerializeField] private int timesHitBeforeTeleporting = 3;
    private int hitCounter = 0;

     //ShadowClone Logic 
    [Tooltip("The particle effect used in a burst when the Enemy is hit and teleports")]
    public ParticleSystem smokeBombParticle;
    [Tooltip("What minion prefab to be summoned")]
    public GameObject shadowClone;
    private Transform cloneSummonLocation;
    
    //Scroll
    [Tooltip("The scroll needed to beat the game.")]
    public GameObject scroll;
    
    //Health Logic
    private Health _healthComponent;
    private float previousHealth;

    //flip logic
    Vector3 curLocalScale;



    // Start is called before the first frame update
    void Start()
    {
        _healthComponent = GetComponent<Health>();
        previousHealth = _healthComponent.HealthPoints;
        curLocalScale = transform.localScale;
        Teleport(0);
        gameObject.GetComponent<EnemyCombat>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        DamageCheck();
        ScrollCheck();
    }

    //Check if you've been damaged
    private void DamageCheck()
    {
        //Return if there is no place to teleport to, the char is dead, or they haven't taken damage.
        if (waypoints.Length <= 1 || _healthComponent.Dead || _healthComponent.HealthPoints >= previousHealth)
        {
            return;
        }

        //increment the hit counter, set previous health to current health,
        hitCounter++;
        previousHealth = _healthComponent.HealthPoints;
        
        //check if the char has been hit enough times to justify teleporting. 
        if (hitCounter < timesHitBeforeTeleporting)
        {
            return;
        }

        //Reset hit Counter
        hitCounter = 0;

        //teleport to the next spot. 
        previousLocation = teleportLocation;
        teleportLocation += 1;
        if (teleportLocation >= waypoints.Length)
        {
            teleportLocation = 0;
        }

        //Create a shadowClone where he was, then teleport.
        GameObject clone = Instantiate(
            shadowClone, 
            transform.position, 
            new Quaternion()
            );
        clone.GetComponent<EnemyCombat>().enabled = true;
        Teleport(teleportLocation);
        previousHealth = _healthComponent.HealthPoints;
    }

    //Teleport to the next location along the line of waypoints if any.
    private void Teleport(int num)
    {
        //Play Particles
        smokeBombParticle.Play();

        //Try to teleport, catch if there's no where to teleport to.
        try
        {
            //Change Location based on sent number
            transform.position = waypoints[num].teleportPosition;
            teleportLocation = num;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        //Play particles
        smokeBombParticle.Play();
        FaceRight(waypoints[teleportLocation].faceRight);
    }

    //Summon the end of level scroll when the boss dies.
    private void ScrollCheck()
    {
        if (_healthComponent.Dead || scroll.activeSelf)
        {
            scroll.SetActive(true);
        }
    }

    /// <summary>
    /// Faces the sprite to the right if sent a true boolean, otherwise faces left.
    /// </summary>
    private void FaceRight(bool faceRight)
    {
        transform.localScale =
            new Vector3(faceRight ? curLocalScale.x : -curLocalScale.x, curLocalScale.y, curLocalScale.z);
        
        if (!_healthComponent)
            return;

        // Update the enemy's health UI scale to make it opposite of the enemy's facing direction.
        // This makes it so the enemy health bar UI is always facing its static direction where the
        // fill is facing to the left.
        
        _healthComponent.enemyHealthImage.fillOrigin = transform.localScale.x switch
        {
            > 0  => (int)Image.OriginHorizontal.Left,
            < 0  => (int)Image.OriginHorizontal.Right,
            _ => (int)Image.OriginHorizontal.Left
        };
    }
}
