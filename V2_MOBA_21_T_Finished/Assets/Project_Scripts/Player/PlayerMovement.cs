using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

//Reviewed Dan
public class PlayerMovement : NetworkBehaviour
{

    public int m_PlayerNumber = 1;                // Used to identify which Player belongs to which player.  This is set by this Player's manager.
    public int m_LocalID = 1;
    public AudioSource m_MovementAudio;           // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
    public ParticleSystem m_DustTrail;        // The particle system of dust that is kicked up from the left track.
    public Rigidbody m_Rigidbody;              // Reference used to move the Player.

	public float shootDistance = 10f;
	public float shootRate = .5f;
	private PlayerShootingGun shootingScriptGun;
	//private PlayerShootingClick shootingScriptClick;

	private Animator anim;
	private NavMeshAgent navMeshAgent;
	private Transform targetedEnemy;
	private Ray shootRay;
	private RaycastHit shootHit;
	private bool walking = false;
	public bool enemyClicked;
	private float nextFire;
	private bool destinationSet = false;
	public float RotationSpeed = 10;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

		anim = GetComponent<Animator> ();
		navMeshAgent = GetComponent<NavMeshAgent> ();
		shootingScriptGun = GetComponent<PlayerShootingGun> ();
		//shootingScriptClick = GetComponent<PlayerShootingClick> ();
    }
		
    private void Start()
    {
		navMeshAgent.destination = transform.position;
    }
	public void StopWalking(){
		walking = false;
		destinationSet = false;
		navMeshAgent.isStopped = true;
	}

    private void Update()
    {
        if (!isLocalPlayer)
            return;

		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;

		if (Input.GetMouseButtonDown(0)) 
		{
			if (Physics.Raycast(ray, out hit, 100))
			{
				if (hit.collider.CompareTag("Player") && shootingScriptGun)
					{
						targetedEnemy = hit.transform;
						enemyClicked = true;
					}

					else
					{
						walking = true;
						enemyClicked = false;
						destinationSet = true;
						CmdSetDestination(hit.point);
						navMeshAgent.isStopped= false;
						


					}
			}
		}
		if (Input.GetMouseButton (1)) {
			

			if (Physics.Raycast (ray, out hit, 100)) {
				//transform.LookAt (hit.point);
				//transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, Time.time * speed);
				//find the vector pointing from our position to the target

				//values that will be set in the Inspector


				//values for internal use
				Quaternion _lookRotation;
				Vector3 _direction;

				_direction = (hit.point - transform.position).normalized;

				//create the rotation we need to be in to look at the target
				_lookRotation = Quaternion.LookRotation(_direction);

				//rotate us over time according to speed until we are in the required rotation
				transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
			}
		}

		if (enemyClicked) {
			RpcMoveAndShoot();
		}

		if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
			//Has he ran out of a path or is he almost not moving.
			//float.Epsilon = is the value near to 0
			if (!navMeshAgent.hasPath || Mathf.Abs (navMeshAgent.velocity.sqrMagnitude) < float.Epsilon) {
				walking = false;
			}
		} else if(destinationSet){
			walking = true;
		} 


		anim.SetBool ("IsWalking", walking);
  
    }
		
	private void CmdSetDestination(Vector3 target)
	{
		navMeshAgent.destination = target;
	}

	//[ClientRpc]
	private void RpcMoveAndShoot()
	{
		if (targetedEnemy == null)
			return;
		navMeshAgent.destination = targetedEnemy.position;
		if (navMeshAgent.remainingDistance >= shootDistance) {
			navMeshAgent.isStopped = false;
			walking = true;
		}

		if (navMeshAgent.remainingDistance <= shootDistance) {

			transform.LookAt(targetedEnemy);
			Vector3 dirToShoot = targetedEnemy.transform.position - transform.position;
			if (Time.time > nextFire)
			{
				nextFire = Time.time + shootRate;
				CmdCallShootGunScript(dirToShoot);
			}
			StopWalking ();
		}
	}

	[Command]
	private void CmdCallShootGunScript(Vector3 dirToShoot)
	{
		shootingScriptGun.RpcShoot(dirToShoot);
	}



    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        // Adjust the rigidbodies position and orientation in FixedUpdate.
		m_Rigidbody.position = transform.position;
    }

	/////////////////////////////////////////////Reset
	/////////////////////Used to reset for next match
    // This function is called at the start of each round to make sure each Player is set up correctly.
    public void SetDefaults()
    {
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
		navMeshAgent.isStopped=true;
		destinationSet = false;
		enemyClicked = false;
		walking = false;
        m_DustTrail.Clear();
        m_DustTrail.Stop();
    }
		
    public void ReEnableParticles()
    {
        m_DustTrail.Play();
    }

    //We freeze the rigibody when the control is disabled to avoid the Player drifting!
    protected RigidbodyConstraints m_OriginalConstrains;
    void OnDisable()
    {
        m_OriginalConstrains = m_Rigidbody.constraints;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    void OnEnable()
    {
		//Debug.Log ("Enable");
		walking = false;
        m_Rigidbody.constraints = m_OriginalConstrains;
		navMeshAgent.destination = transform.position;
    }
}