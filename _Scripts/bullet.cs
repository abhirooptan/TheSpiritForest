using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour {

    public GameObject bulletEffect;
    public GameObject blastEffect;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        var bullet = (GameObject)Instantiate(bulletEffect, transform.position, transform.rotation);
        Destroy(bullet, 2.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.GetComponent<Health>().TakeDamage(20);
        var blastSpawn = (GameObject)Instantiate(blastEffect, transform.position, transform.rotation);
        Destroy(blastSpawn, 1.0f);
        Destroy(gameObject);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    //if (!other.gameObject.CompareTag("Enemy"))
    //    //{
    //        var blastSpawn = (GameObject)Instantiate(blastEffect, transform.position, transform.rotation);
    //        Destroy(blastSpawn, 1.0f);
    //        Destroy(gameObject);
    //    //}
    //}
}
