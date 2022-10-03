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

    //Scroll Motion
    [SerializeField][Range(0, 1)] float speed = 1f;
    [SerializeField][Range(-2, 1)] float range = -2f;


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

    void Update()
    {
        LoopScrollMotion();
    }

    void LoopScrollMotion()
    {
        float yPos = Mathf.PingPong(Time.time * speed, 1) * -range;
        transform.position = new Vector2(transform.position.x, yPos);    
    }
}
