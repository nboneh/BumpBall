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

    ArrayList particleCollisions; 
    // Use this for initialization
    void Start () {
        radius = this.transform.localScale.x/2.0f;
        source = GetComponent<AudioSource>();
        stretchFactor = 0.0f;
        particleCollisions = new ArrayList();
    }

    void FixedUpdate()
    {
        float t = Time.deltaTime;
        UpdateColision(t);

    }

    public void SetRadius(float _radius)
    {
        radius = _radius;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.transform.localScale = new Vector3(radius*2.0f, radius*2.0f,radius*2.0f);
    }

    public void SetDensity(int density)
    {
       Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = radius * density;
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

        float stretchX = radius*2.0f - velx + vely / 2.0f + velz / 2.0f;
        float stretchY = radius*2.0f + velx / 2.0f - vely + velz / 2.0f;
        float stretchZ = radius*2.0f + velx / 2.0f + vely / 2.0f - velz;

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
        if (rb.velocity.magnitude < 10.0f)
            rb.velocity = new Vector3(acceleration.x, rb.velocity.y, acceleration.z);
        else
            rb.velocity = rb.velocity = new Vector3(rb.velocity.x + (acceleration.x * t * 2), rb.velocity.y, rb.velocity.z + (acceleration.z * t * 2));
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
        particleCollisions.Add(particleEffect);

        timeUntilCollideAgain = 0.5f;
        prevCollidedGameObject = collision.gameObject;
        source.PlayOneShot(bouncesound, (collision.relativeVelocity.magnitude * collision.relativeVelocity.magnitude) / 50.0f);
        stretchFactor = 4.0f;

        Destroy(particleEffect.gameObject, particleCollision.duration);
    }

    public bool isTouchingBall(float radius1, float x1, float z1)
    {
        float addedRadius = radius1 + radius;

        float x2 = GetComponent<Rigidbody>().transform.position.x;
        float z2 = GetComponent<Rigidbody>().transform.position.z;

        float dist = Vector3.Distance(new Vector3(x2, 0, z2), new Vector3(x1, 0, z1));

        return dist <= addedRadius;
    }

    void OnDestroy()
    {
        foreach(ParticleSystem particleCollision in particleCollisions) {
            if(particleCollision != null)
              Destroy(particleCollision.gameObject);
        }
    }
}
