//Written by Tyler Hill
//Last Updated: 9/13/22
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Tooltip("How closely the object will move with the camera (100 will have the object unmoving, always in frame)")]
    public float followCameraAccuracy = 100;
    
    [Tooltip("How far ahead of the Camera you want the object to start")]
    public float xLead = 0;
   
    public GameObject Camera;

    private GameObject asset;
    
    private void Start()
    {
        asset = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        asset.transform.position = new Vector3((Camera.transform.position.x * (followCameraAccuracy/100)) + xLead, 0, 0);
    }
}
