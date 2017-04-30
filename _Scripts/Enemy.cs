using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour {

    public enum EnemyType { blue, red, purple }
    public EnemyType enemyType;

    public enum EnemyState { patrol, alert, follow, attack }
    public EnemyState enemyState = EnemyState.patrol;

    public GameObject blastPref;
    public GameObject blastWavePref;
    public GameObject enemyPref;
    public GameObject spawnPos;

    public Transform[] waypoints;
    int waypointCounter;

    float radius = 30;
    
    GameObject player;
    Health mageHealth;
    public NetworkAnimator anim;
    NavMeshAgent navAgent;
    float attack1Timer;
    float attack2TimerPurple = 3.0f;
    float attack2TimerRed = 2.0f;
    float attack2TimerBlue = 0.0f;

    GameObject enemy;
    bool isEnemySpawning;
    float rate;

    float timer;

    private LayerMask raycastLayer;

    private SphereCollider[] weapons;

    public SphereCollider[] Weapons
    {
        get { return weapons; }
    }

    // Use this for initialization
    public void Start()
    {
        anim = GetComponent<NetworkAnimator>();

        if (this.GetComponent<Rigidbody>() == null)
        {
            this.gameObject.AddComponent<Rigidbody>();
        }

        if (this.GetComponent<Health>() == null)
        {
            this.gameObject.AddComponent<Health>();
        }

        weapons = GetComponentsInChildren<SphereCollider>();

        foreach (SphereCollider weapon in weapons)
        {
            weapon.enabled = false;
        }

        if (this.GetComponent<NavMeshAgent>() == null)
            this.gameObject.AddComponent<NavMeshAgent>();

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.stoppingDistance = 2;

        raycastLayer = 1 << LayerMask.NameToLayer("Player");
        InvokeRepeating("SearchForTarget", 2.0f, 0.5f);

        //enemyState = EnemyState.patrol;

        if (!isServer)
            return;

        waypointCounter = Random.Range(1, waypoints.Length - 1);
        navAgent.SetDestination(waypoints[waypointCounter].position);
    }

    public override void OnStartServer()
    {
        mageHealth = GameObject.FindGameObjectWithTag("Mage").GetComponent<Health>();
    }

    // Update is called once per frame
    void FixedUpdate () {

        if (!isServer)
            return;

        if (mageHealth.currentHealth < 1)
            GetComponent<Health>().TakeDamage(100);

        // =====================================================================
        // only for enemy spawn attack
        // =====================================================================
        if (isEnemySpawning)
        {
            if (rate <= 1.0f)
            {
                rate += Time.deltaTime;
                enemy.transform.localScale = new Vector3(rate, rate, rate);
            }
            else
            {
                enemy.GetComponent<Enemy>().enabled = true;
                isEnemySpawning = false;
                rate = 0;
            }
        }
        // =====================================================================

        // =====================================================================
        // Enemy AI
        // =====================================================================
        switch (enemyState)
        {
            case EnemyState.patrol:
                CmdPatrol();
                break;
            case EnemyState.alert:
                Alert();
                break;
            case EnemyState.follow:
                CmdFollow();
                break;
            case EnemyState.attack:
                CmdAttack();
                break;
        }
        // =====================================================================
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
                enemyState = EnemyState.follow;
                //print("player found by " + name);
            }
        }

        if (player != null && player.GetComponent<CharacterController>().enabled == false)
        {
            player = null;
            //print("blooped by " + name);
            waypointCounter = Random.Range(1, waypoints.Length - 1);
            navAgent.SetDestination(waypoints[waypointCounter].position);
            enemyState = EnemyState.patrol;
        }

        //yield return new WaitForSeconds(0.2f);
    }

    [Command]
    public void CmdPatrol()
    {
        navAgent.speed = 2;
        anim.animator.SetBool("Running", false);
        anim.animator.SetBool("Patrol", true);
        if (Vector3.Distance(transform.position, waypoints[waypointCounter].position) <= navAgent.stoppingDistance)
        {
            anim.animator.SetBool("Idle", true);
            timer += Time.deltaTime;
            if(timer > 8)
            {
                waypointCounter = Random.Range(1, waypoints.Length - 1);
                navAgent.SetDestination(waypoints[waypointCounter].position);
                anim.animator.SetBool("Idle", false);
                timer = 0;
            }
        }
    }

    public void Alert()
    {
        navAgent.speed = 0;
        anim.animator.SetBool("Running", false);
        anim.animator.SetBool("Patrol", false);
        anim.animator.SetBool("Idle", true);
        print("Alert");
    }

    [Command]
    public void CmdFollow()
    {
        navAgent.speed = 4;
        navAgent.SetDestination(player.transform.position);
        
        if (Vector3.Distance(transform.position, player.transform.position) > navAgent.stoppingDistance)
        {
            //Debug.Log("Running");
            anim.animator.SetBool("Running", true);
            anim.animator.SetBool("Patrol", false);
            anim.animator.SetBool("Idle", false);
        }
        else
        {
            enemyState = EnemyState.attack;
        }

        switch (enemyType)
        {
            case EnemyType.purple:
                navAgent.stoppingDistance = 10;
                break;
            case EnemyType.red:
                navAgent.stoppingDistance = 15;
                break;
            case EnemyType.blue:
                navAgent.stoppingDistance = 30;
                break;
        }
    }

    [Command]
    public void CmdAttack()
    {
        anim.animator.SetBool("Running", false);
        anim.animator.SetBool("Patrol", false);
        anim.animator.SetBool("Idle", true);
        transform.LookAt(player.transform.position);
        if(Vector3.Distance(transform.position, player.transform.position) < 3)
        {
            attack1Timer += Time.deltaTime;
            if (attack1Timer > 2.0f)
            {
                //Debug.Log("attack now");
                RpcSetTrigger("Attack1Trigger");

                attack1Timer = 0;
            }
        }
        
        else if(Vector3.Distance(transform.position, player.transform.position) >= 3 
            && Vector3.Distance(transform.position, player.transform.position) <= navAgent.stoppingDistance*3)
        {
            

            switch (enemyType)
            {
                case EnemyType.blue:
                    attack2TimerBlue += Time.deltaTime;
                    if (attack2TimerBlue > 8.0f)
                    {
                        //Debug.Log("attack 2 blue");
                        StartCoroutine(PerformEnemySpawn());
                        attack2TimerBlue = 0.0f;
                    }
                    break;
                case EnemyType.red:
                    attack2TimerRed += Time.deltaTime;
                    if (attack2TimerRed > 6.0f)
                    {
                        //Debug.Log("attack 2 red");
                        StartCoroutine(PerformAttackBlastWave());
                        attack2TimerRed = 0.0f;
                    }
                    break;
                case EnemyType.purple:
                    attack2TimerPurple += Time.deltaTime;
                    if (attack2TimerPurple > 4.0f)
                    {
                        //Debug.Log("attack 2 purple");
                        StartCoroutine(PerformAttackBlast());
                        attack2TimerPurple = 0.0f;
                    }
                    break;
            }
        }

        else if (Vector3.Distance(transform.position, player.transform.position) > navAgent.stoppingDistance*3)
        {
            enemyState = EnemyState.follow;
        }
    }

    public IEnumerator PerformAttackBlast()
    {
        RpcSetTrigger("Attack2Trigger");

        yield return new WaitForSeconds(0.5f);

        GameObject blast = Instantiate(blastPref, spawnPos.transform.position, transform.rotation);
        blast.GetComponent<Rigidbody>().velocity = transform.forward * 15;
        NetworkServer.Spawn(blast);
        Destroy(blast, 6.0f);
    }

    public IEnumerator PerformAttackBlastWave()
    {
        RpcSetTrigger("Attack2Trigger");

        yield return new WaitForSeconds(1.3f);

        GameObject blastWave = Instantiate(blastWavePref, spawnPos.transform.position, transform.rotation);
        NetworkServer.Spawn(blastWave);
        Destroy(blastWave, 3);
    }

    public IEnumerator PerformEnemySpawn()
    {
        RpcSetTrigger("Attack2Trigger");

        yield return new WaitForSeconds(0.5f);
        Transform closestWaypoint = waypoints[0];
        foreach(Transform waypoint in waypoints)
        {
            if (Vector3.Distance(transform.position, waypoint.position) < Vector3.Distance(transform.position, closestWaypoint.position))
                closestWaypoint = waypoint;
        }
        enemy = Instantiate(enemyPref, closestWaypoint.position, transform.rotation);
        enemy.transform.localScale = new Vector3(0, 0, 0);
        enemy.GetComponent<Enemy>().enabled = false;
        enemy.GetComponent<Enemy>().waypoints = waypoints;
        //enemy.GetComponent<Enemy>().player = player;
        enemy.GetComponent<Enemy>().enemyState = EnemyState.patrol;
        NetworkServer.Spawn(enemy);
        isEnemySpawning = true;
    }

    [ClientRpc]
    public void RpcSetTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }
}
