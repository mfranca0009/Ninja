using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretScrollBehavior : MonoBehaviour
{
    public int scrollNumber;

    private GameManager _gameManager;

    // Start is called before the first frame update
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        _gameManager.CollectScroll(scrollNumber);

        gameObject.SetActive(false);
    }
}
