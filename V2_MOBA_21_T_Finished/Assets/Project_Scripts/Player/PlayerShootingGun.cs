using UnityEngine;
//using UnitySampleAssets.CrossPlatformInput;
using UnityEngine.Networking;

public class PlayerShootingGun : NetworkBehaviour
    {
        public int damagePerShot = 20;                  // The damage inflicted by each bullet.
        public float timeBetweenBullets = 0.15f;        // The time between each shot.
        public float range = 100f;                      // The distance the gun can fire.

		//[SyncVar]
        float timer;                                    // A timer to determine when to fire.
        Ray shootRay = new Ray();                       // A ray from the gun end forwards.
        RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
        int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.
        public ParticleSystem gunParticles;                    // Reference to the particle system.
        public LineRenderer gunLine;                           // Reference to the line renderer.
		public Transform FireTransform;
		AudioSource gunAudio;                           // Reference to the audio source.
		public AudioClip gunAudioClip;
        public Light gunLight;                                 // Reference to the light component.
		public Light faceLight;								// Duh
        float effectsDisplayTime = 0.5f;                // The proportion of the timeBetweenBullets that the effects will display for.
		private PlayerMovement playerMovement;
		public int numberOfFollowShots = 3;
		private int count = 0;
		private Animator anim;

        void Awake ()
        {
            // Create a layer mask for the Shootable layer.
            shootableMask = LayerMask.GetMask ("Shootable");
			anim = GetComponent<Animator> ();
            // Set up the references.
            //gunParticles = GetComponentInChildren<ParticleSystem> ();
            //gunLine = GetComponent <LineRenderer> ();
            gunAudio = GetComponent<AudioSource> ();
            //gunLight = GetComponent<Light> ();
			//faceLight = GetComponentInChildren<Light> ();
			playerMovement = GetComponent<PlayerMovement> ();
        }

		[ServerCallback]
        void Update ()
        {
		
            // Add the time since Update was last called to the timer.
            timer += Time.deltaTime;

		if(Input.GetKey (KeyCode.F) && timer >= timeBetweenBullets)
			{
				// ... shoot the gun.
			RpcShoot (Vector3.zero);
			}

            // If the timer has exceeded the proportion of timeBetweenBullets that the effects should be displayed for...
            if(timer >= timeBetweenBullets * effectsDisplayTime)
            {
                // ... disable the effects.
                DisableEffects ();
            }
        }

		
        public void DisableEffects ()
        {
            // Disable the line renderer and the light.
            gunLine.enabled = false;
			faceLight.enabled = false;
            gunLight.enabled = false;

        }
		
		[ClientRpc]
		public void RpcShoot (Vector3 dirToShoot)
        {

			count++;
			if(count >= numberOfFollowShots){
				playerMovement.enemyClicked = false;
				count = 0;
			}
            // Reset the timer.
            timer = 0f;

            // Play the gun shot audioclip.
			gunAudio.clip = gunAudioClip;
            gunAudio.Play ();

            // Enable the lights.
            gunLight.enabled = true;
			faceLight.enabled = true;

            // Stop the particles from playing if they were, then start the particles.
            gunParticles.Stop ();
            gunParticles.Play ();

            // Enable the line renderer and set it's first position to be the end of the gun.
            gunLine.enabled = true;
			gunLine.SetPosition (0, FireTransform.position);

            // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
			shootRay.origin = FireTransform.position;

			shootRay.direction = dirToShoot;
	
            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
            {
                // Try and find an EnemyHealth script on the gameobject hit.
                PlayerHealth playerHealth = shootHit.collider.GetComponent <PlayerHealth> ();

                // If the EnemyHealth component exist...
                if(playerHealth != null)
                {
                    // ... the enemy should take damage.
                    playerHealth.Damage (damagePerShot);
                }

                // Set the second position of the line renderer to the point the raycast hit.
                gunLine.SetPosition (1, shootHit.point);
				//Debug.DrawRay(shootRay.origin, shootHit.point, Color.green, 2);
            }
            // If the raycast didn't hit anything on the shootable layer...
            else
            {
                // ... set the second position of the line renderer to the fullest extent of the gun's range.
				gunLine.SetPosition (1, shootHit.point);
            }
        }
 }