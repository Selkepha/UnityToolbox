// Unit.cs
/* ---------------------------------------------------------------------------- */
// Unit class.
// - Assigns attacks to controls.
// - Contains stats.
// - Sends inputs to character controller.
/* ---------------------------------------------------------------------------- */
// Author:  github.com/Raexyl
// Date:    June 2023
/* ---------------------------------------------------------------------------- */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Stats
    public int maxHealth = 100;
    public int health = 100;
    public int attack = 10;
    public int def = 2;

    // Attacks
    public List<Attack> attacks;
    private List<HitBox> m_HitBoxes = new List<HitBox>();

    public ResourceBar healthBar;

    // Components
    private CustomCharacterController m_CCC;

    // Start is called before the first frame update
    void Start()
    {
        m_CCC = GetComponent<CustomCharacterController>();

        // Scan child components for all attached attacks
        attacks = new List<Attack>();
        Attack[] foundAttacks = GetComponentsInChildren<Attack>(false);
        for(int i = 0; i < foundAttacks.Length; i++)
        {
            attacks.Add(foundAttacks[i]);
        }

        m_HitBoxes.Add(gameObject.AddComponent<HitBox>()); // Just the one hitbox for now
        // Set hitbox masks here?

        health = maxHealth; 
    }

    // Update is called once per frame
    void Update()
    {
        // Match inputs to controls
        bool a0 = Input.GetButtonDown("Fire1"); // Attack 0
        bool a1 = Input.GetButtonDown("Fire2"); // Attack 1

        if (a0) { UseAttack(0); };
        if (a1) { UseAttack(1); };

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        m_CCC.Move(x, y);
        if (Input.GetButtonDown("Jump")) { m_CCC.Jump(); };
    }

    public void UseAttack(int i)
    {
        if (i >= attacks.Count) { return; };

        attacks[i].SetHitBox(m_HitBoxes[0]);
        attacks[i].SetSource(this);
        attacks[i].Use();
    }

    /* ----- STAT METHODS ----- */

    //Take damage from an attack from a source unit. Returns the remaining health.
    public int TakeDamageFrom(Unit source, int attackDamage)
    {
        health -= attackDamage + source.GetAttack() - def;
        if (health < 0) { health = 0; };
        healthBar?.SetFill(health / (float)maxHealth); // update healthbar
        return health;
    }

    public int GetAttack()
    {
        return attack;
    }

}
