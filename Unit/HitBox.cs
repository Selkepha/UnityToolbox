// HitBox.cs
/* ---------------------------------------------------------------------------- */
// HitBox class
/* ---------------------------------------------------------------------------- */
// Author:  github.com/Raexyl, using guide: https://www.gamedeveloper.com/design/hitboxes-and-hurtboxes-in-unity
// Date:    June 2023
/* ---------------------------------------------------------------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColliderState
{
    Closed,
    Open,
    Colliding
}

// Hitboxes are intended to be added to units during Start(); as a unity component
public class HitBox : MonoBehaviour
{
    public Vector2 extents = new Vector2(1.0f, 1.0f);
    public Vector2 position = new Vector2(0.0f, 0.0f);
    private Vector2 flippedPosition;
    public LayerMask mask;

    public Color inactiveColor = new Color(0.5f, 0.5f, 0.1f, 0.4f);
    public Color activeColor = new Color(0.7f, 0.2f, 0.2f, 0.4f);
    public Color collidingColor = new Color(0.7f, 0.2f, 0.2f, 0.8f);

    private ColliderState m_State;
    private IHitboxResponder m_Responder = null;

    public bool viewHitBox = true;

    public void Start()
    {
    }

    public void UpdateHitBox()
    {
        if(m_State == ColliderState.Closed) { return; };

        flippedPosition = position;
        flippedPosition.x *= -1;

        // Check for hit
        Collider2D[] colliders;
        if (transform.localScale.x < 0) {
            colliders = Physics2D.OverlapBoxAll((Vector2)transform.position + flippedPosition, extents * 2, 0, mask); // Physics2D.OverlapBoxAllNonAlloc may be more performant
        } else {
            colliders = Physics2D.OverlapBoxAll((Vector2)transform.position + position, extents * 2, 0, mask); // Physics2D.OverlapBoxAllNonAlloc may be more performant
        }

        
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D c = colliders[i];
            //check if source unit! // Or do goodies and enemies as layers?
            m_Responder?.CollidingWith(c); // Trigger hit effect
        }

        // Update state
        m_State = colliders.Length > 0 ? ColliderState.Colliding : ColliderState.Open;
    }

    public void Activate()
    {
        m_State = ColliderState.Open;
    }

    public void Deactivate()
    {
        m_State = ColliderState.Closed; 
    }

    public void SetResponder(IHitboxResponder responder)
    {
        m_Responder = responder;
    }


    //Draw the hitbox in scene view
    private void OnDrawGizmos()
    {
        if(!viewHitBox) { return; };

        //Set correct colour
        switch (m_State)
        {
            case ColliderState.Closed:
                Gizmos.color = inactiveColor;
                break;

            case ColliderState.Open:
                Gizmos.color = activeColor;
                break;

            case ColliderState.Colliding:
                Gizmos.color = collidingColor;
                break;
        }

        // Draw box 
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube((Vector3)position, (Vector3)(extents * 2)); // Because size is halfExtents
    }
}
