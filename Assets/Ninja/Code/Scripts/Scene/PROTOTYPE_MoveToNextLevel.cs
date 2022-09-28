using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PROTOTYPE_MoveToNextLevel : MonoBehaviour
{
    [SerializeField]
    private SceneManagement _sceneManager;

    [SerializeField]
    private UIManager _UIManager;

    public string levelToLoad = "";

     void Start()
    {
        _sceneManager = FindObjectOfType<SceneManagement>();
        _UIManager = FindObjectOfType<UIManager>();
        _UIManager.hideScroll();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            _UIManager.showScroll();
        }
    }
}
