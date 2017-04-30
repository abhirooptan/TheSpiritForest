using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Boss : NetworkBehaviour {

    public Transform bulletSpawn;
    public GameObject bullet;
    public GameObject blast;
    public GameObject blastWave;
    public GameObject enemyToSpawn;
    public GameObject teleportEffect;
    public GameObject morphLight;
    public GameObject tree;
    public GameObject flowchart;
    public Transform[] teleportPos;

    bool isEnemySpawning;
    bool teleportIn;
    bool teleportOut;
    GameObject enemySpawned;
    float rate;

    int randPos;
    [SerializeField] private float timeToTeleport;
    float tempTimeToTeleport;

    private enum MageLevel { level0, level1, level2, level3, level4};
    [SerializeField] private MageLevel mageLevel;

    public enum MageState { doNothing, idle, attack, hit, teleport, fury, dead };
    public MageState mageState;
    
    public bool canTakeDamage;

    private Health healthScript;
    private NetworkAnimator anim;

    bool isMorphing;
    bool isPlayerMorphing;

    public float shakeTimer;
    public float shakeAmount;
    float shakeAmountTemp;

    GameObject cam;
    AudioSource earthquake;

    private GameObject player;
    private LayerMask raycastLayer;
    [SerializeField] private float radius = 50;

    [SerializeField] private float nextAttack1Time = 5.0f;
    private float attack1Timer = 3;

    [SerializeField] private float nextAttack2Time = 15.0f;
    private float attack2Timer = 10;

    [SerializeField]
    private float nextAttack3Time = 20.0f;
    private float attack3Timer = 12;

    // Use this for initialization
    void Start()
    {
        if (blast == null)
            blast = GameObject.Find("Attack1Blast");
        if (blastWave == null)
            blastWave = GameObject.Find("Attack2BlastWave");

        cam = GameObject.FindGameObjectWithTag("MainCamera");
        earthquake = GetComponent<AudioSource>();

        isEnemySpawning = false;
        teleportIn = false;
        teleportOut = false;
        canTakeDamage = true;
        //hit = false;
        rate = 0;

        //mageLevel = MageLevel.level1;
        mageState = MageState.idle;
        healthScript = GetComponent<Health>();

        morphLight.GetComponent<Light>().intensity = 0;
        morphLight.SetActive(false);
        //tree.transform.localScale.Set(0.1f, 0.1f, 0.1f);
        tree.SetActive(false);
        flowchart.gameObject.SetActive(false);
        anim = GetComponent<NetworkAnimator>();

        raycastLayer = 1 << LayerMask.NameToLayer("Player");
    }

    void FixedUpdate()
    {
        if (!isServer)
            return;

        HealthCheck();

        //===============================================================
        // Boss AI Below
        //===============================================================
        switch (mageState)
        {
            case MageState.doNothing:
                break;
            case MageState.idle:
                SearchForTarget();
                break;
            case MageState.attack:
                transform.LookAt(player.transform.position);
                Attack();
                break;
            case MageState.hit:
                CmdHit();
                break;
            case MageState.teleport:
                CmdTeleport() ;
                tempTimeToTeleport = 0;
                break;
            case MageState.fury:
                break;
            case MageState.dead:
                CmdEndScene();
                break;
        }
        //===============================================================

        if (isEnemySpawning)
        {
            if (rate <= 1.0f)
            {
                rate += Time.deltaTime;
                enemySpawned.transform.localScale = new Vector3(rate, rate, rate);
            }
            else
            {
                enemySpawned.GetComponent<Enemy>().enabled = true;
                isEnemySpawning = false;
                rate = 0;
            }
        }

        if (teleportIn) {
            transform.localScale = Vector3.Slerp(transform.localScale, Vector3.zero, Time.deltaTime*10);            
        }

        if (teleportOut) {
            transform.localScale = Vector3.Slerp(transform.localScale, new Vector3(1.25f, 1.25f, 1.25f), Time.deltaTime*10);
        }

        // =======================================================================================
        // death related stuff DO NOT TOUCH
        // =======================================================================================
        if (isMorphing) {
            morphLight.GetComponent<Light>().intensity += 0.02f;
            
        }

        if (isPlayerMorphing) {
            tree.transform.localScale = Vector3.Slerp(tree.transform.localScale, new Vector3(1.0f, 1.0f, 1.0f), Time.deltaTime * 0.2f);
            morphLight.GetComponent<Light>().intensity -= 0.01f;
        }

        if (shakeTimer >= 0)
        {
            if (shakeTimer >= 10)
            {
                earthquake.volume += 0.006f;
                shakeAmountTemp += 0.0005f;
            }
            if (shakeTimer > 5 && shakeTimer < 10)
            {
                earthquake.volume = 1;
                shakeAmountTemp = shakeAmount;
            }
            if (shakeTimer < 5)
            {
                if(earthquake.volume > 0)
                    earthquake.volume -= 0.006f;
                shakeAmountTemp -= 0.0005f;
            }
            
            Vector2 shakePos = Random.insideUnitCircle * shakeAmountTemp;
            cam.transform.position = new Vector3(cam.transform.position.x + shakePos.x, cam.transform.position.y + shakePos.y, cam.transform.position.z);

            shakeTimer -= Time.deltaTime;
        }
        // =======================================================================================
    }

    public void HealthCheck()
    {
        if (healthScript.currentHealth > 75)
            mageLevel = MageLevel.level1;
        else if (healthScript.currentHealth <= 75 && healthScript.currentHealth > 50)
            mageLevel = MageLevel.level2;
        else if (healthScript.currentHealth <= 50 && healthScript.currentHealth > 25)
            mageLevel = MageLevel.level3;
        else if (healthScript.currentHealth <= 25 && healthScript.currentHealth > 1)
            mageLevel = MageLevel.level4;
        else
            mageLevel = MageLevel.level0;
    }

    public void SearchForTarget()
    {
        if (player == null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, raycastLayer);

            if (hitColliders.Length > 0)
            {
                int randomInt = Random.Range(0, hitColliders.Length);
                player = hitColliders[randomInt].gameObject;
                mageState = MageState.attack;
                //print("player found by " + name);
            }

            else
            {
                mageState = MageState.idle;
            }
        }

        if (player != null && player.GetComponent<CharacterController>().enabled == false)
        {
            player = null;
            mageState = MageState.idle;
            //print("blooped by " + name);
        }

        //yield return new WaitForSeconds(0.2f);
    }

    public void Attack()
    {
        switch (mageLevel)
        {
            case MageLevel.level0:
                break;

            case MageLevel.level1:
                attack1Timer += Time.deltaTime;
                if (attack1Timer > nextAttack1Time)
                {
                    CmdAttack1();
                    attack1Timer = 0;
                }
                break;

            case MageLevel.level2:
                attack2Timer += Time.deltaTime;
                attack1Timer += Time.deltaTime;
                if (attack1Timer > nextAttack1Time)
                {
                    if (attack2Timer > nextAttack2Time)
                    {
                        // spawn enemy
                        CmdAttack3();
                        attack2Timer = 0;
                        attack1Timer = 0;
                    }
                    else
                    {
                        CmdAttack1();
                        attack1Timer = 0;
                    }
                }
                break;

            case MageLevel.level3:
                attack3Timer += Time.deltaTime;
                attack2Timer += Time.deltaTime;
                attack1Timer += Time.deltaTime;
                if (attack1Timer > nextAttack1Time)
                {
                    if (attack2Timer > nextAttack2Time)
                    {
                        if (attack3Timer > nextAttack3Time)
                        {
                            CmdAttack4(); ;
                            attack3Timer = 0;
                            attack2Timer = 0;
                            attack1Timer = 0;
                        }
                        else
                        {
                            // spawn enemy
                            CmdAttack3();
                            attack2Timer = 0;
                            attack1Timer = 0;
                        }
                    }
                    else
                    {
                        CmdAttack1();
                        attack1Timer = 0;
                    }
                }
                break;

            case MageLevel.level4:
                tempTimeToTeleport += Time.deltaTime;
                if (tempTimeToTeleport >= timeToTeleport)
                {
                    mageState = MageState.teleport;
                }

                attack3Timer += Time.deltaTime;
                attack2Timer += Time.deltaTime;
                attack1Timer += Time.deltaTime;
                if (attack1Timer > nextAttack1Time)
                {
                    if (attack2Timer > nextAttack2Time)
                    {
                        if (attack3Timer > nextAttack3Time)
                        {
                            CmdAttack4();
                            attack3Timer = 0;
                            attack2Timer = 0;
                            attack1Timer = 0;
                        }
                        else
                        {
                            // spawn enemy
                            CmdAttack3();
                            attack2Timer = 0;
                            attack1Timer = 0;
                        }
                    }
                    else
                    {
                        CmdAttack1();
                        attack1Timer = 0;
                    }
                }
                break;
        }
    }

    public IEnumerator fury()
    {
        RpcSetTrigger("Fury");
        yield return null;
    }

    public IEnumerator hit()
    {
        canTakeDamage = false;
        if (healthScript.currentHealth > 1)
        {
            mageState = MageState.doNothing;
            RpcSetTrigger("Hit");
            yield return new WaitForSeconds(1.0f);
            mageState = MageState.teleport;
        }
        else
        {
            mageState = MageState.dead;
        }
    }

    public IEnumerator teleport()
    {
        mageState = MageState.doNothing;

        canTakeDamage = false;

        RpcSetTrigger("Teleport");
        yield return new WaitForSeconds(1.5f);
        int temp = Random.Range(0, 8);

        while (randPos == temp)
        {
            temp = Random.Range(0, 8);
        }

        randPos = temp;

        // make it vanish
        teleportIn = true;
        var teleportEffectSpawn = (GameObject)Instantiate(teleportEffect, transform.position, transform.rotation);
        
        yield return new WaitForSeconds(1.0f);
        Destroy(teleportEffectSpawn);

        transform.position = teleportPos[randPos].position;
        teleportIn = false;

        // make it appear
        teleportOut = true;
        yield return new WaitForSeconds(1.0f);
        teleportOut = false;

        canTakeDamage = true;

        // look for a random player after every teleport
        player = null;
        mageState = MageState.idle;

        yield return null;
    }

    public IEnumerator attack1()
    {
        RpcSetTrigger("Attack1Trigger");

        yield return new WaitForSeconds(0.7f);

        bulletSpawn.LookAt(player.transform);
        GameObject blast = Instantiate(bullet, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        blast.GetComponent<Rigidbody>().velocity = transform.forward * 15;
        NetworkServer.Spawn(blast);
        Destroy(blast, 6.0f);

        player = null;
        mageState = MageState.idle;
    }

    public IEnumerator attack2()
    {
        RpcSetTrigger("Attack2Trigger");

        yield return new WaitForSeconds(1.3f);

        GameObject blastWaveSpawned = Instantiate(blastWave, bulletSpawn.transform.position, transform.rotation);
        NetworkServer.Spawn(blastWaveSpawned);
        Destroy(blastWaveSpawned, 6.0f);

        player = null;
        mageState = MageState.idle;
    }

    public IEnumerator attack3()
    {
        RpcSetTrigger("Attack3Trigger");

        yield return new WaitForSeconds(0.5f);
        Transform closestWaypoint = teleportPos[0];
        foreach (Transform waypoint in teleportPos)
        {
            if (Vector3.Distance(transform.position, waypoint.position) < Vector3.Distance(transform.position, closestWaypoint.position))
                closestWaypoint = waypoint;
        }
        enemySpawned = Instantiate(enemyToSpawn, closestWaypoint.position, transform.rotation);
        enemySpawned.transform.localScale = new Vector3(0, 0, 0);
        enemySpawned.GetComponent<Enemy>().enabled = false;
        enemySpawned.GetComponent<Enemy>().waypoints = teleportPos;
        enemySpawned.GetComponent<Enemy>().enemyState = Enemy.EnemyState.follow;
        NetworkServer.Spawn(enemySpawned);
        isEnemySpawning = true;

        player = null;
        mageState = MageState.idle;
    }

    public IEnumerator attack4()
    {
        int numberOfBullets = 7;
        GameObject[] bullets = new GameObject[numberOfBullets];
        //GameObject[] blasts = new GameObject[numberOfBullets];

        RpcSetTrigger("Attack4Trigger");
        for (int i = 0; i < numberOfBullets; i++)
        {
            Vector3 localTransform = bulletSpawn.position;
            //localTransform.z += 1.0f;

            bullets[i] = Instantiate(bullet, localTransform, transform.rotation);
            NetworkServer.Spawn(bullets[i]);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.5f);
        
        for (int i = 0; i < numberOfBullets; i++)
        {
            bullets[i].GetComponent<Rigidbody>().velocity = bullets[i].transform.forward * 15;
            Destroy(bullets[i], 6);
        }

        player = null;
        mageState = MageState.idle;
    }

    public void ShakeCamera(float shakePwr, float shakeDur)
    {
        shakeAmount = shakePwr;
        shakeTimer = shakeDur;
    }

    public IEnumerator death()
    {
        mageState = MageState.doNothing;
        RpcSetTrigger("Death");
        yield return new WaitForSeconds(5.0f);

        morphLight.transform.position = new Vector3(transform.position.x, transform.position.y+2, transform.position.z);
        morphLight.SetActive(true);
        isMorphing = true;

        yield return new WaitForSeconds(5.0f);
        teleportIn = true;

        yield return new WaitForSeconds(2.0f);
        earthquake.Play();
        ShakeCamera(0.1f, 15.0f);
        tree.transform.position = transform.position;
        tree.SetActive(true);
        isPlayerMorphing = true;
        
        isMorphing = false;

        yield return new WaitForSeconds(15.0f);
        isPlayerMorphing = false;
        print("morph close");
        earthquake.Stop();
    }

    // =================================================================
    // RPC METHODS BELOW
    // =================================================================

    [Command]
    public void CmdAttack1()
    {
        StartCoroutine(attack1());
    }

    [Command]
    public void CmdAttack2()
    {
        StartCoroutine(attack2());
    }

    [Command]
    public void CmdAttack3()
    {
        StartCoroutine(attack3());
    }

    [Command]
    public void CmdAttack4()
    {
        StartCoroutine(attack4());
    }

    [Command]
    public void CmdTeleport()
    {
        StartCoroutine(teleport());
    }

    [Command]
    public void CmdHit()
    {
        StartCoroutine(hit());
    }

    [Command]
    public void CmdDeath()
    {
        StartCoroutine(death());
    }

    [Command]
    public void CmdEndScene()
    {
        mageState = MageState.doNothing;
        flowchart.gameObject.SetActive(true);
    }

    [ClientRpc]
    public void RpcSetTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }
    // =================================================================
}
