using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform m_T;
    public Transform Target;
    public float followSpeed;

    // Start is called before the first frame update
    void Start()
    {
        if ((m_T = GetComponent<Transform>()) == null) { Debug.Log("Transform not found. Destroying object."); Destroy(gameObject); };
        if (Target == null) { Debug.Log("Target Transform not found. Destroying object."); Destroy(gameObject); }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 interp = Vector2.Lerp(m_T.position, Target.position, followSpeed) - (Vector2)m_T.position;
        interp *= Time.fixedDeltaTime;
        m_T.Translate((Vector3)interp);
    }
}
