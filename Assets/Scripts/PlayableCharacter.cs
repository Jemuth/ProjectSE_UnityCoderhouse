using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class PlayableCharacter : GameCharacter
{
    [SerializeField] private float playerRotationSpeed = 60;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayableCharacterData m_data;

    // Velocity and acceleration parameters
    float velocityZ = 0.0f;
    float velocityX = 0.0f;
    public float acceleration = 2f;
    public float deceleration = 2f;
    public float maxWalkVelocity = 0.5f;
    public float maxRunVelocity = 2f;
    int VelocityZHash;
    int VelocityXHash;

    // Run and stamina bar parameters
    public StaminaBar m_staminaStatus;
    public bool runEnabled;
    [SerializeField] private float m_baseWalkSpeed = 0.5f;
    [SerializeField] private float m_baseRunSpeed = 2f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        VelocityZHash = Animator.StringToHash("Velocity Z");
        VelocityXHash = Animator.StringToHash("Velocity X");
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = !hasFocus;
        Cursor.lockState = hasFocus ? CursorLockMode.None : CursorLockMode.Confined;
    }
    public virtual float GetCurrentSpeed()
    {
        return m_data.speedMultiplier;
    }

    public void PlayerMovement()
    {
        var totalWalkSpeed = m_baseWalkSpeed * m_data.speedMultiplier * Time.deltaTime;
        var totalRunSpeed = m_baseRunSpeed * m_data.speedMultiplier * Time.deltaTime;

        // Walking

        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            transform.position += transform.forward * totalWalkSpeed;
        }
        if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
        {
            transform.position -= transform.forward * totalWalkSpeed;
        }
        if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
        {
            transform.position += transform.right * totalWalkSpeed;
        }
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            transform.position -= transform.right * totalWalkSpeed;
        }
        // Walking

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.S))
        {
            transform.position += transform.forward * totalRunSpeed;
        }
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.W))
        {
            transform.position -= transform.forward * totalRunSpeed;
        }
        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.A))
        {
            transform.position += transform.right * totalRunSpeed;
        }
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.D))
        {
            transform.position -= transform.right * totalRunSpeed;
        }

    }

    private void ChangeVelocity(bool forwardPressed, bool rightPressed, bool leftPressed, bool backPressed, bool runPressed, float currentMaxVelocity)
    {
        // Increase velocity according to inputs
        // Increase velocityZ going forward
        if (forwardPressed && velocityZ < currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * acceleration;
        }
        // Decrease velocityZ if forward is not pressed
        if (!forwardPressed && velocityZ > 0.0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }
        // Increase velocityZ going backwards
        if (backPressed && velocityZ > -currentMaxVelocity)
        {
            velocityZ -= Time.deltaTime * acceleration;
        }
        // Decrease velocityZ if backrward is not pressed
        if (!backPressed && velocityZ < 0.0f)
        {
            velocityZ += Time.deltaTime * deceleration;
        }
        // Increase velocityX going right
        if (rightPressed && velocityX < currentMaxVelocity)
        {
            velocityX += Time.deltaTime * acceleration;
        }
        // Increase velocityX going left
        if (leftPressed && velocityX > -currentMaxVelocity)
        {
            velocityX -= Time.deltaTime * acceleration;
        }
        // Increase velocityX if left is not pressed and velocityX < 0
        if (!leftPressed && velocityX < 0.0f)
        {
            velocityX += Time.deltaTime * deceleration;
        }
        // Decrease velocityX if right is not pressed and velocityX > 0
        if (!rightPressed && velocityX > 0.0f)
        {
            velocityX -= Time.deltaTime * deceleration;
        }

    }

    private void LockOrResetVelocity(bool forwardPressed, bool rightPressed, bool leftPressed, bool backPressed, bool runPressed, float currentMaxVelocity)
    {
         // Reset velocities
        // Reset velocityZ
        if (!forwardPressed && !backPressed && velocityZ != 0.0f && (velocityZ > -0.05f && velocityZ < 0.05f))
        {
            velocityZ = 0.0f;
        }
        // Reset velocityX
        if (!leftPressed && !rightPressed && velocityZ != 0.0f && (velocityX > -0.05f && velocityX < 0.05f))
        {
            velocityX = 0.0f;
        }

        // Locking speeds acording to max velocity
        // Lock forward speed
        if (forwardPressed && runPressed && velocityZ > currentMaxVelocity)
        {
            velocityZ = currentMaxVelocity;
        }
        // Decelerate to walk speed
        else if (forwardPressed && velocityZ > currentMaxVelocity)
        {
            velocityZ -= Time.deltaTime * deceleration;
            // Round to the current max velocity if within offset
            if (velocityZ > currentMaxVelocity && velocityZ < (currentMaxVelocity + 0.05f))
            {
                velocityZ = currentMaxVelocity;
            }
        }
        // Round to the current max velocity if within offset
        else if (forwardPressed && velocityZ < currentMaxVelocity && velocityZ > (currentMaxVelocity - 0.05f))
        {
            velocityZ = currentMaxVelocity;
        }
        // Lock backward speed
        if (backPressed && runPressed && velocityZ < -currentMaxVelocity)
        {
            velocityZ = -currentMaxVelocity;
        }
        // Decelerate to walk speed
        else if (backPressed && velocityZ < -currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * deceleration;
            // Round to the current max velocity if within offset
            if (velocityZ < -currentMaxVelocity && velocityZ > (-currentMaxVelocity - 0.05f))
            {
                velocityZ = -currentMaxVelocity;
            }
        }
        // Round to the current max velocity if within offset
        else if (backPressed && velocityZ > -currentMaxVelocity && velocityZ < (-currentMaxVelocity + 0.05f))
        {
            velocityZ = -currentMaxVelocity;
        }
        // Locking left speed
        if (leftPressed && runPressed && velocityX < -currentMaxVelocity)
        {
            velocityX = -currentMaxVelocity;
        }
        // Decelerate to walk speed
        else if (leftPressed && velocityX < -currentMaxVelocity)
        {
            velocityX += Time.deltaTime * deceleration;
            // Round to the current max velocity if within offset
            if (velocityX < -currentMaxVelocity && velocityX > (-currentMaxVelocity - 0.05f))
            {
                velocityX = -currentMaxVelocity;
            }
        }
        // Round to the current max velocity if within offset
        else if (leftPressed && velocityX > -currentMaxVelocity && velocityX < (-currentMaxVelocity + 0.05f))
        {
            velocityX = -currentMaxVelocity;
        }
        // Locking right speed
        if (rightPressed && runPressed && velocityX > currentMaxVelocity)
        {
            velocityX = currentMaxVelocity;
        }
        // Decelerate to walk speed
        else if (rightPressed && velocityX > currentMaxVelocity)
        {
            velocityX -= Time.deltaTime * deceleration;
            // Round to the current max velocity if within offset
            if (velocityX > currentMaxVelocity && velocityX < (currentMaxVelocity + 0.05f))
            {
                velocityX = currentMaxVelocity;
            }
        }
        // Round to the current max velocity if within offset
        else if (rightPressed && velocityX < currentMaxVelocity && velocityX > (currentMaxVelocity - 0.05f))
        {
            velocityX = currentMaxVelocity;
        }
    }
    
    private Vector3 GetPlayerRotation()
    {
        var l_mouseX = Input.GetAxis("Mouse X");
        var l_mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(l_mouseX, l_mouseY);
    }
   
    private void RotatePlayer(Vector2 p_scrollDelta)
    {
        transform.Rotate(Vector3.up, p_scrollDelta.x * playerRotationSpeed * Time.deltaTime, Space.Self);
    }
    private void RunCooldown()
    {
        if (m_staminaStatus.currentStamina < m_data.maxStamina * 5 / 100)
        {
            runEnabled = false;
        }
        else
        {
            runEnabled = true;
        }
    }
    private void UseStamina()
    {
        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && runEnabled == true && velocityZ > 0)
        {
            
            StaminaBar.instance.UseStamina(2);

        }
    }

    void Update()
    {
        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool backPressed = Input.GetKey(KeyCode.S);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);
        float currentMaxVelocity = runPressed ? maxRunVelocity : maxWalkVelocity;
        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);

        PlayerMovement();
        ChangeVelocity(forwardPressed, rightPressed, leftPressed, backPressed, runPressed, currentMaxVelocity);
        LockOrResetVelocity(forwardPressed, rightPressed, leftPressed, backPressed, runPressed, currentMaxVelocity);
        RotatePlayer(GetPlayerRotation());
        //UseStamina();
        //RunCooldown();

    }
}
