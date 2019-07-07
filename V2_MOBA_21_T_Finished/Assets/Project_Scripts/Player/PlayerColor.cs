using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerColor : NetworkBehaviour {

	//Syncs the value accross the network
	//[SyncVar]
	//hook will allow you to call a function everytime a value comes in from the network
	[SyncVar(hook="ApplyColor")]
	Color m_Color = Color.red;

	void OnTriggerEnter () {
		//This will only allow the server to set the new colour
		if(!isServer){
			return;
		}
		m_Color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
		ApplyColor(m_Color);
	}
	
	// Update is called once per frame
	void ApplyColor (Color c) {
		foreach(var r in GetComponentsInChildren<Renderer>()){
			r.material.color = c;
		}
	}
}

/*
 public class PlayerColor : MonoBehaviour {

	Color m_Color = Color.red;

	void OnTriggerEnter () {
		m_Color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
		ApplyColor(m_Color);
	}
	
	// Update is called once per frame
	void ApplyColor (Color c) {
		foreach(var r in GetComponentsInChildren<Renderer>()){
			r.material.color = c;
		}
	}
}
*/
