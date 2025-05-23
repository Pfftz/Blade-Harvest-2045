using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float speed;
    public Animator animator;
    private bool isRunning = false;
    private Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        direction = new Vector3(horizontal, vertical).normalized;

        // Check if shift is pressed for running
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed = runSpeed;
            isRunning = true;
        }
        else
        {
            speed = walkSpeed;
            isRunning = false;
        }

        AnimateMovement(direction);
    }

    private void FixedUpdate()
    {
        // This is where you would handle physics-based movement
        transform.position += direction * speed * Time.deltaTime;
    }

    void AnimateMovement(Vector3 direction){
        if(animator != null){
            if (direction.magnitude > 0)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", isRunning);
                animator.SetFloat("horizontal", direction.x);
                animator.SetFloat("vertical", direction.y);
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
            }
        }
    }
}
