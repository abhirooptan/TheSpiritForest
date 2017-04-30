using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SwordDamage : NetworkBehaviour
{
    public GameObject bloodEffect;

    PlayerScript player;

    void Start()
    {
        //Debug.Log("Sword Initialized");
        player = GetComponentInParent<PlayerScript>();
        if (GetComponent<NetworkIdentity>() == null)
            gameObject.AddComponent<NetworkIdentity>();
    }

    public void OnTriggerEnter(Collider other)
    {
        var hit = other.gameObject;
        if (hit.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Dont hit me i am the local one");
            // do nothing
        }

        else if(hit.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log("Trigger Damage: " + hit.name);
            var health = hit.GetComponent<Health>();
            var enemy = hit.GetComponent<Enemy>();
            if (health != null)
            {
                Vector3 positionToSpawn = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f), transform.position.y + Random.Range(-1.0f, 1.0f), transform.position.z + Random.Range(-0.5f, 0.5f));
                var bloodshed = (GameObject)Instantiate(bloodEffect, positionToSpawn, transform.rotation);
                NetworkServer.Spawn(bloodshed);
                Destroy(bloodshed, 1.0f);
                enemy.anim.SetTrigger("Hit");
                health.CmdDamagePlayer(player.makeDamage);
                //health.TakeDamage(player.makeDamage);
            }
        }

        else if (hit.gameObject.CompareTag("Mage"))
        {
            //Debug.Log("Trigger Damage: " + hit.name);
            var health = hit.GetComponent<Health>();
            var enemy = hit.GetComponent<Boss>();
            if (health != null && enemy.canTakeDamage)
            {
                Vector3 positionToSpawn = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f), transform.position.y + Random.Range(-1.0f, 1.0f), transform.position.z + Random.Range(-0.5f, 0.5f));
                var bloodshed = (GameObject)Instantiate(bloodEffect, positionToSpawn, transform.rotation);
                NetworkServer.Spawn(bloodshed);
                Destroy(bloodshed, 1.0f);
                health.CmdDamagePlayer(player.makeDamage);
                //health.TakeDamage(player.makeDamage);
            }
        }
    }
}