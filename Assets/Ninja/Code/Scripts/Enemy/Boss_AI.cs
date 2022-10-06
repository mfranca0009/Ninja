using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class Boss_AI : MonoBehaviour
{
    //Teleport Logic
    [Tooltip("An array of waypoints that can be teleported to when hit")]
    [SerializeField] public TeleportWaypoint[] waypoints;
    private int teleportLocation;
    private int previousLocation;

    [SerializeField] private int timesHitBeforeTeleporting = 3;
    private int hitCounter;

    //Prefabs
    [Tooltip("The particle effect used in a burst when the Enemy is hit and teleports")]
    public ParticleSystem smokeBombParticle;
    //ShadowClone Logic
    [Tooltip("What minion prefab to be summoned")]
    public GameObject shadowClone;
    private Transform cloneSummonLocation;

    //Health Logic
    private Health _healthComponent;
    private float previousHealth;


    // Start is called before the first frame update
    void Start()
    {
        _healthComponent = GetComponent<Health>();
        previousHealth = _healthComponent.HealthPoints;
        Teleport(0);
    }

    // Update is called once per frame
    void Update()
    {
        DamageCheck();
    }

    private void DamageCheck()
    {
        //Return if there is no place to teleport to, the char is dead, or they haven't taken damage.
        if (waypoints.Length <= 1 || _healthComponent.Dead || _healthComponent.HealthPoints >= previousHealth)
        {
            return;
        }

        //increment the hit counter, and check if the char was hit enough times to justify teleporting.
        hitCounter++;
        if (hitCounter < timesHitBeforeTeleporting)
        {
            return;
        }

        //teleport to the next spot. 
        previousLocation = teleportLocation;
        teleportLocation += 1;
        if (teleportLocation >= waypoints.Length)
        {
            teleportLocation = 0;
        }

        Instantiate(
            shadowClone, 
            new Vector3(waypoints[previousLocation].teleportPosition.x, waypoints[previousLocation].teleportPosition.y, transform.position.z), 
            new Quaternion()
            );
        Teleport(teleportLocation);
        previousHealth = _healthComponent.HealthPoints;
    }

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

    /// <summary>
    /// Faces the sprite to the right if sent a true boolean, otherwise faces left.
    /// </summary>
    private void FaceRight(bool faceRight)
    {
        Vector3 curLocalScale = transform.localScale;
        transform.localScale =
            new Vector3(faceRight ? curLocalScale.x : -curLocalScale.x, curLocalScale.y, curLocalScale.z);
    }
}
