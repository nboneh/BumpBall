using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

    bool stretchValue;
    float radius;
	// Use this for initialization
	void Start () {
        radius = this.transform.localScale.x;
        Rigidbody rb = GetComponent<Rigidbody>();
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
        UpdateStretch();
    }
	
	// Update is called once per frame
	void Update () {

    }

    void UpdateStretch()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 up = rb.transform.up;
        Vector3 right = rb.transform.right;
        Vector3 forward = rb.transform.forward;
        Vector3 vel = rb.velocity;

        float velx = Vector3.Dot(right, vel) / 100f;
        float vely = Vector3.Dot(up, vel) / 100f;
        float velz = Vector3.Dot(forward, vel) / 100f;
        rb.transform.localScale = new Vector3(radius - velx + vely / 2.0f + velz / 2.0f,
            radius + velx / 2.0f - vely + velz / 2.0f,
            radius + velx / 2.0f + vely / 2.0f - velz);
    }
}
