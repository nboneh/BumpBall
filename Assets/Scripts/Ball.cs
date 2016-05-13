using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

    float stretchFactor;
    float radius;
    public AudioClip bouncesound;
    public ParticleSystem particleCollision;


    private GameObject prevCollidedGameObject;
    private float timeUntilCollideAgain = 0.0f;

    private AudioSource source;
    // Use this for initialization
    void Start () {
        radius = this.transform.localScale.x;
        Rigidbody rb = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
        stretchFactor = 0.0f;
    }

    void FixedUpdate()
    {
        float t = Time.deltaTime;
        UpdateColision(t);

    }

    public float GetRadius()
    {
        return radius;
    }
	
	// Update is called once per frame
	void Update () {

    }

    void UpdateColision(float t)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 up = rb.transform.up;
        Vector3 right = rb.transform.right;
        Vector3 forward = rb.transform.forward;
        Vector3 vel = rb.velocity;

        float velx = (Vector3.Dot(right, vel)/40.0f) * stretchFactor;
        float vely = (Vector3.Dot(up, vel) / 40.0f) * stretchFactor;
        float velz =( Vector3.Dot(forward, vel) / 40.0f) * stretchFactor;

        float stretchX = radius - velx + vely / 2.0f + velz / 2.0f;
        float stretchY = radius + velx / 2.0f - vely + velz / 2.0f;
        float stretchZ = radius + velx / 2.0f + vely / 2.0f - velz;

        rb.transform.localScale = new Vector3(stretchX,stretchY,stretchZ);

        if (stretchFactor > 0.0f)
        {
            stretchFactor -= t * 40.0f;
            if (stretchFactor < 0.0f)
            {
                stretchFactor = 0.0f;
            }
        }

        if(timeUntilCollideAgain > 0.0f)
        {
            timeUntilCollideAgain -= t * 10.0f;
            if (timeUntilCollideAgain < 0.0f)
            {
                timeUntilCollideAgain = 0.0f;
            }
        }

    }

    public void updateAcceleration(Vector3 acceleration, float t)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb.velocity.magnitude < 7.0f)
            rb.velocity = new Vector3(acceleration.x, rb.velocity.y, acceleration.z);
        else
            rb.velocity = new Vector3(rb.velocity.x + (acceleration.x * t), rb.velocity.y, rb.velocity.z + (acceleration.z * t));
    }
    void OnCollisionEnter(Collision collision)
    {


        if (collision.gameObject.name == "Plane")
            return;

        if (timeUntilCollideAgain > 0.0f && prevCollidedGameObject == collision.gameObject)
            return;


        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, contact.normal);
        Vector3 pos = contact.point;
        pos.y = pos.y + radius / 2.0f;
        ParticleSystem particleEffect = (ParticleSystem)Instantiate(particleCollision, pos, rot);
        particleEffect.startSize = collision.relativeVelocity.magnitude/10.0f;
        particleEffect.startSpeed = collision.relativeVelocity.magnitude / 10.0f;
        ParticleSystem.ShapeModule shape = particleEffect.shape;
        shape.radius = radius / 4.0f;


        timeUntilCollideAgain = 0.5f;
        prevCollidedGameObject = collision.gameObject;
        source.PlayOneShot(bouncesound, (collision.relativeVelocity.magnitude * collision.relativeVelocity.magnitude) / 1000.0f);
        stretchFactor = 4.0f;

        Destroy(particleEffect.gameObject, particleCollision.duration);
    }
}
