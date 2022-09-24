using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class Canvas_levelName : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    int levelNumber;

    // Start is called before the first frame update
    void Start()
    {
        levelNumber = SceneManager.GetActiveScene().buildIndex;

        textDisplay.text = "Level " + levelNumber;
    }
}
