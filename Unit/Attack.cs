// Attack.cs
/* ---------------------------------------------------------------------------- */
// Attack base class. Override CollidingWith() method in derivatives to change
// attack. Alter attackDamage, knockBack in derivative.
// Each attack is intended to use only one hitbox, which changes with each frame.
// This class manipulates hitboxes to create an attack.
/* ---------------------------------------------------------------------------- */
// Author:  github.com/Raexyl, using guide: https://www.gamedeveloper.com/design/hitboxes-and-hurtboxes-in-unity
// Date:    June 2023
/* ---------------------------------------------------------------------------- */

/* ----- TODO ----- */
// - Different damage on each frame?
// - Different knockback on each frame?

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour, IHitboxResponder
{
    public int attackDamage = 10;
    public Vector2 knockBack = new Vector2(0, 0);
    protected Unit source;

    public float cooldownTime = 0.0f;
    protected float m_CooldownAcc = 0.0f; // Stores time left on cooldown.

    protected Animator m_Animator;
    public string animatorTrigger; // A string to an animator trigger

    protected HitBox hitBox;
    public bool viewHitBoxes = false;
    public List<Bounds> hitBoxFrames = new List<Bounds>();
    // Posx, Posy, Extentx, Extenty

    protected bool m_IsAttacking = false;
    protected bool m_HasHit = false;

    void Start()
    {
        if (GetComponentInParent<Unit>() == null) { Debug.Log("Unit component required in parent!"); Destroy(this); };
        m_Animator = GetComponentInParent<Animator>();
    }

    void Update()
    {
        m_CooldownAcc -= Time.deltaTime;
        if(m_CooldownAcc < 0.0f) { m_CooldownAcc = 0.0f; };
    }

    virtual public void Use()
    {
        if(m_CooldownAcc > 0.0f) { return; }; // Return if still on cooldown.
        m_CooldownAcc = cooldownTime;
        hitBox.mask = LayerMask.GetMask("Units");
        hitBox.SetResponder(this);
        if (!m_IsAttacking) { 
            StartCoroutine("ThisAttack");
        }
    }

    virtual public void CollidingWith(Collider2D c)
    {
        // Check it is not the source
        if(c.GetComponent<Unit>() == source) { return; };

        // Apply damage
        m_HasHit = true;
        Unit u = c.GetComponent<Unit>();
        u?.TakeDamageFrom(source, attackDamage);

        // Deal knockback
        CustomCharacterController ccc = c.GetComponent<CustomCharacterController>();
        ccc?.ApplyKnockback(knockBack, true);

        Debug.Log("Damage dealt!");
    }

    IEnumerator ThisAttack()
    {
        m_IsAttacking = true;
        hitBox.Activate();
        m_Animator.SetTrigger(animatorTrigger);

        for (int i = 0; i < hitBoxFrames.Count; i++)
        {
            // hitbox position is in local space!
            // Update hitbox position and shape:
            hitBox.position = hitBoxFrames[i].center;
            hitBox.extents = hitBoxFrames[i].extents;

            if (!m_HasHit) { hitBox.UpdateHitBox(); }; // Update hitbox to check for collisions
            yield return new WaitForFixedUpdate(); // Wait until next frame:
        }

        hitBox.Deactivate();
        m_IsAttacking = false;
        m_HasHit = false;
    }

    // Set for source and hitbox

    public void SetHitBox(HitBox hb)
    {
        hitBox = hb;
    }

    public void SetSource(Unit u)
    {
        source = u;
    }

    //Draw the hitbox in scene view
    private void OnDrawGizmos()
    {
        if (!viewHitBoxes) { return; };

        // Draw box


        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

        for(int i = 0; i < hitBoxFrames.Count; i++)
        {
            switch(i % 3)
            {
                case 0:
                    Gizmos.color = new Color(0.7f, 0.1f, 0.1f, 0.3f);
                    break;

                case 1:
                    Gizmos.color = new Color(0.1f, 0.7f, 0.1f, 0.3f);
                    break;

                case 2:
                    Gizmos.color = new Color(0.1f, 0.1f, 0.7f, 0.3f);
                    break;
            }

            Gizmos.DrawCube((Vector3)hitBoxFrames[i].center, (Vector3)(hitBoxFrames[i].extents * 2)); // Because size is halfExtents
        }        

    }
}
