using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyCarControl : MonoBehaviour
{
    public CarAI AI;
    public int AIRPM;
    public bool Alive = true;

    [SerializeField] WheelCollider[] wheelColliders;

    [SerializeField] Transform[] wheelTransforms;

    [SerializeField] private Renderer[] frontLeftTires, frontRightTires, backLeftTires, backRightTires;

    [SerializeField] private Light[] breakLights, reverseLights, headLights, signalLights;
    [SerializeField] private float breakIntensity, reverseIntensity, headIntensity, signalIntensity;
    [SerializeField] private bool headLightOn, signalsOn;

    [SerializeField] private Color32[] CarColors; // 0 = Base, 1 = Glow;

    [SerializeField] private Transform RobotHands;
    [SerializeField] private Renderer CarBody;
    [SerializeField] private Renderer[] Robots, Weapons; //0 = Knife, 1 = Sword, 2 = Hammer, 3 = Crossbow, 4 = Numchucks, 5 = Idle
    [SerializeField] private Animator[] RobotAnimations;
    [SerializeField] private Animator Numchucks;
    [SerializeField] private GameObject shieldEnergy;
    [SerializeField] private Animator ShieldUpDown;
    [SerializeField] private int currentWeapon;
    [SerializeField] private int emptyHand = 5;
    [SerializeField] private float heldTime = 0;
    private float heldPlus = 0;
    [SerializeField] private bool projectileHit = false;
    private bool canAttack = true;
    public Material energyMat;

    public EnemyLaunchProjectile shootArrow, hammerSwing, swordone, swordtwo, knifeone, knifetwo;

    public Rigidbody rb;
    public Vector3 com = new Vector3();

    [SerializeField] private AudioSource[] audioSrc; // 0 for engine, 1 for brakes
    [SerializeField] private float pitchStart;

    [SerializeField] private float startTime = 5;
    private bool enableControl = false;

    public float maxHealth = 15;
    public float currentHealth;

    private float boostDecimal;
    public float boostTimeLimit, boostSeconds;
    public bool canBoost = false;
    public bool boostStart = false;
    private int boostMult;

    [SerializeField] private ParticleSystem[] ExhaustParticles, BoostParticles;
    [SerializeField] private ParticleSystem Explosion;
    [SerializeField] private AudioSource Boom;

    public int CurrentGear; // int value to set current gear

    public GameObject Target, impactParticle;

    public float stopTime = 0f;
    public bool reverse = false;

    IEnumerator playerDeath()
    {
        Explosion.Play();
        if (!Boom.isPlaying)
        {
            Boom.PlayOneShot(Boom.clip);
        }

        yield return new WaitForSeconds(2f);
        Explosion.Pause();
        Boom.Stop();
        Destroy(gameObject);
    }

    private void particleControl(bool boost)
    {
        if (boost)
        {
            foreach (ParticleSystem i in ExhaustParticles)
            {
                if (i.isPlaying)
                {
                    i.Stop();
                }
            }
            foreach (ParticleSystem i in BoostParticles)
            {
                if (!i.isPlaying)
                {
                    i.Play();
                }
            }
        }
        else
        {
            foreach (ParticleSystem i in ExhaustParticles)
            {
                if (!i.isPlaying)
                {
                    i.Play();
                }
            }
            foreach (ParticleSystem i in BoostParticles)
            {
                if (i.isPlaying)
                {
                    i.Stop();
                }
            }
        }
    }

    private void Start()
    {
        AI.CustomDestination = Target.transform;
        CurrentGear = Random.Range(1, 5);
        AI.allowMovement = false;
        AIRPM = AI.MaxRPM;

        int chance = Random.Range(0, 100);
        if (chance <= 15)
        {
            Vector3 scale = new Vector3(RobotHands.transform.localScale.x * -1, RobotHands.transform.localScale.y, RobotHands.transform.localScale.z);
            RobotHands.transform.localScale = scale;
        }

        particleControl(false);
        Explosion.Stop();
        currentHealth = maxHealth;
        currentWeapon = emptyHand;

        pitchStart = audioSrc[0].pitch;
        currentHealth = maxHealth;

        for (int i = 0; i < CarColors.Length; i++)
        {
            CarColors[i] = new Color32(
            (byte)Random.Range(0, 255),
            (byte)Random.Range(0, 255),
            (byte)Random.Range(0, 255),
            255);
        }

        materialInstance(CarBody, 0, CarColors[0], false);
        materialInstance(CarBody, 8, CarColors[1], true);

        materialInstance(Weapons[0], 1, CarColors[1], true);
        materialInstance(Weapons[1], 1, CarColors[1], true);
        materialInstance(Weapons[2], 1, CarColors[1], true);
        materialInstance(Weapons[3], 1, CarColors[1], true);
        materialInstance(Weapons[4], 3, CarColors[1], true);

        energyMat = shieldEnergy.GetComponent<Renderer>().material;
        Color32 energy = new Color32(
            CarColors[1].r, CarColors[1].g, CarColors[1].b, 255);
        energyMat.SetColor("_Color", energy);
        energyMat.SetColor("_RimColor", energy);
        shieldEnergy.GetComponent<Renderer>().material = energyMat;

        for (int i = 0; i < Robots.Length; i++)
        {
            materialInstance(Robots[i], 2, CarColors[0], false);
            materialInstance(Robots[i], 1, CarColors[1], true);
        }
        setRobot(emptyHand);

        int[] tireElements = { 1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34 };
        for (int i = 0; i < frontLeftTires.Length; i++)
        {
            for (int j = 0; j < tireElements.Length; j++)
            {
                materialInstance(frontLeftTires[i], tireElements[j], CarColors[1], true);
                materialInstance(frontRightTires[i], tireElements[j], CarColors[1], true);
                materialInstance(backLeftTires[i], tireElements[j], CarColors[1], true);
                materialInstance(backRightTires[i], tireElements[j], CarColors[1], true);
            }
        }
        headLightOn = true;
        headLightsOnOff();

        boostDecimal = boostTimeLimit / 2;
        boostMult = 1;

        rb.centerOfMass = com; // Updates the car's center of mass to prevent tipping over
    }

    private void setRobot(int type)
    {
        if (type == 0)
        {
            robotEnable(true, false, false, false, false, false);
        }
        else if (type == 1)
        {
            robotEnable(false, true, false, false, false, false);
        }
        else if (type == 2)
        {
            robotEnable(false, false, true, false, false, false);
        }
        else if (type == 3)
        {
            robotEnable(false, false, false, true, false, false);
        }
        else if (type == 4)
        {
            robotEnable(false, false, false, false, true, false);
        }
        else
        {
            robotEnable(false, false, false, false, false, true);
        }
    }

    private void robotEnable(bool zero, bool one, bool two, bool three, bool four, bool five)
    {
        Robots[0].enabled = zero;
        Robots[1].enabled = one;
        Robots[2].enabled = two;
        Robots[3].enabled = three;
        Robots[4].enabled = four;
        Robots[5].enabled = five;
        RobotAnimations[0].enabled = zero;
        RobotAnimations[1].enabled = one;
        RobotAnimations[2].enabled = two;
        RobotAnimations[3].enabled = three;
        RobotAnimations[4].enabled = four;
        RobotAnimations[5].enabled = five;
        Weapons[0].enabled = zero;
        Weapons[1].enabled = one;
        Weapons[2].enabled = two;
        Weapons[3].enabled = three;
        Weapons[4].enabled = four;
        Numchucks.enabled = four;
    }

    public void HammerAttack()
    {
        hammerSwing.fire((float)AIRPM, currentWeapon);
        canAttack = false;
    }

    public void SwordAttackOne()
    {
        swordone.fire((float)AIRPM, currentWeapon);
        canAttack = false;
    }

    public void SwordAttackTwo()
    {
        swordtwo.fire((float)AIRPM, currentWeapon);
        canAttack = false;
    }
    public void KnifeAttackOne()
    {
        knifeone.fire((float)AIRPM, currentWeapon);
        canAttack = false;
    }

    public void KnifeAttackTwo()
    {
        knifetwo.fire((float)AIRPM, currentWeapon);
        canAttack = false;
    }

    public void ArrowShot()
    {
        shootArrow.fire((float)AIRPM, currentWeapon);
        canAttack = false;
    }

    private void robotAttack(int type, int attackAmount)
    {
        if (heldTime > .1f)
        {
            if (canAttack)
            {
                RobotAnimations[type].SetInteger("attackAmount", attackAmount);

                if (type == 4)
                {
                    Numchucks.SetInteger("attackAmount", attackAmount);
                    ShieldUpDown.SetBool("shieldUp", true);
                    projectileHit = true;
                }
                else
                {
                    ShieldUpDown.SetBool("shieldUp", false);
                }
                if (heldTime > 2.5f)
                {
                    clearAttack(type);
                }
            }
        }
        else
        {
            clearAttack(type);
        }
    }

    private void clearAttack(int type)
    {
        RobotAnimations[type].SetInteger("attackAmount", 0);
        if (type == 4)
        {
            Numchucks.SetInteger("attackAmount", 0);
            projectileHit = false;
        }
        ShieldUpDown.SetBool("shieldUp", false);
        canAttack = true;
    }

    public void materialInstance(Renderer rend, int index, Color32 color, bool emission)
    {
        Material[] materials = rend.materials;
        if (emission == true)
        {
            materials[index].SetColor("_EmissionColor", color);
        }
        materials[index].SetColor("_Color", color);
        rend.materials = materials;
    }

    private void enableEmission(Renderer rend, int index, bool enabled)
    {
        Material[] materials = rend.materials;

        if (enabled == true)
        {
            materials[index].EnableKeyword("_EMISSION");
        }
        else
        {
            materials[index].DisableKeyword("_EMISSION");
        }
        rend.materials = materials;
    }

    private void lightOnOff(int matIndex, Light[] lights, float intensity, bool onOff)
    {
        enableEmission(CarBody, matIndex, onOff);
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = intensity;
        }
    }

    private void headLightsOnOff()
    {
        if (headLightOn == true)
        {
            lightOnOff(5, headLights, headIntensity, true);
        }
        else
        {
            lightOnOff(5, headLights, 0, false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            currentWeapon = other.gameObject.GetComponent<giveInt>().giveWeapon;
        }

        if (projectileHit == false)
        {
            if (other.tag == "KnifeSlash")
            {
                damage(.5f);
            }
            if (other.tag == "SwordSlash")
            {
                damage(1.5f);
            }
            if (other.tag == "HammerEnergy")
            {
                damage(3f);
            }
            if (other.tag == "Arrow")
            {
                damage(0.75f);
            }
            if (other.tag == "Spikes")
            {
                damage(0.25f);
            }
        }
    }

    private void damage(float damage)
    {
        currentHealth -= damage;
        audioSrc[2].PlayOneShot(audioSrc[2].clip);
    }

    private void OnCollisionEnter(Collision collision)
    {
        int opposingGear, notMovingBonus;
        Rigidbody opposingBody;

        if (rb.velocity.magnitude == 0)
        {
            notMovingBonus = 2;
        }
        else
        {
            notMovingBonus = 0;
        }

        if (collision.collider.tag == "Player")
        {
            opposingGear = collision.gameObject.GetComponent<CarControl>().CurrentGear;
            opposingBody = collision.gameObject.GetComponent<Rigidbody>();

            if (opposingBody.velocity.magnitude > rb.velocity.magnitude)
            {
                damage(Mathf.Abs(opposingGear - CurrentGear) + notMovingBonus);
            }
        }
        if (collision.collider.tag == "Enemy")
        {
            opposingGear = collision.gameObject.GetComponent<EnemyCarControl>().CurrentGear;
            opposingBody = collision.gameObject.GetComponent<Rigidbody>();

            if (opposingBody.velocity.magnitude > rb.velocity.magnitude)
            {
                damage(Mathf.Abs(opposingGear - CurrentGear) + notMovingBonus);
            }
        }

        if (projectileHit == false)
        {
            if (collision.collider.tag == "KnifeSlash")
            {
                damage(.5f);
            }
            if (collision.collider.tag == "SwordSlash")
            {
                damage(1.5f);
            }
            if (collision.collider.tag == "HammerEnergy")
            {
                damage(3f);
            }
            if (collision.collider.tag == "Arrow")
            {
                damage(0.75f);
            }
            if (collision.collider.tag == "Spikes")
            {
                damage(0.25f);
            }
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            StartCoroutine(playImpact(contact));
        }
    }

    IEnumerator playImpact(ContactPoint contact)
    {
        GameObject impactInstance;
        impactInstance = Instantiate(impactParticle, contact.point, Quaternion.identity);

        yield return new WaitForSeconds(3);

        Destroy(impactInstance);
    }

    private void FixedUpdate()
    {
        if (currentHealth <= 0)
        {
            StartCoroutine(playerDeath());
        }

        if (enableControl == false)
        {
            if (boostDecimal < boostTimeLimit)
            {
                boostDecimal += Time.deltaTime;
            }
            else
            {
                boostDecimal = boostTimeLimit;
                canBoost = true;
            }

            if (startTime >= 0.75f)
            {
                startTime -= Time.deltaTime;
                AI.allowMovement = false;
                AI.MaxRPM = 0;
            }
            else
            {
                startTime -= Time.deltaTime;
                if (startTime < 0)
                {
                    enableControl = true;
                    AI.allowMovement = true;
                    AI.MaxRPM = AIRPM;
                }
            }
        }
        else
        {
            if (Alive)
            {
                if (Target != null)
                {
                    AI.CustomDestination = Target.transform;
                }
                setRobot(currentWeapon);
                HShifter();
                headLightsOnOff();

                if (reverse == false)
                {
                    if (rb.velocity.magnitude <= 0.25f)
                    {
                        stopTime += Time.deltaTime;
                        if (stopTime > 5)
                        {
                            reverse = true;
                        }
                    }
                }
                else
                {
                    if (stopTime > 0)
                    {
                        stopTime -= Time.deltaTime;
                        AI.MovementTorque = -1;
                    }
                    else
                    {
                        stopTime = 0;
                        AI.MovementTorque = 1;
                        reverse = false;
                    }
                }

                if (boostMult == 2)
                {
                    currentWeapon = emptyHand;
                    if (boostStart)
                    {
                        RobotAnimations[emptyHand].SetBool("blocking", true);
                        boostStart = false;
                        ShieldUpDown.SetBool("shieldUp", false);
                    }
                    audioSrc[0].pitch = pitchStart + 0.25f;
                    particleControl(true);
                }
                else
                {
                    boostStart = true;
                    RobotAnimations[emptyHand].SetBool("blocking", false);
                    audioSrc[0].pitch = pitchStart;
                    ShieldUpDown.SetBool("shieldUp", false);
                    particleControl(false);
                }

                if (currentWeapon < emptyHand)
                {
                    if (Vector3.Distance(transform.position, AI.CustomDestination.transform.position) < 75f)
                    {
                        heldTime += Time.deltaTime;
                        if (heldTime < 3 + heldPlus)
                        {
                            robotAttack(currentWeapon, 2);
                        }
                        else
                        {
                            heldPlus += 3f;
                            robotAttack(currentWeapon, 0);
                        }
                    } 
                    else
                    {
                        heldTime = 0f;
                        heldPlus = 0f;
                        robotAttack(currentWeapon, 0);
                    }
                }

                if (canBoost == false)
                {
                    if (boostDecimal < boostTimeLimit)
                    {
                        boostDecimal += Time.deltaTime;
                    }
                    else
                    {
                        boostDecimal = boostTimeLimit;
                        canBoost = true;
                    }
                }
                else
                {
                    if (boostSeconds == boostTimeLimit && Vector3.Distance(transform.position, AI.CustomDestination.transform.position) > 75f)
                    {
                        if (boostDecimal > 0)
                        {
                            boostDecimal -= Time.deltaTime * 2;
                            boostMult = 2;
                        }
                        else
                        {
                            boostDecimal = 0;
                            canBoost = false;
                            boostMult = 1;
                        }
                    } else if (boostMult == 2 && boostSeconds < boostTimeLimit)
                    {
                        if (boostDecimal > 0)
                        {
                            boostDecimal -= Time.deltaTime * 2;
                            boostMult = 2;
                        }
                        else
                        {
                            boostDecimal = 0;
                            canBoost = false;
                            boostMult = 1;
                        }
                    }
                    else
                    {
                        boostMult = 1;
                    }
                }
                boostSeconds = Mathf.RoundToInt(boostDecimal);

                if (AI.MaxRPM > 0)
                {
                    AI.MaxRPM = AIRPM * CurrentGear * boostMult;
                }

                if (AI.MovementTorque < 0)
                {
                    lightOnOff(7, reverseLights, reverseIntensity, true);
                }
                else
                {
                    lightOnOff(7, reverseLights, 0, false);
                }
            }
        }
    }

    // Takes the current gear and one of the mesh render lists to change the wheel model based on the current gear
    void setWheelVisibility(Renderer[] renderList)
    {
        if (CurrentGear == 2)
        {
            renderList[0].enabled = false;
            renderList[1].enabled = true;
            renderList[2].enabled = false;
            renderList[3].enabled = false;
            renderList[4].enabled = false;
        }
        else if (CurrentGear == 3)
        {
            renderList[0].enabled = false;
            renderList[1].enabled = false;
            renderList[2].enabled = true;
            renderList[3].enabled = false;
            renderList[4].enabled = false;
        }
        else if (CurrentGear == 4)
        {
            renderList[0].enabled = false;
            renderList[1].enabled = false;
            renderList[2].enabled = false;
            renderList[3].enabled = true;
            renderList[4].enabled = false;
        }
        else if (CurrentGear == 5)
        {
            renderList[0].enabled = false;
            renderList[1].enabled = false;
            renderList[2].enabled = false;
            renderList[3].enabled = false;
            renderList[4].enabled = true;
        }
        else
        {
            renderList[0].enabled = true;
            renderList[1].enabled = false;
            renderList[2].enabled = false;
            renderList[3].enabled = false;
            renderList[4].enabled = false;
        }
    }
    void setWheelVisibilities()
    {
        setWheelVisibility(frontLeftTires);
        setWheelVisibility(frontRightTires);
        setWheelVisibility(backLeftTires);
        setWheelVisibility(backRightTires);
    }

    void HShifter()
    {
        setGear(CurrentGear);
    }
    void setGear(int gear)
    {
        if (gear != 0)
        {
            CurrentGear = gear;
        }
        else
        {
            CurrentGear = 0;
        }
        setWheelVisibilities();
    }
}