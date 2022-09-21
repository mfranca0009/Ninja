using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpToPoint : MonoBehaviour
{
    //Script Variables
    [Tooltip("This vector3 designates where the colliding object will be warped to. If a point is set to -999, it will use the object's current x, y, or z point instead.")]
    public Vector3 warpPoint = Vector3.zero;
    private Vector3 newWarpPoint;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Cloud")
        {
            SetWarpPoint(warpPoint, collision.gameObject);
            collision.gameObject.transform.position = newWarpPoint;
            Debug.Log(newWarpPoint);

        }
    }

    //Script Functions
    private void SetWarpPoint(Vector3 v3, GameObject asset){
        SetWarpPoint(v3.x, v3.y, v3.z, asset);
    }

    private void SetWarpPoint(float x, float y, float z, GameObject asset)
    {
        if (x == -999) { x = asset.transform.position.x; }
        if (y == -999) { y = asset.transform.position.y; }
        if (z == -999) { z = asset.transform.position.z; }
        newWarpPoint = new Vector3(x, y, z);
    }
}
