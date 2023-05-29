using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovements : MonoBehaviour
{
    public Transform Car;
    public float lerpSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.position = Car.transform.position;
        transform.rotation = Quaternion.Lerp(transform.rotation, Car.transform.rotation, Time.deltaTime * lerpSpeed);
    }
}