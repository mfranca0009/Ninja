using System;
using UnityEngine;

public class EnemyMoveSoundRadius : MonoBehaviour
{
    #region Public Fields
    
    [Tooltip("The circle collider 2D used for setting a sound radius")]
    public CircleCollider2D circleCollider2D;

    #endregion
    
    #region Serialized Fields

    [Header("Sound Radius Settings")] 
    
    [Tooltip("The sound radius size")]
    [SerializeField] private float soundRadius = 4f;
    
    #endregion

    #region Private Fields

    private EnemyMovement _enemyMovement;
    
    #endregion
    
    #region Unity Events

    private void Awake()
    {
        _enemyMovement = GetComponent<EnemyMovement>();
        
        // If editor value is greater or less than the default collider radius, update it.
        if (soundRadius > circleCollider2D.radius || soundRadius < circleCollider2D.radius)
            circleCollider2D.radius = soundRadius;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!_enemyMovement || col.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;
        
        _enemyMovement.ShouldPlayWalkRunSFX = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!_enemyMovement || other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        _enemyMovement.ShouldPlayWalkRunSFX = false;
    }

    #endregion
}
