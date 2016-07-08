using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Control : MonoBehaviour {

    public Image stick;
    public Image pad;
    bool on = false;
	bool preOn = false;
    float alpha = 0;
    float maxMoveRadius;
    
	// Use this for initialization
	void Start () {
        resize();
        updateAlpha();
	}

    void resize()
    {
        float scalePad = (Mathf.Min(Screen.width,Screen.height)/600.0f);
        float stickScale = scalePad * (2 / 3.0f);
        pad.rectTransform.localScale = new Vector3(scalePad, scalePad, scalePad);
        stick.rectTransform.localScale = new Vector3(stickScale, stickScale, stickScale);


        maxMoveRadius = (pad.transform.localScale.x * pad.rectTransform.rect.width) / 2.0f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            stick.rectTransform.position = mousePos;
            pad.rectTransform.transform.position = mousePos;
			preOn = true;
        } else if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 padPos = pad.rectTransform.position;
            if (Vector3.Distance(padPos, mousePos) >= maxMoveRadius)
            {
               float angle = getAngle(mousePos, padPos);
                Vector3 calcPos = new Vector3();
                calcPos.x = maxMoveRadius * Mathf.Cos(angle) + padPos.x;
                calcPos.y = maxMoveRadius * Mathf.Sin(angle) + padPos.y;
                stick.rectTransform.position = calcPos;
            }
            else
                  stick.rectTransform.position = mousePos;
			if (preOn)
				on = true;
        } else if (Input.GetMouseButtonUp(0))
        {
            on = false;
			preOn = false;
        }
    }

    void FixedUpdate()
    {
        float t = Time.deltaTime;
        if(on && alpha < 1.0f)
        {
            alpha += t*3;
            if(alpha > 1.0f)
            {
                alpha = 1.0f;
            }
        }

        if (!on && alpha > 0.0f)
        {
            alpha -= t*3;
            if (alpha < 0.0f)
            {
                alpha = 0.0f;
            }

            //Snap back
            Vector3 padPos = pad.rectTransform.position;
            Vector3 stickPos = stick.rectTransform.position;
            float angle = getAngle(stickPos, padPos);
            float radius = Vector3.Distance(stickPos, padPos) - t * 500 ;
            if(radius < 0)
            {
                radius = 0;
            }
            Vector3 calcPos = new Vector3();
            calcPos.x = radius * Mathf.Cos(angle) + padPos.x;
            calcPos.y = radius * Mathf.Sin(angle) + padPos.y;
            stick.rectTransform.position = calcPos;
        }

        updateAlpha();
    }

    void updateAlpha()
    {
        Color c = stick.color;
        c.a = alpha;
        stick.color = c;

        c = pad.color;
        c.a = alpha;
        pad.color = c;
    }

    float getAngle(Vector3 one, Vector3 two)
    {
         return Mathf.Atan2(one.y - two.y, one.x - two.x);
    }


    public float getRadius()
    {
        return Vector3.Distance(pad.rectTransform.position, stick.rectTransform.position);
    }

    public Vector3 getAcceleration()
    {
        Vector3 padPos = pad.rectTransform.position;
        Vector3 stickPos = stick.rectTransform.position;
        float angle = getAngle(stickPos, padPos);
        float radius = (Vector3.Distance(stickPos, padPos))/ maxMoveRadius * 8;

        Vector3 acceleration = new Vector3();
        acceleration.x = radius * Mathf.Cos(angle);
        acceleration.y = 0;
        acceleration.z = radius * Mathf.Sin(angle) ;

        return acceleration;
    }

    public bool isOn()
    {
        return on;
    }

    public void turnOff()
    {
        on = false;
		preOn = false;
    }
}
