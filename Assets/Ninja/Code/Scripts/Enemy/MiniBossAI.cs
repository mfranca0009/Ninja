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
        if (_healthComponent.HealthPoints <= 0)
        {
            return;
        }
        if (_healthComponent.HealthPoints < previousHealth)
        {
            int randnum;
            do
            {
                randnum = (int)Math.Truncate(Random.value * 4);
            } while (randnum == teleportLocation);

            Teleport(randnum);
            previousHealth = _healthComponent.HealthPoints;
        }
    }

    private void Teleport(int num)
    {
        //TO-DO: When hit, play smoke particles and teleport to a new location
        //Play Particles
        smokeBombParticle.Play();

        //Change Location based on sent number
        switch (num)
        {
            case 0:
                transform.position = waypoints[0].teleportPosition;
                teleportLocation = 0;
                break;
            case 1:
                transform.position = waypoints[1].teleportPosition;
                teleportLocation = 1;
                break;
            case 2:
                transform.position = waypoints[2].teleportPosition;
                teleportLocation = 2;
                break;
            default:
                transform.position = waypoints[3].teleportPosition;
                teleportLocation = 3;
                break;
        }
        //play particles
        smokeBombParticle.Play();
        if (waypoints[teleportLocation].faceRight)
        {
            FaceRight(true);
        }
        else
        {
            FaceRight(false);
        }
        
        

    }

    /// <summary>
    /// Faces the sprite to the right if sent a true boolean, otherwise faces left.
    /// </summary>
    private void FaceRight(bool faceRight)
    {
        if (faceRight)
        {
            transform.localScale = new Vector3(0.2f, 0.2f, 0);
        }
        else
        {
            transform.localScale = new Vector3(-0.2f, 0.2f, 0);
        }
    }
}
