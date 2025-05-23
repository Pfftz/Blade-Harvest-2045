using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    public Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        // Configure Rigidbody2D for top-down movement
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    void Update()
    {
        // Get input for both horizontal and vertical movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        // Create a direction vector
        moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        
        // Update facing direction based on movement
        UpdateFacingDirection();
    }
    
    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics consistency
        rb.velocity = moveDirection * speed;
    }
    
    void UpdateFacingDirection()
    {
        if (moveDirection != Vector2.zero)
        {
            // Determine the rotation based on movement direction
            if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
            {
                // Horizontal movement is dominant
                if (moveDirection.x > 0)
                {
                    // Moving right
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else
                {
                    // Moving left
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }
            else
            {
                // Vertical movement is dominant
                if (moveDirection.y > 0)
                {
                    // Moving up
                    transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                }
                else
                {
                    // Moving down
                    transform.rotation = Quaternion.Euler(0f, 0f, 270f);
                }
            }
        }
    }
}