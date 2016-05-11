using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

    float stretchFactor;
    float radius;
    public AudioClip bouncesound;
    

    private GameObject prevColidedGameObject;
    private float timeTillColideAgain = 0.0f;

    private AudioSource source;
    // Use this for initialization
    void Start () {
        radius = this.transform.localScale.x;
        Rigidbody rb = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
        stretchFactor = 0.0f;
        if (this.gameObject.name.Equals("Sphere"))
        {
            rb.velocity = new Vector3(6, 0, 6);
        } else if (this.gameObject.name.Equals("basket_bal"))
        {
            rb.velocity = new Vector3(-6, 0, 6);
        } else if (this.gameObject.name.Equals("default"))
        {
            rb.velocity = new Vector3(6, 0, -6);
        } else if (this.gameObject.name.Equals("soccerball")) {
            rb.velocity = new Vector3(-6, 0, -6);
        }
    }

    void FixedUpdate()
    {
        float t = Time.deltaTime;
        UpdateColision(t);

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

        float velx = (Vector3.Dot(right, vel)/60.0f) * stretchFactor;
        float vely = (Vector3.Dot(up, vel) / 60.0f) * stretchFactor;
        float velz =( Vector3.Dot(forward, vel) / 60.0f) * stretchFactor;

        float stretchX = radius - velx + vely / 2.0f + velz / 2.0f;
        float stretchY = radius + velx / 2.0f - vely + velz / 2.0f;
        float stretchZ = radius + velx / 2.0f + vely / 2.0f - velz;

        rb.transform.localScale = new Vector3(stretchX,
            stretchY,
            stretchZ);

        if (stretchFactor > 0.0f)
        {
            stretchFactor -= t * 40.0f;
            if (stretchFactor < 0.0f)
            {
                stretchFactor = 0.0f;
            }
        }

        if(timeTillColideAgain > 0.0f)
        {
            timeTillColideAgain -= t * 10.0f;
            if (timeTillColideAgain < 0.0f)
            {
                timeTillColideAgain = 0.0f;
            }
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        //ContactPoint contact = collision.contacts[0];
        //Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
       // Vector3 pos = contact.point;
        //Instantiate(explosionPrefab, pos, rot);

        if (collision.gameObject.name == "Plane")
            return;

        if (timeTillColideAgain > 0.0f && prevColidedGameObject == collision.gameObject)
            return;



        timeTillColideAgain = 0.5f;
        prevColidedGameObject = collision.gameObject;
        source.PlayOneShot(bouncesound, (collision.relativeVelocity.magnitude * collision.relativeVelocity.magnitude) / 10000.0f);
        stretchFactor = collision.relativeVelocity.magnitude/3.0f;

    }
}
