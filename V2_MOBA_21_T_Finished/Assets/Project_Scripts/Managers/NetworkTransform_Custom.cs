using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings (channel = 0, sendInterval = 0.04f)]
public class NetworkTransform_Custom : NetworkBehaviour {

	public float lerpRate;
	[SerializeField]
	Transform playerPrefab;
	//[SerializeField]
	//Transform playerCamera;

	[SyncVar]
	private Vector3 syncPos;

	[SyncVar]
	private float syncRotY;

	public int snapThreshold = 5;

	//[SyncVar]
	//private float syncRotX;


	void Update ()
	{
		TransmitSync ();
	}

	void FixedUpdate ()
	{
		LerpTransform ();
	}

	void LerpTransform ()
	{
		if (!isLocalPlayer)
		{
			if(Vector3.Distance (playerPrefab.position, syncPos) > snapThreshold) {
				playerPrefab.position = syncPos;
			}
			playerPrefab.position = Vector3.Lerp (playerPrefab.position, syncPos, lerpRate * Time.deltaTime);
			playerPrefab.rotation = Quaternion.Lerp (playerPrefab.rotation, Quaternion.Euler (0, syncRotY,  0), lerpRate * Time.deltaTime);

			//playerCamera.rotation = Quaternion.Lerp (playerCamera.rotation, Quaternion.Euler(syncRotX, playerCamera.rotation.eulerAngles.y, 0), lerpRate * Time.deltaTime);
		}
	}

	[Command]
	void CmdSyncTransform (Vector3 pos, float rotY)
	{
		syncPos = pos;
		syncRotY = rotY;
		//syncRotX = rotX;
	}

	[ClientCallback]
	void TransmitSync ()
	{
		if (isLocalPlayer)
		{
			CmdSyncTransform (playerPrefab.position, playerPrefab.rotation.eulerAngles.y);
		}
	}

}