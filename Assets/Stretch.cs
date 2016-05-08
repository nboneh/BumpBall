using UnityEngine;
using System.Collections;

public class Stretch : MonoBehaviour {

    bool scaleOn;
    float radius;
	// Use this for initialization
	void Start () {
        radius = this.transform.localScale.x;
    }
	
	// Update is called once per frame
	void Update () {
        Rigidbody rb = GetComponent<Rigidbody>();
        float velx = rb.velocity.x/20f;
        float vely = rb.velocity.y/20f ;
        float velz = rb.velocity.z/20f;
        this.transform.localScale = new Vector3(radius - velx+vely/2.0f+velz/2.0f,
            radius + velx/2.0f - vely + velz/2.0f,
            radius + velx/2.0f + vely/2.0f - velz);
    }
}
