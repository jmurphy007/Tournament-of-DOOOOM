using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateProjectiles : MonoBehaviour
{
    public float x;
    public bool rotate;
    void Update()
    {
        if (rotate)
        {
            transform.Rotate(x * Time.deltaTime, 0, 0);
        } else
        {
            transform.rotation = Quaternion.Euler(x, transform.rotation.y, transform.rotation.z);
        }
    }
}