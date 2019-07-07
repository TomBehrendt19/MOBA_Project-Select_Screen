using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

//Reviewed Dan
//This script has one function that is run on the server.
//This script has 3 sync variables
public class PlayerShootingClick : NetworkBehaviour
{
    public int m_PlayerNumber = 1;            // Used to identify the different players.
    public Rigidbody m_Shell;                 // Prefab of the shell.
    public Transform m_FireTransform;         // A child of the Player where the shells are spawned.
    public Slider m_AimSlider;                // A child of the Player that displays the current launch force.
    public AudioSource m_ShootingAudio;       // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
    public AudioClip m_ChargingClip;          // Audio that plays when each shot is charging up.
    public AudioClip m_FireClip;              // Audio that plays when each shot is fired.
    public float m_MinLaunchForce = 15f;      // The force given to the shell if the fire button is not held.
    public float m_MaxLaunchForce = 30f;      // The force given to the shell if the fire button is held for the max charge time.
    public float m_MaxChargeTime = 0.75f;     // How long the shell can charge for before it is fired at max force.

	//Effects
	public Light gunLight;
	public Light faceLight;	
	public ParticleSystem gunParticles;  
	float timer;                                    // A timer to determine when to fire.
	public float timeBetweenBullets = 0.15f;        // The time between each shot.
	float effectsDisplayTime = 1f;                // The proportion of the timeBetweenBullets that the effects will display for.


    [SyncVar]
    public int m_localID;

    private string m_FireButton;            // The input axis that is used for launching shells.
    private Rigidbody m_Rigidbody;          // Reference to the rigidbody component.
    [SyncVar]
    private float m_CurrentLaunchForce;     // The force that will be given to the shell when the fire button is released.
    [SyncVar]
    private float m_ChargeSpeed;            // How fast the launch force increases, based on the max charge time.
    private bool m_Fired;                   // Whether or not the shell has been launched with this button press.

    private void Awake()
    {
        // Set up the references.
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void Start()
    {

        // The fire axis is based on the player number.
       
        // The rate that the launch force charges up is the range of possible forces by the max charge time.
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }

	public void DisableEffects ()
	{
		faceLight.enabled = false;
		gunLight.enabled = false;
	}

	/*
	A Custom Attribute that can be added to member functions of NetworkBehaviour scripts, to make them only run on clients, but not generate warnings.
	
	This custom attribute is the same as the [Client] custom attribute, except that it does not generate a warning in the console if called on a server. 
	This is useful to avoid spamming the console for functions that will be invoked by the engine, such as Update() or physics callbacks. 
	*/
    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer)
            return;
		
		timer += Time.deltaTime; //for turning off effects

		// If the timer has exceeded the proportion of timeBetweenBullets that the effects should be displayed for...
		if(timer >= timeBetweenBullets * effectsDisplayTime)
		{
			// ... disable the effects.
			DisableEffects ();
		}
    }

	[ClientRpc]
	public void RpcFireFromMoveScript(Vector3 direction)
	{
		m_CurrentLaunchForce = m_MaxLaunchForce;
		Fire (direction);
	}

	private void Fire(Vector3 direction)
    {
        // Set the fired flag so only Fire is only called once.
        m_Fired = true;

        // Change the clip to the firing clip and play it.
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();
		CmdFire(m_Rigidbody.velocity, m_CurrentLaunchForce, direction, m_FireTransform.position, m_FireTransform.rotation);
        // Reset the launch force.  This is a precaution in case of missing button events.
        m_CurrentLaunchForce = m_MinLaunchForce;
    }

	//Command tell the server to run this function not the client. These functions must begin with the prefix "Cmd" and cannot be static.
    [Command]
    private void CmdFire(Vector3 rigidbodyVelocity, float launchForce, Vector3 forward, Vector3 position, Quaternion rotation)
    {
		/////Effects
		timer = 0f;
		// Enable the lights.
		gunLight.enabled = true;
		faceLight.enabled = true;
		// Stop the particles from playing if they were, then start the particles.
		gunParticles.Stop ();
		gunParticles.Play ();


        // Create an instance of the shell and store a reference to it's rigidbody.
        Rigidbody shellInstance =
             Instantiate(m_Shell, position, rotation) as Rigidbody;

        // Create a velocity that is the Player's velocity and the launch force in the fire position's forward direction.
        Vector3 velocity = rigidbodyVelocity + launchForce * forward;

        // Set the shell's velocity to this velocity.
        shellInstance.velocity = velocity;
        NetworkServer.Spawn(shellInstance.gameObject);
    }
		
	/////////////////////////////////// Resets Player firre setting if die
    // This is used by the game manager to reset the Player.
    public void SetDefaults()
    {
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }
}