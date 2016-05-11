using UnityEngine;
using System.Collections;

public class FieldView : MonoBehaviour {

    public float width = 16f;
    public float height = 9f;


    void Start()
    {
        SetResoluton();
    }

    void SetResoluton()
    {
        GetComponent<Camera>().aspect = width / height;
    }
}
