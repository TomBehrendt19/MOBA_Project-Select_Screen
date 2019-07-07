using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSlash : NetworkBehaviour {

	private Animator anim;
	private PlayerMovement playerMovementScript;
	bool playerInRange;                         // Whether player is within the trigger collider and can be attacked.
	bool playerAttack; 
	float timer;                                // Timer for counting up to the next attack.
	GameObject target;
	public float timeBetweenAttacks = 0.5f;
	public float damage = 20;
	public float damageDelayFromKeyPress = 0.5f;
	bool damageAlreadyTaken;
	PlayerHealth playerHealthScript;
    string enemyTag;

	// Use this for initialization
	void Start () {
		if (!isLocalPlayer) {
			return;
		}
		anim = GetComponent<Animator> ();
		playerMovementScript = GetComponent<PlayerMovement>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			return;
		}
		if (Input.GetKeyDown (KeyCode.Q)) {
			anim.SetBool ("Slash", true);
			playerMovementScript.StopWalking ();
		}
		if (Input.GetKeyUp (KeyCode.Q)) {
			anim.SetBool ("Slash", false);
		}
		playerAttack = AnimatorIsPlaying ("slash");
		if (playerAttack) {
			playerMovementScript.StopWalking ();
		}else {
			damageAlreadyTaken = false;
		} 
	}

	void FixedUpdate () {
		if (!isLocalPlayer) {
			return;
		}
		if(playerInRange && AnimatorIsPlaying ("slash")){

			var lookPos = target.transform.position - transform.position;
			lookPos.y = 0;
			var rotation = Quaternion.LookRotation(lookPos);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5);

		}
        if(playerAttack && playerInRange){
			///damage and timer

			if(!damageAlreadyTaken){
					damageAlreadyTaken = true;
					StartCoroutine(damageDelay(damageDelayFromKeyPress));
			}
		}	
	}
    
	private IEnumerator damageDelay(float waitTime)
	{

		yield return new WaitForSeconds(waitTime);
		//Debug.Log ("Delay");
		if (playerAttack && playerInRange) {
			CmdInflictedDamage(target);
		}
	}
    [Command]
	void CmdInflictedDamage(GameObject target){
        RpcInflictedDamage(target);
	}
    
    [ClientRpc]
    void RpcInflictedDamage(GameObject target){
        playerHealthScript = target.GetComponent<PlayerHealth>();
        if (playerHealthScript) {
            playerHealthScript.Damage (damage);
        }
    }

	bool AnimatorIsPlaying(string stateName){
		return anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
	}

	void OnTriggerStay(Collider other)
	{
		if (!isLocalPlayer) {
			return;
		}
		// If the entering collider is the player...
		if(other.gameObject.tag == "Player")
		{
			// ... the player is in range.
			playerInRange = true;
			target = other.gameObject;
		
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (!isLocalPlayer) {
			return;
		}
		// If the exiting collider is the player...
		if(other.gameObject.tag == "Player")
		{
			// ... the player is no longer in range.
			playerInRange = false;
		}
	}

	void Attack(){
		// Add the time since Update was last called to the timer.
		timer += Time.deltaTime;

		// If the timer exceeds the time between attacks, the player is in range and this enemy is alive...
		if(timer >= timeBetweenAttacks && playerInRange)
		{
			// ... attack.
			Attack ();
		}
	}

}
