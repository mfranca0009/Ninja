using UnityEngine;

public class ScrollFragment : MonoBehaviour
{
    #region Public Fields
    
    [Header("Scroll Fragment Settings")]
    
    [Tooltip("The scroll fragment number that is being obtained")]
    public int scrollNumber;
    
    #endregion

    #region Private Fields
    
    private GameManager _gameManager;
    private AchievementManager _achievementManager;

    #endregion
    
    // Start is called before the first frame update
    private void Start()
    {
        _achievementManager = FindObjectOfType<AchievementManager>();
        _gameManager = GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        _gameManager.CollectScroll(scrollNumber);


    }
}
