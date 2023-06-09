// CustomCharacterController.cs
/* ---------------------------------------------------------------------------- */
// Character controller intended for tilemap, blocky 2D environments.
// Includes: -
// - Double-jumps
// - Air Influence
// - Wall jumping
// - Wall sliding
// - Acceleration / Deceleration when running
/* ---------------------------------------------------------------------------- */
// Author:  github.com/Raexyl
// Date:    June 2023
/* ---------------------------------------------------------------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]

public class CustomCharacterController : MonoBehaviour
{
    // Components
    private Transform m_T;
    private Rigidbody2D m_Rb;
    private Collider2D m_Collider;

    // Tweakables
    public float moveSpeed = 7.0f; // Maximum movement speed.
    public float jumpSpeed = 7.0f; // Size of the jump.
    public float gravity = 20.0f; // Gravity strength while moving up.
    public float fallGravity = 30.0f; // Gravity strength while falling down.
    public float airInfluence = 0.4f; // Degree of control while in the air.
    public float maxFallSpeed = 25.0f; // Maximum fall speed.
    public bool enableWallJump = true; // Enable / Disable wall jump.
    public Vector2 wallJumpVector = new Vector2(7.0f, 10.0f); // Direction of wall jump. (Assuming rightwards wall jump)
    public float maxWallDragSpeed = 4.0f; // Maximum fall speed when sliding down walls.
    public int accelerationFrames = 5; // Frames to reach full moveSpeed.
    public int decelerationFrames = 5; // Frames to stop after running.

    // Privates
    protected Vector2 m_InputMove = new Vector2(0, 0);

    private Vector2 m_Velocity = new Vector2(0, 0);

    private ContactPoint2D[] m_Contacts = new ContactPoint2D[5]; // Assuming there'll never be more contacts
    private bool m_IsGrounded;
    private bool m_IsWalledLeft;
    private bool m_IsWalledRight;
    private bool m_IsCeilinged;

    private int m_JumpCount = 2;
    protected bool m_Jump = false;
    private bool m_HasWallJumpedThisFrame = false;

    private int m_InputLockCount = 0;
    private Vector2 m_LockedVelocity;

    /* -------------------------- PUBLIC METHODS --------------------------------------- */

    // flipX indicates whether the vector should be flipped if the unit is facing the other direction.
    public void ApplyKnockback(Vector2 velocity, bool flipX)
    {
        if (!flipX)
        {
            m_Velocity = velocity;
        }
        else
        {
            m_Velocity = Vector2.Scale(velocity, (Vector2)m_T.localScale);
        }
    }

    // Use these public methods to pass inputs
    public void Move(float x, float y)
    {
        if(m_InputLockCount > 0) { return; };
        m_InputMove.x = x;
        m_InputMove.y = y;
    }
    public void Jump()
    {
        if (m_InputLockCount > 0) { return; };
        m_Jump = true;
    }

    public void LockVelocityToFor(Vector2 vel, int frames, bool flipX)
    {
        m_InputLockCount = frames;
        if(!flipX)
        {
            m_LockedVelocity = vel;
        }
        else
        {
            m_LockedVelocity = Vector2.Scale(vel, (Vector2)m_T.localScale);
        }
    }
    
    public Vector2 GetVelocity()
    {
        return m_Velocity;
    }

    /* ----- Other Methods ----- */

    // Start is called before the first frame update
    void Start()
    {
        //Assign Components
        if ((m_T = GetComponent<Transform>()) == null) { Debug.Log("Transform not found. Destroying object."); Destroy(gameObject); };
        if ((m_Rb = GetComponent<Rigidbody2D>()) == null) { Debug.Log("Rigidbody2D not found. Destroying object."); Destroy(gameObject); };
        if ((m_Collider = GetComponent<Collider2D>()) == null) { Debug.Log("Collider not found. Destroying object."); Destroy(gameObject); };

        // Rb setup
        m_Rb.gravityScale = 0; // Gravity will be applied in this script instead.
        m_Rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        m_Rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Initialising private variables
        m_IsGrounded = false;
        m_IsWalledLeft = false;
        m_IsWalledRight = false;
        m_IsCeilinged = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private Vector3 localFlip;
    void FixedUpdate()
    {
        // Apply movement controls
        ApplyInputs();

        // Face left or right
        localFlip = m_T.localScale;
        if (m_Velocity.x != 0) { localFlip.x = Mathf.Abs(localFlip.x) * Mathf.Sign(m_Velocity.x); };
        m_T.localScale = localFlip;
    }



    private void ApplyInputs()
    { 

        CheckContacts();

        ApplyGravity();
        ApplyWallLogic();

        //Movement
        Vector3 newPosition = m_T.position;

        // Ground Movement Input
        if (m_IsGrounded) 
        {
            // Accel / Decel
            if (m_InputMove.x != 0) // If accelerating
            {
                m_Velocity.x += moveSpeed * Mathf.Sign(m_InputMove.x) * (1 / (float)accelerationFrames);
            }
            else // If decelerating
            {
                float changeVel = moveSpeed * (1 / (float)decelerationFrames) * Mathf.Sign(m_Velocity.x);
                if(Mathf.Abs(changeVel) > Mathf.Abs(m_Velocity.x)) { m_Velocity.x = 0; } else { m_Velocity.x -= changeVel; };
            }
            m_Velocity.x = Mathf.Clamp(m_Velocity.x, -moveSpeed, moveSpeed);    

            m_JumpCount = 2;
        }

        if(!m_IsGrounded) // Air Influence
        {
            // Adjust jumpCount
            if(m_JumpCount == 2) { m_JumpCount = 1; };

            // Apply air influence
            float coeff = moveSpeed - Mathf.Abs(m_Velocity.x);
            if (Mathf.Sign(m_InputMove.x) != Mathf.Sign(m_Velocity.x)) { coeff = 1.0f; };
            m_Velocity.x += coeff * m_InputMove.x * airInfluence;
        }

        // Jump
        if (m_Jump && m_JumpCount > 0 && !m_HasWallJumpedThisFrame)
        {
            m_Velocity.y = jumpSpeed; m_JumpCount--;
        }

        // If velocity is locked

        if (m_InputLockCount > 0)
        {
            m_InputLockCount--;
            m_Velocity = m_LockedVelocity;
            if (m_InputLockCount == 0) { m_Velocity = Vector2.zero; };
        }

        // Update position with velocity.
        newPosition += (Vector3)(m_Velocity) * (float)Time.fixedDeltaTime;
        m_Rb.MovePosition(newPosition);

        //Reset inputs
        m_InputMove.x = 0;
        m_InputMove.y = 0;
        m_Jump = false; //Reset sticky inputs
    }

    // Check walls and apply corrections
    private void ApplyWallLogic()
    {
        // Stop when you hit walls.
        if(m_IsWalledLeft && m_Velocity.x < 0)
        {
            m_Velocity.x = 0;
        }
        if (m_IsWalledRight && m_Velocity.x > 0)
        {
            m_Velocity.x = 0;
        }

        // Wall Jump
        m_HasWallJumpedThisFrame = false;
        if (enableWallJump)
        {
            if (!m_IsGrounded && m_Jump) // must be in the air, and jump must be input
            {
                if (m_IsWalledLeft) { m_Velocity = wallJumpVector; m_HasWallJumpedThisFrame = true; };
                if (m_IsWalledRight) { m_Velocity = wallJumpVector; m_Velocity.x *= -1; m_HasWallJumpedThisFrame = true; };
            }
        }

        //Apply max wall fall speed
        if (m_IsWalledLeft && m_InputMove.x < 0 && !m_IsGrounded)
        {
            if (m_Velocity.y <= -maxWallDragSpeed) { m_Velocity.y = -maxWallDragSpeed; };
        }
        if (m_IsWalledRight && m_InputMove.x > 0 && !m_IsGrounded)
        {
            if (m_Velocity.y <= -maxWallDragSpeed) { m_Velocity.y = -maxWallDragSpeed; };
        }

        if(m_IsCeilinged)
        {
            m_Velocity.y = 0;
        }
    }

    private void ApplyGravity()
    {
        //Gravity
        if (m_IsGrounded)
        {
            m_Velocity.y = 0;
        }
        else
        {
            if (m_Velocity.y > 0 && m_InputMove.y > 0)
            {
                 m_Velocity += Vector2.down * (float)Time.fixedDeltaTime * gravity;
            }
            else
            {
                m_Velocity += Vector2.down * (float)Time.fixedDeltaTime * fallGravity;
            }
            if(m_Velocity.y < -maxFallSpeed) { m_Velocity.y = -maxFallSpeed; }; //Clamp fall velocity
        }
    }

    // Check collider contact normals to work out state of character.
    private void CheckContacts()
    {
        // Must assign each of the below
        m_IsGrounded = false;
        m_IsWalledLeft = false;
        m_IsWalledRight = false;
        m_IsCeilinged = false;

        int noOfContacts = m_Collider.GetContacts(m_Contacts);
        for (int i = 0; i < noOfContacts; i++)
        {

            if(m_Velocity.x >= 0 )
            {
                //m_IsWalledRight
                if (m_IsWalledRight == false && Vector2.Dot(m_Contacts[i].normal, Vector2.left) > 0.95f)
                {
                    m_IsWalledRight = true;
                    continue;
                }
            } 
            
            if (m_Velocity.x <= 0)
            {
                // m_IsWalledLeft
                if (m_IsWalledLeft == false && Vector2.Dot(m_Contacts[i].normal, Vector2.right) > 0.95f)
                {
                    m_IsWalledLeft = true;
                    continue;
                }
            }

            if(m_Velocity.y <= 0)
            {
                //m_IsGrounded
                if (m_IsGrounded == false && Vector2.Dot(m_Contacts[i].normal, Vector2.up) > 0.95f)
                {
                    m_IsGrounded = true;
                    continue;
                }
            }
            else if (m_Velocity.y > 0)     
            {
                //m_IsCeilinged
                if (m_IsCeilinged == false && Vector2.Dot(m_Contacts[i].normal, Vector2.down) > 0.95f)
                {
                    m_IsCeilinged = true;
                    continue;
                }
            }
        }
    }


}
