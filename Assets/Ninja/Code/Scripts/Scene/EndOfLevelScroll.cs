using UnityEngine;

public class EndOfLevelScroll : MonoBehaviour
{
    #region Public Fields
    
    // UNUSED - maybe in the future?
    // public string levelToLoad;

    #endregion

    #region Private Fields

    // UI Manager
    private UIManager _uiManager;
    
    #endregion
    
    #region Unity Events
    
    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();

        if (!_uiManager)
            return;
        
        _uiManager.ShowScrollUI(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || !_uiManager)
            return;
        
        _uiManager.ShowScrollUI(true);
    }

    #endregion
}