using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAttack : Attack
{
    public Vector2 chargeVector;
    public int chargeFrames;

    override public void CollidingWith(Collider2D c)
    {
        // Check it is not the source
        if (c.GetComponent<Unit>() == source) { return; };

        // Apply damage
        m_HasHit = true;
        Unit u = c.GetComponent<Unit>();
        u?.TakeDamageFrom(source, attackDamage);

        // Deal knockback
        CustomCharacterController ccc = c.GetComponent<CustomCharacterController>();
        ccc?.ApplyKnockback(Vector2.Scale(knockBack, source.transform.localScale), false);

        Debug.Log("Damage dealt!");
    }

    override public void Use()
    {
        // Regular attack stuff
        if (m_CooldownAcc > 0.0f) { return; }; // Return if still on cooldown.
        m_CooldownAcc = cooldownTime;
        hitBox.mask = LayerMask.GetMask("Units");
        hitBox.SetResponder(this);


        if (!m_IsAttacking) { 
            StartCoroutine("ThisAttack");
            GetComponentInParent<CustomCharacterController>().LockVelocityToFor(chargeVector, chargeFrames, true);
            // Charge
        };
    }
}
