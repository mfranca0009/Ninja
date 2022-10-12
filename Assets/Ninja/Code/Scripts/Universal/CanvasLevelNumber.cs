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
            "PauseCanvas" => $"Level {SceneManager.GetActiveScene().buildIndex}",
            "ScrollCanvas" => $"Level {SceneManager.GetActiveScene().buildIndex} Complete!",
            _ => textDisplay.text
        };
    }
    
    #endregion
}
