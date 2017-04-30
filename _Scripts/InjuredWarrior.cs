using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;

public class InjuredWarrior : MonoBehaviour {

    public GameObject flowchart;

    private bool isEnabled = false;

	// Use this for initialization
	void Start () {
        flowchart.gameObject.SetActive(false);
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isEnabled)
        {
            flowchart.gameObject.SetActive(true);

            isEnabled = true;
            other.GetComponent<PlayerScript>().isInteracting = true;
            Cursor.visible = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isEnabled = false;
            other.GetComponent<PlayerScript>().isInteracting = false;
            Cursor.visible = false;
        }
    }

    //[ClientRpc]
    public void HandleFlowchart(GameObject other, bool enable)
    {
        if(enable)
            flowchart.gameObject.SetActive(enable);

        isEnabled = enable;
        other.GetComponent<PlayerScript>().isInteracting = enable;
        Cursor.visible = enable;
    }
}
