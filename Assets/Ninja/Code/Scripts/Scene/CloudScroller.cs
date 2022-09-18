using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudScroller : MonoBehaviour
{

    [Tooltip("This value deternmines how fast the object scrolls accross the screen. Negitive values moves it right.")]
    public float Speed = 0.0f;

    private GameObject asset;
    private float step = 0.0f;
    private Vector3 targetPoint; 

    // Start is called before the first frame update
    void Start()
    {
        asset = this.gameObject;
        step = Speed * Time.deltaTime;
        targetPoint = new Vector3(-99, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, step);
    }
}
