using UnityEngine;

public class LoadSceneTimer : MonoBehaviour
{
    #region Public Properties
    
    public int StartLoadSceneTimer { get; set; }

    #endregion
    
    #region Serialized Fields
    [Header("Load Scene Timer Settings")]
    
    [Tooltip("The delay before loading the specified scene")]
    [SerializeField] private float waitToLoadScene = 60.0f;
    
    [Tooltip("The scene name to load when the timer expires")]
    [SerializeField] private string sceneToLoad;
    
    #endregion

    #region Private Fields

    private float _waitToLoadSceneTimer;
    private SceneManagement _sceneManagement;
    
    #endregion

    #region Unity Events
    
    // Start is called before the first frame update
    private void Start()
    {
        _sceneManagement = FindObjectOfType<SceneManagement>();
    }

    private void OnEnable()
    {
        _waitToLoadSceneTimer = waitToLoadScene;
    }

    // Update is called once per frame
    private void Update()
    {
        if (StartLoadSceneTimer != 1)
            return;
        
        UpdateLoadSceneTimer();
    }
    
    #endregion
    
    #region Update Methods

    private void UpdateLoadSceneTimer()
    {
        if (_waitToLoadSceneTimer <= 0)
        {
            _sceneManagement.LoadSceneByString(sceneToLoad);
            _waitToLoadSceneTimer = waitToLoadScene;
        }
        else
            _waitToLoadSceneTimer -= Time.deltaTime;
    }
    
    #endregion
}
