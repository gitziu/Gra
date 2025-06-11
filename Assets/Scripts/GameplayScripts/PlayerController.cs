using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform leftWallCheck;
    [SerializeField] private Transform rightWallCheck;
    private Rigidbody2D rb;
    private Animator animator;
    public bool isGrounded = false;
    private KeyCode dashKey = KeyCode.K;
    private float checkRadious = 0.2f;
    private float wallCheckRadious = 0.1f;
    public LayerMask groundLayer;

    [Header("Visual")]
    public Sprite normal;
    public Sprite dashing;
    private SpriteRenderer sr;

    [Header("Movement")]
    public float moveSpeed = 8f;
    //public float maxMoveSpeed = 12f;
    public float jumpForce = 18f;
    private bool facingRight = true;
    public float fallGravityMultiplier = 2.5f;
    public float lowJumpGravityMultiplier = 3.5f;

    [Header("Wall jump")]

    public bool touchingWall = false;
    public bool touchingRightWall = false;
    public bool wallJumpInProgress = false;
    public float wallJumpBannedDirection = 0f;
    //public bool touchedAfterWallJump = true;
    //public bool isWallJumping = false;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f;
    public bool isDashing = false;
    public int availableDash = 1;
    private float dashEndTime = 0f;

    [Header("Animations")]
    public bool isFalling;
    public bool isRunning;
    public bool isJumping;
    public bool isHurt = false;
    public float DeathDuration = 0.35f;
    public float DeathEndTime;
    public bool CurrentlyDying = false;

    void Awake()
    {
        groundCheck = transform.Find("groundCheck");
        leftWallCheck = transform.Find("leftWallCheck");
        rightWallCheck = transform.Find("rightWallCheck");
        rb = transform.GetComponent<Rigidbody2D>();
        sr = transform.Find("visual").GetComponent<SpriteRenderer>();
        sr.sprite = normal;
        animator = transform.Find("visual").GetComponent<Animator>();

    }

    public void flip()
    {
        Vector3 localScale = transform.Find("visual").localScale;
        localScale.x *= -1;
        transform.Find("visual").localScale = localScale;
    }

    void Update()
    {
        if (LevelGameplayManager.Instance.levelEnd) return;
        if (Time.time >= DeathEndTime && CurrentlyDying) respawn();
        if (CurrentlyDying) return;
        touchingWall = Physics2D.OverlapCircle(leftWallCheck.position, wallCheckRadious, groundLayer) || Physics2D.OverlapCircle(rightWallCheck.position, wallCheckRadious, groundLayer);
        touchingRightWall = Physics2D.OverlapCircle(rightWallCheck.position, wallCheckRadious, groundLayer);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadious, groundLayer);
        if (isGrounded || isDashing) wallJumpInProgress = false;
        if (isGrounded || isDashing) wallJumpBannedDirection = 0f;
        /*
        if (touchingWall)
        {
            wallJumpBannedDirection = touchingRightWall ? 1f : -1f;
        }
        */
        //Debug.Log("current time : " + Time.time);
        if (Time.time >= dashEndTime && isDashing)
        {
            isDashing = false;
            rb.gravityScale = 1f;
            sr.sprite = normal;
        }
        if (isGrounded && availableDash == 0 && !isDashing) availableDash = 1;
        if (isDashing) return;
        AdjustGravityScale();
        if (availableDash > 0 && Input.GetKeyDown(dashKey))
        {
            Dash();
        }
        if (isDashing) return;
        if (Input.GetKeyDown(KeyCode.Space) && touchingWall && !isGrounded)
        {
            flip();
            wallJumpInProgress = true;
            facingRight = !facingRight;
            Debug.Log("Definitly should be wall jumping");
            Debug.Log("velocityx : " + (touchingRightWall ? moveSpeed * -1 : moveSpeed));
            rb.linearVelocity = new Vector2(touchingRightWall ? moveSpeed * -1 : moveSpeed, jumpForce);
            //rb.linearVelocityX = (touchingRightWall ? -moveSpeed : moveSpeed);
            Debug.Log("current velocity : " + rb.linearVelocity);
        }
        float verticalVelocity = rb.linearVelocityY;
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (isGrounded)
        {
            isJumping = false;
            isFalling = false;
            isRunning = Mathf.Abs(horizontalInput) > 0; // Only consider running if grounded
        }
        else // Not grounded
        {
            isJumping = verticalVelocity > 0.1f;
            isFalling = verticalVelocity < -0.1f;
            isRunning = false; // Cannot be running if not grounded
        }
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);

        if (wallJumpInProgress) return;
        //if (isWallJumping) return;
        if (Input.GetAxisRaw("Horizontal") == -1 && facingRight)
        {
            facingRight = false;
            flip();
        }
        if (Input.GetAxisRaw("Horizontal") == 1 && !facingRight)
        {
            facingRight = true;
            flip();
        }
        //animations
        //Debug.Log("change in velocity");
        rb.linearVelocityX = (Input.GetAxisRaw("Horizontal") - wallJumpBannedDirection) * moveSpeed;
        //rb.linearVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed, rb.linearVelocityY);
        //Debug.Log("current velocity : " + rb.linearVelocity);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
        }
    }

    void AdjustGravityScale()
    {
        if (rb.linearVelocityY < 0)
        {
            rb.gravityScale = fallGravityMultiplier;
        }
        else if (rb.linearVelocityY > 0 && !Input.GetButton("Jump"))
        {
            rb.gravityScale = lowJumpGravityMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftWallCheck.position, wallCheckRadious);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rightWallCheck.position, wallCheckRadious);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadious);
    }

    private void Dash()
    {
        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (direction == Vector2.zero) direction = Vector2.up;
        availableDash -= 1;
        isDashing = true;
        sr.sprite = dashing;
        rb.gravityScale = 0;
        Debug.Log("dashing in direction : " + direction);
        rb.linearVelocity = direction * dashForce;
        Debug.Log("current velocity : " + rb.linearVelocity);
        Debug.Log("current time : " + Time.time + " dash end time : " + (Time.time + dashDuration));
        dashEndTime = Time.time + dashDuration;
    }


    public void Death()
    {
        DeathEndTime = Time.time + DeathDuration;
        CurrentlyDying = true;
        LevelGameplayManager.Instance.attempts++;
        isHurt = true;
        isFalling = false;
        isRunning = false;
        isJumping = false;
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isHurt", isHurt);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.simulated = false;
        transform.GetComponent<BoxCollider2D>().enabled = false;
    }

    public void respawn()
    {
        transform.position = LevelGameplayManager.Instance.PlayerSpawnPoint;
        CurrentlyDying = false;
        rb.simulated = true;
        transform.GetComponent<BoxCollider2D>().enabled = true;
        isHurt = false;
        rb.gravityScale = 1f;
        animator.SetBool("isHurt", isHurt);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Border") || collision.collider.CompareTag("Obstacles")) && CurrentlyDying == false)
        {
            Death();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Exit"))
        {
            LevelGameplayManager.Instance.successful++;
            LevelGameplayManager.Instance.triggerLevelEnd(true);
        }
        if (collision.gameObject.CompareTag("Collectible"))
        {
            LevelGameplayManager.Instance.collectedCollectibles++;
        }
        if (collision.gameObject.CompareTag("Enemy")) Death();
    }

}
