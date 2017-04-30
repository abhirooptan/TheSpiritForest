using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets.Utility;

public class PlayerScript : NetworkBehaviour
{
    public enum PlayerType { Knight, Warrior, Swordsman};
    public PlayerType playerType;

    public float level1Damage;
    public float level2Damage;
    public float level3Damage;

    // this variable is accessed in the SwordDamage script 
    [HideInInspector] public float makeDamage;
    [HideInInspector] public bool isBlocking;

    public bool isInteracting;

    public NetworkAnimator anim;
    private MeshCollider[] weapons;

    public MeshCollider[] Weapons
    {
        get { return weapons; }
    }

    float speed = 7.0f;

    Camera cam;

    float mouseClickTime = 0.0f;
    int mouseClickCount = 0;
    float mouseClickCharge = 0;

    public void Start()
    {
        cam = Camera.main;
        //base.OnStartClient();
        NetworkIdentity nIdentity = GetComponent<NetworkIdentity>();
        if (nIdentity.isLocalPlayer)
        {   
            //if I am the owner of this prefab
            SmoothFollowCustom script = cam.GetComponent<SmoothFollowCustom>();
            script.setTarget(transform);

            cam.GetComponent<MouseLooker>().enabled = true;
            

            weapons = GetComponentsInChildren<MeshCollider>();

            foreach (MeshCollider weapon in weapons)
            {
                weapon.enabled = false;
            }

            anim = GetComponent<NetworkAnimator>();
        }        
    }

    void Update()
    {
        mouseClickTime += Time.deltaTime;

        if (!isLocalPlayer)
        {
            return;
        }

        //Get input from controls
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        PlayerMovement(x, z);

        // ====================================================================
        // PLAYER ROLL
        // ====================================================================
        if (Input.GetButtonDown("Fire3"))
        {
            anim.animator.SetBool("Roll", true);
            anim.animator.SetFloat("Input X", x);
            anim.animator.SetFloat("Input Z", z);
        }
        // ====================================================================


        // ====================================================================
        // PLAYER DEFENCE
        // ====================================================================
        if (Input.GetButtonDown("Fire2") && !isInteracting)
        {
            anim.animator.SetBool("Block", true);
            isBlocking = true;
        }

        else if (Input.GetButtonUp("Fire2") && !isInteracting)
        {
            anim.animator.SetBool("Block", false);
            isBlocking = false;
        }
        // ====================================================================


        // ====================================================================
        // PLAYER ATTACK
        // ====================================================================
        if (Input.GetButton("Fire1") && !isInteracting)
        {
            mouseClickCharge++;
            //print(mouseClickCharge);

            if (mouseClickCharge > 10)
                anim.animator.SetBool("Charging", true);

        }

        if (Input.GetButtonUp("Fire1") && !isInteracting)
        {
            if (mouseClickCharge > 50)
            {
                mouseClickCharge = 0;
                anim.animator.SetBool("Level3", true);
                makeDamage = level3Damage;
            }
            else
            {
                mouseClickCharge = 0;

                if (mouseClickCount == 0)
                {
                    anim.animator.SetBool("Level1", true);
                    mouseClickTime = 0;
                    mouseClickCount++;
                    makeDamage = level1Damage;
                }

                if (mouseClickCount == 1)
                {
                    if (mouseClickTime < 0.2f)
                    {
                        anim.animator.SetBool("Level2", true);
                        mouseClickCount = 0;
                        makeDamage = level2Damage;
                    }
                    else
                    {
                        anim.animator.SetBool("Level1", true);
                        mouseClickCount = 0;
                        makeDamage = level1Damage;
                    }
                }
            }

            anim.animator.SetBool("Charging", false);
            //print("Mouse Counter " + mouseClickCount + " Mouse Time " + mouseClickTime);
        }

        if (Input.GetButtonUp("Fire4") && !isInteracting)
        {
            anim.animator.SetBool("Level2", true);
            mouseClickCount = 0;
            makeDamage = level2Damage;
        }

            if (mouseClickTime > 0.5f)
        {
            mouseClickCount = 0;
        }
        // ====================================================================        
    }

    public IEnumerator COStunPause(float pauseTime)
    {
        yield return new WaitForSeconds(pauseTime);
    }

    void PlayerMovement(float x, float y)
    {
        //Apply inputs to anim
        anim.animator.SetFloat("Input X", x);
        anim.animator.SetFloat("Input Z", y);

        if (y > 0 && x == 0)
            speed = 9;
        else
            speed = 7;

        Vector3 MouseWorldPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.LookAt(MouseWorldPosition);
        transform.rotation = Quaternion.Euler(new Vector3(0, 180 + transform.rotation.eulerAngles.y, 0));
    }
}