using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarControl : MonoBehaviour
{
    public bool Alive = true;
    public bool gameEnd = false;
    public AudioSource levelMusic, pauseMusic;
    private bool isPaused = false;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private Image pauseBackground;
    [SerializeField] private Button[] pauseBtns; // 0 for Continue, 1 for Controls, 2 for Resest, 3 for Main Menu
    [SerializeField] private GameObject ControlBtnImgs, GameOverBtnImgs;

    public ControlBtns controls;

    [SerializeField] private TextMeshProUGUI gameEndText;
    [SerializeField] private Image gameEndBackground;
    [SerializeField] private Button[] gameEndBtns; // 0 for Reset, 1  for Main Menu

    public LevelLoader loader;

    private WheelCollider frontLeft, frontRight, backLeft, backRight;

    [SerializeField] WheelCollider[] wheelColliders;

    [SerializeField] Transform[] wheelTransforms;

    [SerializeField] private Renderer[] frontLeftTires, frontRightTires, backLeftTires, backRightTires;

    [SerializeField] private Light[] breakLights, reverseLights, headLights, signalLights;
    [SerializeField] private float breakIntensity, reverseIntensity, headIntensity, signalIntensity;
    [SerializeField] private bool headLightOn, signalsOn;
    private float signalTimer = 0f;

    [SerializeField] private Camera[] cameras;
    [SerializeField] private float zoomMult = 0.25f;

    [SerializeField] private Color32[] CarColors; // 0 = Base, 1 = Glow;

    [SerializeField] private Transform RobotHands;
    [SerializeField] private Renderer CarBody;
    [SerializeField] private Renderer[] Robots, Weapons, UIWeapons; //0 = Knife, 1 = Sword, 2 = Hammer, 3 = Crossbow, 4 = Numchucks, 5 = Idle
    [SerializeField] private Animator[] RobotAnimations;
    [SerializeField] private Animator Numchucks;
    [SerializeField] private Renderer UIArrow;
    [SerializeField] private GameObject shieldEnergy;
    [SerializeField] private Animator ShieldUpDown;
    [SerializeField] private int currentWeapon;
    [SerializeField] private int emptyHand = 4;
    [SerializeField] private float heldTime = 0;
    [SerializeField] private bool projectileHit = false;
    private bool canAttack = true;
    public Material energyMat;

    public LaunchProjectile shootArrow, hammerSwing, swordone, swordtwo, knifeone, knifetwo;

    public Rigidbody rb;
    public Vector3 com = new Vector3();

    [SerializeField] private AudioSource[] gameEndsounds; // 0 for win, 1 for lose
    private bool gameEndSoundPlayed = false;
    [SerializeField] private AudioSource[] audioSrc; // 0 for engine, 1 for brakes. 2 for countdown, 3 for GO, 4 for Damage, 5 for item collect
    [SerializeField] private float pitchStart;
    private bool braking;

    [SerializeField] private float startTime = 5;
    private bool enableControl = false;

    [SerializeField] private TextMeshProUGUI StartTimer, Gear;
    [SerializeField] private Image Booster, Health, BoosterFade, HealthFade;
    [SerializeField] private Image[] Glows;

    [SerializeField] private float maxHealth = 10;
    [SerializeField] private float currentHealth;
    private bool healthStart = false;
    
    private float boostDecimal;
    public float boostTimeLimit, boostSeconds;
    public bool canBoost = false;
    public bool boostStart = false;
    private int boostMult;

    [SerializeField] private ParticleSystem[] ExhaustParticles, BoostParticles;
    [SerializeField] private ParticleSystem Explosion;
    [SerializeField] private AudioSource Boom;

    public float acceleration = 500f;
    public float brakingForce = 250f;
    public float maxTurnAngle = 30f;

    public float currentAcceleration = 0f;
    [SerializeField] private float currentBrakingForce = 0f;
    private float currentTurnAngle = 0f;

    public float xAxes, GasInput, BrakeInput, ClutchInput;
    /* 
        xAxes is for the steering wheel input
        GasInput, BrakeInput, and ClutchInput are for their respective pedal inputs.
    */

    public int CurrentGear; // int value to set current gear

    private KeyCode[] keyInputs = { // all of the number inputs for gears for keyboard users
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6
    };

    public int enemiesLeft = 0;
    public GameObject[] enemies;
    [SerializeField] private TextMeshProUGUI countText;

    private LogitechGSDK.LogiControllerPropertiesData properties;
    public float stopTurnForce = 500f;
    public int wheelConstant = 35;
    public int impactForce = 60;

    [SerializeField] private GameObject impactParticle;

    private bool controlPressed = false;

    private void cursorLock(bool visible)
    {
        Cursor.visible = visible;
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
        } 
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void setPauseMenu(bool visible)
    {
        pauseText.enabled = visible;
        pauseBackground.enabled = visible;
        ControlBtnImgs.SetActive(visible);
        foreach (Button i in pauseBtns)
        {
            i.gameObject.SetActive(visible);
        }
        cursorLock(visible);
    }
    public void pauseGame()
    {
        if (Alive)
        {
            if (isPaused == true)
            {
                Time.timeScale = 1;
                isPaused = false;
            }
            else
            {
                Time.timeScale = 0;
                isPaused = true;
            }
            setPauseMenu(isPaused);
        }
    }

    public void endGame(bool visible, string endText)
    {
        if (visible)
        {
            if (enemiesLeft > 0)
            {
                Alive = false;
            }
        }
        gameEndText.enabled = visible;
        gameEndBackground.enabled = visible;
        GameOverBtnImgs.SetActive(visible);
        foreach (Button i in gameEndBtns)
        {
            i.gameObject.SetActive(visible);
        }
        gameEndText.text = endText;
        cursorLock(visible);
    }

    IEnumerator playerDeath()
    {
        Explosion.Play();
        if (!Boom.isPlaying)
        {
            Boom.PlayOneShot(Boom.clip);
        }
        currentAcceleration = 0;
        currentBrakingForce = brakingForce * 10;
        
        yield return new WaitForSeconds(2f);
        Explosion.Pause();
        Boom.Stop();
        endGame(true, "Game Over");
        if (gameEndSoundPlayed == false)
        {
            gameEndsounds[1].Play();
            gameEndSoundPlayed = true;
        }
    }


    public void restartGame()
    {
        Time.timeScale = 1;
        loader.LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    public void mainMenu()
    {
        Time.timeScale = 1;
        loader.LoadLevel(0);
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
        if (MainMenu.leftHand)
        {
            Vector3 scale = new Vector3(RobotHands.transform.localScale.x * -1, RobotHands.transform.localScale.y, RobotHands.transform.localScale.z);
            RobotHands.transform.localScale = scale;
        }
        particleControl(false);
        Explosion.Stop();
        currentHealth = maxHealth;
        currentWeapon = emptyHand;
        setPauseMenu(false);
        controls.controlsScreen(false);

        pitchStart = audioSrc[0].pitch;
        currentHealth = maxHealth;

        if (MainMenu.randomColor)
        {
            for (int i = 0; i < CarColors.Length; i++)
            {
                CarColors[i] = new Color32(
                (byte)Random.Range(0, 255),
                (byte)Random.Range(0, 255),
                (byte)Random.Range(0, 255),
                255);
            }
        } else
        {
            for (int i = 0; i < CarColors.Length; i++)
            {
                CarColors[i] = showCustomizations.customColors[i];
            }
        }

        foreach (Image i in Glows)
        {
            i.color = CarColors[1];
        }

        materialInstance(CarBody, 0, CarColors[0], false);
        materialInstance(CarBody, 8, CarColors[1], true);
        
        materialInstance(Weapons[0], 1, CarColors[1], true);
        materialInstance(UIWeapons[0], 1, CarColors[1], true);
        materialInstance(Weapons[1], 1, CarColors[1], true);
        materialInstance(UIWeapons[1], 1, CarColors[1], true);
        materialInstance(Weapons[2], 1, CarColors[1], true);
        materialInstance(UIWeapons[2], 1, CarColors[1], true);
        materialInstance(Weapons[3], 1, CarColors[1], true);
        materialInstance(UIWeapons[3], 1, CarColors[1], true);
        materialInstance(Weapons[4], 3, CarColors[1], true);
        materialInstance(UIWeapons[4], 3, CarColors[1], true);

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

        int[] tireElements = {1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34};
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

        frontLeft = wheelColliders[0];
        frontRight = wheelColliders[1];
        backLeft = wheelColliders[2];
        backRight = wheelColliders[3];

        boostDecimal = boostTimeLimit / 2;
        boostMult = 1;

        print(LogitechGSDK.LogiSteeringInitialize(false));
        // Checks if a Logitech steering wheel is connected and informs the user.
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            print("Steering Wheel Connected");
            properties = new LogitechGSDK.LogiControllerPropertiesData();
            properties.forceEnable = true;
        }
        else
        {
            print("No Steering Wheel Connected");
        }
        rb.centerOfMass = com; // Updates the car's center of mass to prevent tipping over
    }

    private void fadeOnFull(Image fade, bool fading)
    {
        Color32 fadeOut = new Color(255f, 255f, 255f, 0f);
        Color32 fadeIn = new Color(255f, 255f, 255f, 200f);

        if (fading)
        {
            fade.color = Color.Lerp(fadeOut, fadeIn, Mathf.Sin(Time.time * 2.25f));
        }
        else
        {
            fade.color = fadeOut;
        }
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
        UIWeapons[0].enabled = zero;
        UIWeapons[1].enabled = one;
        UIWeapons[2].enabled = two;
        UIWeapons[3].enabled = three;
        UIWeapons[4].enabled = four;
        Numchucks.enabled = four;
        UIArrow.enabled = three;
    }

    public void HammerAttack()
    {
        hammerSwing.fire(currentAcceleration, currentWeapon);
        canAttack = false;
    }

    public void SwordAttackOne()
    {
        swordone.fire(currentAcceleration, currentWeapon);
        canAttack = false;
    }

    public void SwordAttackTwo()
    {
        swordtwo.fire(currentAcceleration, currentWeapon);
        canAttack = false;
    }
    public void KnifeAttackOne()
    {
        knifeone.fire(currentAcceleration, currentWeapon);
        canAttack = false;
    }

    public void KnifeAttackTwo()
    {
        knifetwo.fire(currentAcceleration, currentWeapon);
        canAttack = false;
    }

    public void ArrowShot()
    {
        shootArrow.fire(currentAcceleration, currentWeapon);
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
                } else
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
        } else
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

    private void signalLightsOnOff()
    {
        if (signalsOn == true)
        {
            lightOnOff(3, signalLights, signalIntensity, true);
        }
        else
        {
            lightOnOff(3, signalLights, 0, false);
        }
    }

    void OnApplicationQuit()
    {
        LogitechGSDK.LogiSteeringShutdown();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            currentWeapon = other.gameObject.GetComponent<giveInt>().giveWeapon;
            audioSrc[5].Stop();
            audioSrc[5].Play();
        }

        if (other.tag == "Spikes")
        {
            damage(0.25f);
            int playedForce = impactForce / 30;
            LogitechGSDK.LogiPlayFrontalCollisionForce(0, playedForce);
        }
    }

    private void damage(float damage)
    {
        currentHealth -= damage;
        audioSrc[4].PlayOneShot(audioSrc[4].clip);
    }

    private void OnCollisionEnter(Collision collision)
    {
        int opposingGear, notMovingBonus;
        Rigidbody opposingBody;
        int playedForce = (impactForce / 10) * Mathf.Abs(CurrentGear);

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
                playedForce = impactForce;
            }
        }
        if (collision.collider.tag == "Enemy")
        {
            opposingGear = collision.gameObject.GetComponent<EnemyCarControl>().CurrentGear;
            opposingBody = collision.gameObject.GetComponent<Rigidbody>();

            if (opposingBody.velocity.magnitude > rb.velocity.magnitude)
            {
                damage(Mathf.Abs(opposingGear - CurrentGear) + notMovingBonus);
                playedForce = impactForce;
            }
        }

        if (projectileHit == false)
        {
            if (collision.collider.tag == "KnifeSlash")
            {
                damage(.5f);
                playedForce = impactForce / 15;
            }
            if (collision.collider.tag == "SwordSlash")
            {
                damage(1.5f);
                playedForce = impactForce / 4;
            }
            if (collision.collider.tag == "HammerEnergy")
            {
                damage(3f);
                playedForce =  impactForce / 3;
            }
            if (collision.collider.tag == "Arrow")
            {
                damage(0.75f);
                playedForce = impactForce / 12;
            }
            if (collision.collider.tag == "Spikes")
            {
                damage(0.25f);
                playedForce =  impactForce / 30;
            }
        }

        LogitechGSDK.LogiPlayFrontalCollisionForce(0, playedForce);

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

    private void slowDown()
    {
        if (currentAcceleration > 0 && CurrentGear > 0)
        {
            currentAcceleration -= 100f;
        }
        else if (currentAcceleration < 0f && CurrentGear == -1)
        {
            currentAcceleration += 100f;
        }
        else
        {
            currentAcceleration = 0f;
        }
    }

    private void fade(Image bar, Image fade)
    {
        if (bar.fillAmount == 1)
        {
            fadeOnFull(fade, true);
        }
        else
        {
            fadeOnFull(fade, false);
        }
    }

    private void Update()
    {
        if (currentHealth > 0)
        {
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
            enemiesLeft = enemies.Length;
            countText.text = enemiesLeft.ToString();
            if (enemiesLeft <= 0)
            {
                if (gameEndSoundPlayed == false)
                {
                    gameEndsounds[0].Play();
                    gameEndSoundPlayed = true;
                }
                endGame(true, "You Win!");
                gameEnd = true;
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    if (LogitechGSDK.LogiButtonReleased(0, 23) || LogitechGSDK.LogiButtonIsPressed(0, 2))
                    {
                        mainMenu();
                    }

                    if (LogitechGSDK.LogiButtonIsPressed(0, 1))
                    {
                        restartGame();
                    }
                }
                else
                {
                    if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.D))
                    {
                        mainMenu();
                    }

                    if (Input.GetKeyUp(KeyCode.A))
                    {
                        restartGame();
                    }
                }
            } 
            else
            {
                gameEnd = false;
            }
            if (isPaused)
            {
                levelMusic.Pause();
                if (!pauseMusic.isPlaying)
                {
                    pauseMusic.Play();
                }
                if (audioSrc[0].isPlaying)
                {
                    audioSrc[0].Pause();
                }
                if (audioSrc[1].isPlaying)
                {
                    audioSrc[1].Pause();
                }
                if (signalTimer > 10)
                {
                    if (signalsOn == true)
                    {
                        signalsOn = false;
                    }
                    else
                    {
                        signalsOn = true;
                    }
                    signalTimer = 0f;
                }
                else
                {
                    signalTimer += 0.1f;
                }
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    if (LogitechGSDK.LogiButtonReleased(0, 23))
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            pauseGame();
                        }
                    }

                    if (LogitechGSDK.LogiButtonIsPressed(0, 1))
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            controls.ControlsScreen.SetActive(true);
                        }
                    }

                    if (LogitechGSDK.LogiButtonIsPressed(0, 3))
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            pauseGame();
                        }
                    }

                    if (controls.ControlsScreen.activeInHierarchy == true)
                    {
                        if (LogitechGSDK.LogiButtonIsPressed(0, 2))
                        {
                            controls.ControlsScreen.SetActive(false);
                        }
                    }
                    else
                    {
                        if (LogitechGSDK.LogiButtonReleased(0, 2))
                        {
                            restartGame();
                        }
                    }

                    if (LogitechGSDK.LogiButtonIsPressed(0, 0))
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            mainMenu();
                        } 
                        else
                        {
                            if (controlPressed == false)
                            {
                                controls.updateControlDisplay();
                                controlPressed = true;
                            }
                        }
                    }
                    else
                    {
                        controlPressed = false;
                    }
                }
                else
                {
                    if (Input.GetKeyUp(KeyCode.Escape))
                    {
                        pauseGame();
                    }

                    if (Input.GetKeyUp(KeyCode.D))
                    {
                        if (controls.ControlsScreen.activeInHierarchy == true)
                        {
                            controls.ControlsScreen.SetActive(false); ;
                        } else
                        {
                            restartGame();
                        }
                    }

                    if (Input.GetKeyUp(KeyCode.W))
                    {
                        pauseGame();
                    }

                    if (Input.GetKeyUp(KeyCode.A))
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            controls.ControlsScreen.SetActive(true);
                        }
                    }

                    if (Input.GetKeyUp(KeyCode.S))
                    {
                        if (controls.ControlsScreen.activeInHierarchy == true)
                        {
                            controls.updateControlDisplay();
                        }
                        else
                        {
                            mainMenu();
                        }
                    }
                }
            }
            else
            {
                levelMusic.UnPause();
                pauseMusic.Stop();
                audioSrc[0].UnPause();
                audioSrc[1].UnPause();
                signalsOn = false;
                signalTimer = 0f;
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    if (LogitechGSDK.LogiButtonIsPressed(0, 23))
                    {
                        pauseGame();
                    }
                } else
                {
                    if (Input.GetKeyUp(KeyCode.Escape))
                    {
                        pauseGame();
                    }
                }
            }
            signalLightsOnOff();
        } 
        else
        {
            if (!Alive)
            {
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    if (LogitechGSDK.LogiButtonReleased(0, 23) || LogitechGSDK.LogiButtonIsPressed(0, 2))
                    {
                        mainMenu();
                    }

                    if (LogitechGSDK.LogiButtonIsPressed(0, 1))
                    {
                        restartGame();
                    }
                }
                else
                {
                    if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.D))
                    {
                        mainMenu();
                    }

                    if (Input.GetKeyUp(KeyCode.A))
                    {
                        restartGame();
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        fade(Booster, BoosterFade);
        fade(Health, HealthFade);

        if (currentHealth <= 0)
        {
            StartCoroutine(playerDeath());
            gameEnd = true;
        }
        else
        {
            endGame(false, "");
        }

        if (enableControl == false)
        {
            foreach (Image i in Glows)
            {
                if (i.fillAmount < 1)
                {
                    i.fillAmount += Time.deltaTime / 5;
                }
            }

            if (boostDecimal < boostTimeLimit)
            {
                boostDecimal += Time.deltaTime;
            }
            else
            {
                boostDecimal = boostTimeLimit;
                canBoost = true;
            }
            boosterBar();

            if (healthStart == false)
            {
                Health.fillAmount += Time.deltaTime / 5;
                if (Health.fillAmount >= 1)
                {
                    healthStart = true;
                }
            }
            else
            {
                Health.fillAmount = currentHealth / maxHealth;
            }

            if (StartTimer.enabled == true)
            {
                if (startTime >= 0.75f)
                {
                    StartTimer.text = Mathf.RoundToInt(startTime).ToString();
                    startTime -= Time.deltaTime;
                    heldTime += Time.deltaTime;
                    if (heldTime < 1.025f)
                    {
                        if (heldTime > 1)
                        {
                            audioSrc[2].PlayOneShot(audioSrc[2].clip);
                        }
                    }
                    else
                    {
                        heldTime = 0f;
                    }
                }
                else
                {
                    if (!audioSrc[3].isPlaying)
                    {
                        audioSrc[3].Play();
                    }
                    StartTimer.text = "GO!";
                    startTime -= Time.deltaTime;
                    setGear(1);
                    if (startTime < 0)
                    {
                        StartTimer.enabled = false;
                        enableControl = true;
                    }
                }
            }
        }
        else
        {
            if (Alive && !gameEnd)
            {
                if (!levelMusic.isPlaying)
                {
                    levelMusic.Play();
                }

                setRobot(currentWeapon);
                HShifter();
                updateCams();
                headLightsOnOff();
                Health.fillAmount = currentHealth / maxHealth;

                float pitchAdd;

                if (currentAcceleration != 0)
                {
                    if (CurrentGear == 2)
                    {
                        pitchAdd = 0.25f;
                    }
                    else if (CurrentGear == 3)
                    {
                        pitchAdd = 0.325f;
                    }
                    else if (CurrentGear == 4)
                    {
                        pitchAdd = 0.5f;
                    }
                    else if (CurrentGear == 5)
                    {
                        pitchAdd = .625f;
                    }
                    else
                    {
                        pitchAdd = 0.125f;
                    }
                }
                else
                {
                    pitchAdd = 0f;
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
                    audioSrc[0].pitch = pitchStart + pitchAdd + 0.25f;
                    particleControl(true);
                }
                else
                {
                    boostStart = true;
                    RobotAnimations[emptyHand].SetBool("blocking", false);
                    audioSrc[0].pitch = pitchStart + pitchAdd;
                    ShieldUpDown.SetBool("shieldUp", false);
                    particleControl(false);
                }

                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                /* If a Logitech steering wheel is connected, give the players steering wheel controls.
                 * If not, then give the regular keyboard controls.
                 */
                {
                    LogitechGSDK.DIJOYSTATE2ENGINES rec;
                    rec = LogitechGSDK.LogiGetStateUnity(0);

                    if (LogitechGSDK.LogiButtonIsPressed(0, 21))
                    {
                        headLightOn = true;
                    }
                    if (LogitechGSDK.LogiButtonIsPressed(0, 22))
                    {
                        headLightOn = false;
                    }

                    float floatValue = 32768f; // simple float variable to save time typing the max float value

                    if (Mathf.Abs(rec.lX) < stopTurnForce) {
                        if (LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_CONSTANT))
                        {
                            LogitechGSDK.LogiStopConstantForce(0);
                        }
                    } 
                    else
                    {
                        if (rec.lX < -stopTurnForce)
                        {
                            LogitechGSDK.LogiPlayConstantForce(0, -wheelConstant);
                        }
                        if (rec.lX > stopTurnForce)
                        {
                            LogitechGSDK.LogiPlayConstantForce(0, wheelConstant);
                        }
                    }

                    xAxes = rec.lX / floatValue;

                    if (currentWeapon < emptyHand)
                    {
                        if (LogitechGSDK.LogiButtonIsPressed(0, 5))
                        {
                            heldTime += Time.deltaTime;
                            robotAttack(currentWeapon, 2);
                        }
                        else
                        {
                            heldTime = 0f;
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
                        if (LogitechGSDK.LogiButtonIsPressed(0, 4))
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
                    boosterBar();

                    if (rec.lY > 0)
                    {
                        GasInput = 0f;
                        slowDown();
                    }
                    else if (rec.lY < 0)
                    {
                        GasInput = rec.lY / -floatValue;
                        if (Mathf.Abs(currentAcceleration) < (acceleration * CurrentGear * GasInput))
                        {
                            currentAcceleration += acceleration * CurrentGear * GasInput * boostMult;
                        }
                        else
                        {
                            currentAcceleration = acceleration * CurrentGear * GasInput * boostMult;
                        }
                    }

                    if (rec.lRz > 0)
                    {
                        BrakeInput = 0f;
                        currentBrakingForce = 0f;
                        lightOnOff(1, breakLights, 0, false);
                    }
                    else if (rec.lRz < 0)
                    {
                        BrakeInput = rec.lRz / -floatValue;
                        if (CurrentGear == -1)
                        {
                            currentBrakingForce = 2 * brakingForce;
                        }
                        else
                        {
                            currentBrakingForce = 2 * brakingForce * CurrentGear;
                        }
                        lightOnOff(1, breakLights, breakIntensity, true);
                    }

                    if (rec.rglSlider[0] > 0)
                    {
                        ClutchInput = 0f;
                    }
                    else if (rec.rglSlider[0] < 0)
                    {
                        ClutchInput = rec.rglSlider[0] / -floatValue;
                    }

                    UpdateTorque();
                    currentTurnAngle = maxTurnAngle * xAxes;
                    UpdateWheels();
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        headLightOn = !headLightOn;
                    }

                    if (currentWeapon < emptyHand)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            heldTime += Time.deltaTime;
                            robotAttack(currentWeapon, 2);
                        }
                        else
                        {
                            heldTime = 0f;
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
                        if (Input.GetMouseButton(1))
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
                    boosterBar();

                    if (Input.GetAxis("Vertical") != 0)
                    {
                        if (Mathf.Abs(currentAcceleration) < (acceleration * CurrentGear * boostMult))
                        {
                            currentAcceleration += acceleration * CurrentGear * Input.GetAxis("Vertical") * boostMult;
                        }
                        else
                        {
                            currentAcceleration = acceleration * CurrentGear * Input.GetAxis("Vertical") * boostMult;
                        }
                    }
                    else
                    {
                        slowDown();
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        if (CurrentGear == -1)
                        {
                            currentBrakingForce = 2 * brakingForce;
                        }
                        else
                        {
                            currentBrakingForce = 2 * brakingForce * CurrentGear;
                        }
                        lightOnOff(1, breakLights, breakIntensity, true);
                        if (currentAcceleration != 0 && braking == false)
                        {
                            audioSrc[1].Play();
                            braking = true;
                        }
                    }
                    else
                    {
                        currentBrakingForce = 0f;
                        lightOnOff(1, breakLights, 0, false);
                        audioSrc[1].Stop();
                        braking = false;
                    }

                    UpdateTorque();
                    currentTurnAngle = maxTurnAngle * Input.GetAxis("Horizontal");
                    UpdateWheels();
                }
                if (currentAcceleration < 0)
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
    
    void boosterBar()
    {
        Booster.fillAmount = boostDecimal / boostTimeLimit;
    }

    void UpdateWheel(WheelCollider col, Transform trans)
    {
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);

        trans.position = position;
        trans.rotation = rotation;
    }

    private void UpdateTorque()
    {
        // Update the speed of the wheels
        frontRight.motorTorque = currentAcceleration;
        frontLeft.motorTorque = currentAcceleration;

        frontRight.brakeTorque = currentBrakingForce;
        frontLeft.brakeTorque = currentBrakingForce;
        backRight.brakeTorque = currentBrakingForce;
        backLeft.brakeTorque = currentBrakingForce;
    }

    void UpdateWheels()
    {
        // Update front wheel steering angle and wheel transforms
        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        UpdateWheel(frontLeft, wheelTransforms[0]);
        UpdateWheel(frontRight, wheelTransforms[1]);
        UpdateWheel(backLeft, wheelTransforms[2]);
        UpdateWheel(backRight, wheelTransforms[3]);
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
        else if(CurrentGear == 4)
        {
            renderList[0].enabled = false;
            renderList[1].enabled = false;
            renderList[2].enabled = false;
            renderList[3].enabled = true;
            renderList[4].enabled = false;
        }
        else if(CurrentGear == 5)
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

    void updateCams()
    {
        if (Alive && !gameEnd)
        {
            if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
            {
                // change camera output based on Logitech input
                if (LogitechGSDK.LogiButtonIsPressed(0, 1))
                {
                    enableCams(false, false, true, false);
                }
                else if (LogitechGSDK.LogiButtonIsPressed(0, 2))
                {
                    enableCams(false, true, false, false);
                }
                else if (LogitechGSDK.LogiButtonIsPressed(0, 0) || LogitechGSDK.LogiButtonIsPressed(0, 3))
                {
                    enableCams(false, false, false, true);
                }
                else
                {
                    enableCams(true, false, false, false);
                }
            }
            else
            {
                // change camera output based on keyboard input
                if (Input.GetKey(KeyCode.Q))
                {
                    enableCams(false, true, false, false);
                    if (Input.GetKey(KeyCode.E))
                    {
                        enableCams(false, false, false, true);
                    }
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    enableCams(false, false, true, false);
                    if (Input.GetKey(KeyCode.Q))
                    {
                        enableCams(false, false, false, true);
                    }
                }
                else
                {
                    enableCams(true, false, false, false);
                }
            }
            updateCamFOV(cameras[0]);
            updateCamFOV(cameras[3]);
        }
    }

    void updateCamFOV(Camera cam)
    {
        float desiredFOV;

        if (currentAcceleration > 0)
        {
            if (CurrentGear == 2)
            {
                desiredFOV = 39f;
            }
            else if (CurrentGear == 3)
            {
                desiredFOV = 42f;
            }
            else if (CurrentGear == 4)
            {
                desiredFOV = 48f;
            }
            else if (CurrentGear == 5)
            {
                desiredFOV = 54f;
            }
            else
            {
                desiredFOV = 36f;
            }
        }
        else
        {
            desiredFOV = 36f;
        }

        if (boostMult == 2)
        {
            desiredFOV += 3f;
        }
        
        if (cam.fieldOfView < desiredFOV)
        {
            cam.fieldOfView += zoomMult;
        }
        else if (cam.fieldOfView > desiredFOV)
        {
            cam.fieldOfView -= zoomMult;
        }
        else
        {
            cam.fieldOfView = desiredFOV;
        }
    }

    // just to help simplify updating the cameras
    void enableCams(bool Main, bool Left, bool Right, bool Back)
    {
        cameras[0].enabled = Main;
        cameras[1].enabled = Left;
        cameras[2].enabled = Right;
        cameras[3].enabled = Back;
    }

    void HShifter()
    {
        if (MainMenu.auto == true)
        {
            if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
            {
                if (ClutchInput > 0.5f)
                {
                    if (LogitechGSDK.LogiButtonIsPressed(0, 12) || LogitechGSDK.LogiButtonIsPressed(0, 14) || LogitechGSDK.LogiButtonIsPressed(0, 16))
                    {
                        if (CurrentGear < 5)
                        {
                            setGear(CurrentGear + 1);
                        }
                        else
                        {
                            setGear(5);
                        }
                    } 
                    else if (LogitechGSDK.LogiButtonIsPressed(0, 13) || LogitechGSDK.LogiButtonIsPressed(0, 15) || LogitechGSDK.LogiButtonIsPressed(0, 17))
                    {
                        if (CurrentGear > 0)
                        {
                            setGear(CurrentGear - 1);
                        }
                        else
                        {
                            setGear(0);
                        }
                    }
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                        if (CurrentGear < 5)
                        {
                            setGear(CurrentGear + 1);
                        }
                        else
                        {
                            setGear(5);
                        }
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                        if (CurrentGear > 0)
                        {
                            setGear(CurrentGear - 1);
                        }
                        else
                        {
                            setGear(0);
                        }
                    }
                }
            }
        }
        else
        {
            if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
            {
                // Gear shift input for steering wheel
                if (ClutchInput > 0.5f)
                {
                    if (LogitechGSDK.LogiButtonIsPressed(0, 12))
                    {
                        setGear(1);
                    }
                    else if (LogitechGSDK.LogiButtonIsPressed(0, 13))
                    {
                        setGear(2);
                    }
                    else if (LogitechGSDK.LogiButtonIsPressed(0, 14))
                    {
                        setGear(3);
                    }
                    else if (LogitechGSDK.LogiButtonIsPressed(0, 15))
                    {
                        setGear(4);
                    }
                    else if (LogitechGSDK.LogiButtonIsPressed(0, 16))
                    {
                        setGear(5);
                    }
                    else if (LogitechGSDK.LogiButtonIsPressed(0, 17))
                    {
                        setGear(-1);
                    }
                    else if (LogitechGSDK.LogiButtonReleased(0, 12) || LogitechGSDK.LogiButtonReleased(0, 13) || LogitechGSDK.LogiButtonReleased(0, 14) ||
                        LogitechGSDK.LogiButtonReleased(0, 15) || LogitechGSDK.LogiButtonReleased(0, 16) || LogitechGSDK.LogiButtonReleased(0, 17))
                    {
                        setGear(0);
                    }
                }
            }
            else
            {
                // Gear shift input for keyboard input
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (ClutchInput < 1)
                    {
                        ClutchInput += .1f;
                    }
                    else
                    {
                        ClutchInput = 1;
                    }
                    if (ClutchInput > 0.5f)
                    {
                        if (Input.GetKey(keyInputs[0]))
                        {
                            setGear(1);
                        }
                        else if (Input.GetKey(keyInputs[1]))
                        {
                            setGear(2);
                        }
                        else if (Input.GetKey(keyInputs[2]))
                        {
                            setGear(3);
                        }
                        else if (Input.GetKey(keyInputs[3]))
                        {
                            setGear(4);
                        }
                        else if (Input.GetKey(keyInputs[4]))
                        {
                            setGear(5);
                        }
                        else if (Input.GetKey(keyInputs[5]))
                        {
                            setGear(-1);
                        }
                        else
                        {
                            setGear(0);
                        }
                    }
                }
                else
                {
                    if (ClutchInput > 0)
                    {
                        ClutchInput -= .1f;
                    }
                    else
                    {
                        ClutchInput = 0;
                    }
                }
            }
        }
    }
    void setGear(int gear)
    {
        if (gear != 0)
        {
            CurrentGear = gear;
            currentAcceleration -= 50;
            if (gear == -1)
            {
                Gear.text = "R";
            } else
            {
                Gear.text = gear.ToString();
            }
        }
        else
        {
            CurrentGear = 0;
            Gear.text = "N";
        }
        setWheelVisibilities();
    }
}