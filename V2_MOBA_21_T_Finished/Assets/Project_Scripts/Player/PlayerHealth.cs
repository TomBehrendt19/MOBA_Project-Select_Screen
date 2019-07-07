using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

//Reviewed Dan
//This script has one function that is run on all clients "ClientRpc"
//The ClientRpc function plays a partical effect, sound and deactivates the player

public class PlayerHealth : NetworkBehaviour
{
    public float m_StartingHealth = 100f;             // The amount of health each Player starts with.
    public Slider m_Slider;                           // The slider to represent how much health the Player currently has.
    public Image m_FillImage;                         // The image component of the slider.
    public Color m_FullHealthColor = Color.green;     // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;       // The color the health bar will be when on no health.
	public AudioClip m_PlayerExplosion;                 // The clip to play when the Player explodes.
    public ParticleSystem m_ExplosionParticles;       // The particle system the will play when the Player is destroyed.
	public GameObject m_PlayerGEO;                // References to all the gameobjects that need to be disabled when the Player is dead.
	public GameObject m_Gun;
	public GameObject m_PlayerRenderers;
    public GameObject m_HealthCanvas;
    public GameObject m_AimCanvas;
    public GameObject m_LeftDustTrail;
    //public GameObject m_RightDustTrail;
    public PlayerSetup m_Setup;
    public PlayerManager m_Manager;                   //Associated manager, to disable control when dying.

    [SyncVar(hook = "OnCurrentHealthChanged")]
    public float m_CurrentHealth;                  // How much health the Player currently has.*
    [SyncVar]
    private bool m_ZeroHealthHappened;              // Has the Player been reduced beyond zero health yet?
    private Collider m_Collider;                 // Used so that the Player doesn't collide with anything when it's dead.


    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
		//m_Animator = GetComponent<Animator> ();
		//OnCurrentHealthChanged(m_CurrentHealth); ///????May not need this line
    }


    // This is called whenever the Player takes damage.
    public void Damage(float amount)
    {
        // Reduce current health by the amount of damage done.
        m_CurrentHealth -= amount;

        // If the current health is at or below zero and it has not yet been registered, call OnZeroHealth.
        if (m_CurrentHealth <= 0f && !m_ZeroHealthHappened)
        {
            OnZeroHealth();
        }
    }


    private void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }


    void OnCurrentHealthChanged(float value)
    {
        m_CurrentHealth = value;
        // Change the UI elements appropriately.
        SetHealthUI();

    }

    private void OnZeroHealth()
    {
        // Set the flag so that this function is only called once.
        m_ZeroHealthHappened = true;

        RpcOnZeroHealth();
    }

    private void InternalOnZeroHealth()
    {
        // Disable the collider and all the appropriate child gameobjects so the Player doesn't interact or show up when it's dead.
		SetPlayerActive(false);
    }


	//[ClientRPC] functions are called by code on Unity Multiplayer servers, and then invoked on corresponding GameObjects on clients connected to the server.
	//This ensures that the function is called and run on all clients
	//These functions must begin with the prefix "Rpc" and cannot be static.
    [ClientRpc]
    private void RpcOnZeroHealth()
    {
        // Play the particle system of the Player exploding.
        m_ExplosionParticles.Play();

        // Create a gameobject that will play the Player explosion sound effect and then destroy itself.
		AudioSource.PlayClipAtPoint(m_PlayerExplosion, transform.position);

        InternalOnZeroHealth();
    }


	/////////////////////////////// Deactivates Player if health is < 0
	private void SetPlayerActive(bool active)
    {
        m_Collider.enabled = active;

		//m_Animator.SetBool ("Die", !active);
		m_PlayerGEO.SetActive(active);
		if(m_Gun){
			m_Gun.SetActive(active);
		}
		m_HealthCanvas.SetActive(active);
        m_AimCanvas.SetActive(active);
        m_LeftDustTrail.SetActive(active);
        //m_RightDustTrail.SetActive(active);
		m_PlayerRenderers.SetActive(active);

		///////////////////////////////////Enables and disables player controls
		if (active) {
			m_Manager.EnableControl ();
		}else {
			m_Manager.DisableControl();
		}

		///////////////////////////////////Activates and deactivates the crown
        m_Setup.ActivateCrown(active);

    }

    // This function is called at the start of each round to make sure each Player is set up correctly.
    public void SetDefaults()
    {
        m_CurrentHealth = m_StartingHealth;
        m_ZeroHealthHappened = false;
		SetPlayerActive(true);
    }
}