using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rotate : MonoBehaviour {

    [SerializeField]
    private float rotationSpeed = 15f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        //Determines what angle and speed the object rotates.
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);

	}

    //Debugs to tell the user when the models have been disabled.
    private void OnDisable()
    {
        Debug.Log("OnDisable");
    }
}
