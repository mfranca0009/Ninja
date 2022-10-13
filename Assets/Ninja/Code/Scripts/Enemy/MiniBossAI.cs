using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TeleportWaypoint
{
    public Vector2 teleportPosition;
    public bool faceRight;
}

public class MiniBossAI : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("An array of waypoints that can be teleported to when hit")]
    [SerializeField] public TeleportWaypoint[] waypoints;
    private int teleportLocation;

    [Tooltip("The particle effect used in a burst when the Enemy is hit and teleports")]
    public ParticleSystem smokeBombParticle;

    #endregion

    #region Private Fields

    private Health _healthComponent;
    private float previousHealth;
    private Vector3 curLocalScale;
    private ParticleSystem smokeBomb1;
    private ParticleSystem smokeBomb2;

    #endregion

    #region Unity Events

    // Start is called before the first frame update
    void Start()
    {
        _healthComponent = GetComponent<Health>();
        previousHealth = _healthComponent.HealthPoints;
        curLocalScale = transform.localScale;
        smokeBomb1 = Instantiate(smokeBombParticle, transform.position, transform.rotation);
        smokeBomb2 = Instantiate(smokeBombParticle, transform.position, transform.rotation);
        Teleport(0);
    }

    // Update is called once per frame
    void Update()
    {
        DamageCheck();
        CleanOnDeath();
    }

    #endregion

    #region Private Helper Functions

    /// <summary>
    /// Clean up excess assets that have no purpose after the miniboss is dead.
    /// </summary>
    private void CleanOnDeath()
    {
        if (!_healthComponent.Dead)
            return;

        Destroy(smokeBomb1);
        Destroy(smokeBomb2);
    }

    /// <summary>
    /// This function checks if the miniboss has been hurt. If he has, he teleports to a random waypoint.
    /// </summary>
    private void DamageCheck()
    {
        //If the char is dead, or their hp hasn't lowered, return
        if (waypoints.Length <= 1 || _healthComponent.Dead || _healthComponent.HealthPoints >= previousHealth)
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

    /// <summary>
    /// This function plays the particle system that is sent in. 
    /// </summary>
    /// <param name="smokeBomb"></param>
    private void PlayParticles(ParticleSystem smokeBomb)
    {
        if (!smokeBomb)
            return;

        smokeBomb.transform.position = transform.position;
        smokeBomb.Play();
    }

    /// <summary>
    /// This Function plays a smokebomb particle effect where the player was, teleports to the waypoint sent in via num and then plays another smoke bomb effect where they appear at. 
    /// </summary>
    /// <param name="num"></param>
    private void Teleport(int num)
    {
        //Play Particles
        PlayParticles(smokeBomb1);
        if (waypoints.Length <= 1)
        {
            return;
        }
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
        PlayParticles(smokeBomb2);
        FaceRight(waypoints[teleportLocation].faceRight);
    }

    #endregion
}
