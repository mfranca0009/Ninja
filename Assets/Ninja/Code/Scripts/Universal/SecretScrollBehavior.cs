using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretScrollBehavior : MonoBehaviour
{
    public int scrollNumber;

    private GameManager _gameManager;
    private AchievementManager _achievementManager;

    // Start is called before the first frame update
    void Start()
    {
        _achievementManager = FindObjectOfType<AchievementManager>();
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
