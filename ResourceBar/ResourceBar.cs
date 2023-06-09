using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBar : MonoBehaviour
{
    public Transform m_BarTransform;

    public void SetFill(float amount)
    {
        amount = Mathf.Clamp(amount, 0.0f, 1.0f);
        Vector2 newPos;
        newPos.x = -(1.0f - amount) * 8.0f;
        newPos.y = 0;
        m_BarTransform.localPosition = (Vector3)newPos;
    }
}
