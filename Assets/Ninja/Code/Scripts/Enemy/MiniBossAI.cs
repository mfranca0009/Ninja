using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

[Serializable]
public class TeleportWaypoint
{
    public Vector2 teleportPosition;
    public bool faceRight;
}

public class MiniBossAI : MonoBehaviour
{
    [Tooltip("An array of waypoints that can be teleported to when hit")]
    [SerializeField] public TeleportWaypoint[] waypoints;
    private int teleportLocation;

    [Tooltip("The particle effect used in a burst when the Enemy is hit and teleports")]
    public ParticleSystem smokeBombParticle;
    
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
        //If the char is dead, or their hp hasn't lowered, return
        if (_healthComponent.Dead || _healthComponent.HealthPoints >= previousHealth)
        {
            return;
        }

        //otherwise, teleport randomly to a new spot. 
        int randNum;
        do
        {
            randNum = Random.Range(0, waypoints.Length);
        } while (randNum == teleportLocation);

        Teleport(randNum);
        previousHealth = _healthComponent.HealthPoints;
    }

    private void Teleport(int num)
    {
        //Play Particles
        smokeBombParticle.Play();

        try
        {
            //Change Location based on sent number
            transform.position = waypoints[num].teleportPosition;
            teleportLocation = num;
        }catch (Exception ex)
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
