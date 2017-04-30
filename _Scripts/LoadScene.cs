using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {

    [SerializeField]
    private string levelName;

	// Use this for initialization
	void Start () {
		
	}

    public void LoadLevel()
    {

        SceneManager.LoadScene(levelName);
    }
}
