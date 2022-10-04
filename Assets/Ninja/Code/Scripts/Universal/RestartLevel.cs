using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class RestartLevel : MonoBehaviour
{
    private Health _health;
    [SerializeField] private float respawnTimer = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        _health = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_health.Dead)
        {
            return;
        }

        respawnTimer -= Time.deltaTime;
        if (respawnTimer < 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

}
