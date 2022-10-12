using UnityEngine;

public class EndOfLevelScroll : MonoBehaviour
{
    #region Public Fields
    
    // UNUSED - maybe in the future?
    // public string levelToLoad;

    #endregion
    
    #region Serialized Fields

    [Header("Scroll Motion Settings")]
    
    [Tooltip("The amplitude of the sin wave, how fast it will travel up/down.")]
    [SerializeField] private float amplifier = 0.3f;
    
    [Tooltip("The frequency of the sin wave, how far it will travel up/down.")]
    [SerializeField] private float frequency = 3f;

    #endregion

    #region Private Fields
    
    // Scroll
    private Vector2 _homePos;

    // UI Manager
    private UIManager _uiManager;
    
    #endregion
    
    #region Unity Events
    
    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
        
        if (_uiManager)
            _uiManager.ShowScrollUI(false);

        _homePos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || !_uiManager)
            return;
        
        _uiManager.ShowScrollUI(true);
    }

    private void Update()
    {
        LoopScrollMotion();
    }

    #endregion
    
    #region Update Methods
    
    private void LoopScrollMotion()
    {
        transform.position = new Vector3(_homePos.x, Mathf.Sin(Time.time * frequency) * amplifier + _homePos.y, 0);
    }
    
    #endregion
}