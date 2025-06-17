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
    private bool isFlashAnimationPlaying = false; // Add flag to track animation state
    
    // Add facing direction tracking
    private Vector2 facingDirection = Vector2.down; // Default facing down
    
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

        // Update facing direction when moving
        if (direction.magnitude > 0)
        {
            facingDirection = new Vector2(direction.x, direction.y).normalized;
        }

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

    // Property to check if flash animation is playing
    public bool IsFlashAnimationPlaying => isFlashAnimationPlaying;

    // Property to get facing direction
    public Vector2 FacingDirection => facingDirection;

    // Method to get the tile position in front of the player
    public Vector3Int GetTileInFront(Vector3 playerPosition)
    {
        // Convert facing direction to discrete tile offset
        Vector2Int tileOffset = Vector2Int.zero;
        
        // Determine which direction has the stronger component
        if (Mathf.Abs(facingDirection.x) > Mathf.Abs(facingDirection.y))
        {
            // Moving more horizontally
            tileOffset.x = facingDirection.x > 0 ? 1 : -1;
        }
        else
        {
            // Moving more vertically
            tileOffset.y = facingDirection.y > 0 ? 1 : -1;
        }
        
        // Fixed: Use FloorToInt for proper tile grid alignment
        Vector3Int playerTilePos = new Vector3Int(
            Mathf.FloorToInt(playerPosition.x), 
            Mathf.FloorToInt(playerPosition.y), 
            0
        );
        
        return playerTilePos + new Vector3Int(tileOffset.x, tileOffset.y, 0);
    }

    // New method to trigger flash animation
    public void TriggerFlashAnimation()
    {
        if (animator != null && !isFlashAnimationPlaying)
        {
            isFlashAnimationPlaying = true;
            animator.SetBool("isFlashOn", true);
            // Start coroutine to turn off the flash after a short duration
            StartCoroutine(TurnOffFlash());
        }
    }

    private IEnumerator TurnOffFlash()
    {
        // Wait for a short duration (adjust as needed for your animation)
        yield return new WaitForSeconds(0.5f);
        
        if (animator != null)
        {
            animator.SetBool("isFlashOn", false);
        }

        // Add a small delay to ensure animation completes before allowing next use
        yield return new WaitForSeconds(0.1f);
        isFlashAnimationPlaying = false;
    }
}
