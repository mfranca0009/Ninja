using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PROTOTYPE_MoveToNextLevel : MonoBehaviour
{
    public string levelToLoad = "";

    //Scroll Motion
    [SerializeField] private float amplifier = 0.3f;
    [SerializeField] private float frequency = 3f;

    private Vector2 _homePos;

    private SceneManagement _sceneManager;
    private UIManager _UIManager;
    
    void Start()
    {
        _sceneManager = FindObjectOfType<SceneManagement>();
        _UIManager = FindObjectOfType<UIManager>();
        _UIManager.ShowScrollUI(false);

        _homePos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            _UIManager.ShowScrollUI(true);
        }
    }

    void Update()
    {
        LoopScrollMotion();
    }

    void LoopScrollMotion()
    {
        transform.position = new Vector3(_homePos.x, Mathf.Sin(Time.time * frequency) * amplifier + _homePos.y, 0);
    }
}
