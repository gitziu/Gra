using System.Collections;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform leftWallCheck;
    [SerializeField] private Transform rightWallCheck;
    private Rigidbody2D rb;
    public bool isGrounded = false;
    private KeyCode dashKey = KeyCode.K;
    private float checkRadious = 0.1f;
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
    private float wallJumpEnd = 0f;

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

    void Awake()
    {
        groundCheck = transform.Find("groundCheck");
        leftWallCheck = transform.Find("leftWallCheck");
        rightWallCheck = transform.Find("rightWallCheck");
        rb = transform.GetComponent<Rigidbody2D>();
        sr = transform.Find("visual").GetComponent<SpriteRenderer>();
        sr.sprite = normal;
    }

    public void flip()
    {
        Vector3 localScale = transform.Find("visual").localScale;
        localScale.x *= -1;
        transform.Find("visual").localScale = localScale;
    }

    void Update()
    {
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

}
