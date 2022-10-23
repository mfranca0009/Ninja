using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CanvasLevelNumber : MonoBehaviour
{
    #region Public Fields

    public TextMeshProUGUI textDisplay;

    #endregion

    #region Unity Events
    
    private void OnEnable()
    {
        textDisplay.text = gameObject.tag switch
        {
            "PauseCanvas" => $"Level {SceneManager.GetActiveScene().buildIndex - 1}",
            "ScrollCanvas" => $"Level {SceneManager.GetActiveScene().buildIndex - 1} Complete!",
            _ => textDisplay.text
        };
    }
    
    #endregion
}
