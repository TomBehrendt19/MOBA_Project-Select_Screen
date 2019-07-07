using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

//Reviewed Dan
public class ShellExplosion : NetworkBehaviour
{
    public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
	public ParticleSystem m_ExplosionParticlesTwo;         // Reference to the particles that will play on explosion.
    public AudioSource m_ExplosionAudio;                // Reference to the audio that will play on explosion.
    public float m_MaxDamage = 100f;                    // The amount of damage done if the explosion is centred on a Player.
    public float m_ExplosionForce = 1000f;              // The amount of force added to a Player at the centre of the explosion.
    public float m_MaxLifeTime = 2f;                    // The time in seconds before the shell is removed.
    public float m_ExplosionRadius = 5f;                // The maximum distance away from the explosion Players can be and are still affected.


	private int m_PlayerMask;                             // A layer mask so that only the Players are affected by the explosion.

    private void Start()
    {
        if (isServer)
        {
            // If it isn't destroyed by then, destroy the shell after it's lifetime.
            Destroy(gameObject, m_MaxLifeTime);
            GetComponent<Collider>().enabled = false;
            StartCoroutine(EnableCollision());
        }

        // Set the value of the layer mask based solely on the Players layer.
		m_PlayerMask = LayerMask.GetMask("Shootable");
    }

    //allow to delay a bit the activation of the collider so that it don't collide when spawn close to the canon
    IEnumerator EnableCollision()
    {
        yield return new WaitForSeconds(0.1f);
        GetComponent<Collider>().enabled = true;
    }

    //Trigger are handled only on the server, as it have authority
	[ServerCallback] //ServerCallback makes the function only run on servers
    private void OnTriggerEnter(Collider other)
    {
        // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_PlayerMask);

        // Go through all the colliders...
        for (int i = 0; i < colliders.Length; i++)
        {
            // ... and find their rigidbody.
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

            // If they don't have a rigidbody, go on to the next collider.
            if (!targetRigidbody)
				continue; //The continue statement passes control to the next iteration of the enclosing while, do, for, or foreach statement in which it appears.

            // Find the PlayerHealth script associated with the rigidbody.
            PlayerHealth targetHealth = targetRigidbody.GetComponent<PlayerHealth>();

            // If there is no PlayerHealth script attached to the gameobject, go on to the next collider.
            if (!targetHealth)
                continue;

            // Create a vector from the shell to the target.
            Vector3 explosionToTarget = targetRigidbody.position - transform.position;

            // Calculate the distance from the shell to the target.
            float explosionDistance = explosionToTarget.magnitude;

            // Calculate the proportion of the maximum distance (the explosionRadius) the target is away.
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

            // Calculate damage as this proportion of the maximum possible damage.
            float damage = relativeDistance * m_MaxDamage;

            // Make sure that the minimum damage is always 0.
			damage = Mathf.Max(0f, damage); //Returns largest of two or more values

            // Deal this damage to the Player.
            targetHealth.Damage(damage);
        }

        if (!NetworkClient.active)//if we are ALSO client (so hosting), this will be done by the Destroy so Skip
            PhysicForces();

        // Destroy the shell on clients.
        NetworkServer.Destroy(gameObject);
    }

    //called on client when the Network destroy that object (it was destroyed on server)
    public override void OnNetworkDestroy()
    {

        //we spawn the explosion particle
        ExplodeShell();
        //set the particle to be destroyed at the end of their lifetime
        ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
        Destroy(m_ExplosionParticles.gameObject, mainModule.duration);
		ParticleSystem.MainModule mainModuleTwo = m_ExplosionParticlesTwo.main;
		Destroy(m_ExplosionParticlesTwo.gameObject, mainModule.duration);
        base.OnNetworkDestroy();
    }

    void ExplodeShell()
    {
        // Unparent the particles from the shell.
        m_ExplosionParticles.transform.parent = null;
		m_ExplosionParticlesTwo.transform.parent = null;
        // Play the particle system.
        m_ExplosionParticles.Play();
		m_ExplosionParticlesTwo.Play();
        // Play the explosion sound effect.
        m_ExplosionAudio.Play();

        PhysicForces();
    }


    //This apply force on object. Do that on all clients & server as each must apply force to object they own
    void PhysicForces()
    {
        // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_PlayerMask);

        // Go through all the colliders...
        for (int i = 0; i < colliders.Length; i++)
        {
            // ... and find their rigidbody.
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

            // If they don't have a rigidbody or we don't own that object, go on to the next collider.
            if (!targetRigidbody || !targetRigidbody.GetComponent<NetworkIdentity>().hasAuthority)
                continue;

            // Add an explosion force with no vertical bias.
            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);
        }
    }
}