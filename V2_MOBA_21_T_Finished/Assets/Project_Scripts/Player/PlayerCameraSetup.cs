using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCameraSetup : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		//Set the camera to to taget the local player
		if (isLocalPlayer) {
			GameObject.FindGameObjectWithTag ("CameraRig").GetComponent<UnityStandardAssets.Cameras.AutoCam> ().Target = gameObject.transform;
		}
	}

}
