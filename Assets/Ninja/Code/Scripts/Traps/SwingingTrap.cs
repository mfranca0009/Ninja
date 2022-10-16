using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingTrap : MonoBehaviour
{
    public GameObject trap;
    public GameObject blade;
    public float rotationSpeed = 360.0f;

    private bool shouldRotate = false;
    private bool hasActivated = false;
    private float step = -4.0f;
    private GameObject trapParent;
    private AchievementManager _achievementManager;

    void Start()
    {
        trapParent = trap.transform.parent.gameObject;
        _achievementManager = FindObjectOfType<AchievementManager>();
    }
    // Update is called once per frame
    void Update()
    {
        if (shouldRotate)
        {
            trap.transform.Rotate(0.0f, 0.0f, step * Time.deltaTime);
            step -= 1.0f;
            if (trapParent.transform.localScale.x >= 0.00f)
            {
                blade.transform.Rotate(0.0f, 0.0f, rotationSpeed);
                if (trap.transform.rotation.z <= -.92f) // Based on the z value of the rotation Quaternion
                {
                    shouldRotate = false;
                }
            }
            else
            {
                blade.transform.Rotate(0.0f, 0.0f, -rotationSpeed);

                if (trap.transform.rotation.z >= .92f) //Based on the z value of the rotation Quaternion
                {
                    shouldRotate = false;
                }
            }
        }
        
    }

    public bool ShouldRotate()
    {
        return shouldRotate;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || hasActivated)
            return;

        shouldRotate = true;
        hasActivated = true;

        _achievementManager.Achievements.Find(achi => achi.Title == "No Traps Activated").Eligible = false;
    }
}
