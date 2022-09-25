using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PROTOTYPE_MoveToNextLevel : MonoBehaviour
{
    [SerializeField]
    private SceneManagement _sceneManager;

    public string levelToLoad = "";

     void Start()
    {
        _sceneManager = FindObjectOfType<SceneManagement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            _sceneManager.LoadSceneByString(levelToLoad);
        }
    }
}
