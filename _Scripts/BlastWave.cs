using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastWave : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    private void OnParticleCollision(GameObject other)
    {
        other.GetComponent<Health>().TakeDamage(0.8f);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
