using UnityEngine;

public class LaunchProjectile : MonoBehaviour {
    public GameObject projectile;
    private GameObject shot;
    public float launchVelocity = 850f;
    public CarControl car;
    
    public void fire(float currentAcceleration, int type)
    {
        if (type != 3)
        {
            Material mat = projectile.GetComponent<Renderer>().sharedMaterial;
            mat = car.energyMat;
            projectile.GetComponent<Renderer>().sharedMaterial = mat;
        }
        shot = Instantiate(projectile, transform.position, transform.rotation);
        shot.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, launchVelocity + currentAcceleration / 2));
    }
}

