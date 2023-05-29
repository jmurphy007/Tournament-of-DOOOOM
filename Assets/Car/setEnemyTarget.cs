using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setEnemyTarget : MonoBehaviour
{
    public EnemyCarControl car;
    public List<GameObject> Targets = new List<GameObject>();
    public List<float> targetDistances = new List<float>();
    private float time;
    public float checkTime = 10;

    // Start is called before the first frame update
    void Start()
    {
        setTargets();
    }

    // Update is called once per frame
    void Update()
    {
        if (time > checkTime)
        {
            time = 0;
            setTargets();
        } 
        else
        {
            time += Time.deltaTime;
        }
    }

    void setTargets()
    {
        if (Targets.Count != 0)
        {
            Targets.Clear();
        }
        if (targetDistances.Count != 0)
        {
            targetDistances.Clear();
        }

        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Target"))
        {
            if (i != gameObject)
            {
                Targets.Add(i);
                targetDistances.Add(Vector3.Distance(i.transform.position, transform.position));
            }
        }
        float min = Mathf.Infinity;
        int index = 0;
        float[] tempArray = targetDistances.ToArray();

        for (int i = 0; i < tempArray.Length; i++)
        {
            if (tempArray[i] < min)
            {
                min = tempArray[i];
                index = i;
            }
        }

        car.Target = Targets[index];
    }
}
