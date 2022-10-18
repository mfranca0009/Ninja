using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneTimer : MonoBehaviour
{
    [SerializeField] private float waitToLoadScene = 60.0f;
    private float timeElapsed;
    [SerializeField] private string sceneToLoad;
    private SceneManagement sceneManagement;
    // Start is called before the first frame update
    void Start()
    {
        sceneManagement = FindObjectOfType<SceneManagement>();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > waitToLoadScene)
        {
            sceneManagement.LoadSceneByString(sceneToLoad);
        }
    }
}
