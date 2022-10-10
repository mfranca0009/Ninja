using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    #region Private Fields
    
    // Game Manager
    private GameManager _gameManager;
    
    #endregion

    #region Unity Events

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != LayerMask.NameToLayer("Player") || !_gameManager || (_gameManager &&
                _gameManager.LevelMidpointReached))
            return;

        _gameManager.LevelMidpointReached = true;
    }

    #endregion
}