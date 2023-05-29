using System.Collections;
using UnityEngine;

public class destoryProjectile : MonoBehaviour
{

    private void Awake()
    {
        StartCoroutine(destroyAfterTime());
    }

    IEnumerator destroyAfterTime()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= -250f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
