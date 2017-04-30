using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour
{

    public const int maxHealth = 100;
    public bool destroyOnDeath;

    [SyncVar(hook = "OnChangeHealth")]
    public float currentHealth = maxHealth;

    public RectTransform healthBar;
    public GameObject playerDeathEffect;

    private NetworkStartPosition[] spawnPoints;

    PlayerScript player;

    void Start()
    {
        if (isLocalPlayer)
        {
            spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        }
    }

    [Command]
    public void CmdDamagePlayer(float amount)
    {
        print("damage before");
        TakeDamage(amount);
        print("damage after");
    }

    //[ClientRpc]
    //public void RpcDamagePlayer(float amount)
    //{
    //    TakeDamage(amount);
    //}

    public void TakeDamage(float amount)
    {
        print("damage inside");
        player = GetComponent<PlayerScript>();

        if (gameObject.CompareTag("Player"))
        {
            switch (player.playerType)
            {
                case PlayerScript.PlayerType.Knight:
                    if (player.isBlocking)
                        currentHealth -= amount / 10;
                    else
                        currentHealth -= amount / 2;
                    break;
                case PlayerScript.PlayerType.Warrior:
                    if (player.isBlocking)
                        currentHealth -= amount / 5;
                    else
                        currentHealth -= amount;
                    break;
                case PlayerScript.PlayerType.Swordsman:
                    if (player.isBlocking)
                        currentHealth -= amount / 2;
                    else
                        currentHealth -= amount;
                    break;
            }
        }

        else if (gameObject.CompareTag("Mage"))
        {
            gameObject.GetComponent<Boss>().mageState = Boss.MageState.hit;
            currentHealth -= amount;
        }

        else
        {
            currentHealth -= amount;
        }

        //print("Health " + currentHealth);

        if (currentHealth <= 0 && !gameObject.CompareTag("Mage"))
        {
            Vector3 positionToSpawnPlayerDeath = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            var deathEffect = (GameObject)Instantiate(playerDeathEffect, positionToSpawnPlayerDeath, transform.rotation);
            NetworkServer.Spawn(deathEffect);
            Destroy(deathEffect, 1.0f);

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
                player.enabled = false;
                GetComponent<CharacterController>().enabled = false;
                GetComponent<Animator>().enabled = false;
                foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
                    mesh.enabled = false;
                //currentHealth = maxHealth;

                // called on the Server, invoked on the Clients
                //RpcRespawn();
            }
        }

        else if (currentHealth > 100)
            currentHealth = 100;
    }

    void OnChangeHealth(float currentHealth)
    {
        healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            // Set the spawn point to origin as a default value
            Vector3 spawnPoint = Vector3.zero;

            // If there is a spawn point array and the array is not empty, pick one at random
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
            }

            // Set the player’s position to the chosen spawn point
            transform.position = spawnPoint;
        }
    }
}