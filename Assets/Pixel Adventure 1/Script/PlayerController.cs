using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    [Header("Idle Settings")]
    public float idleTimeToSit = 5f; // 앉기 전까지의 대기 시간

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;
    private float moveInput;
    private bool isGrounded;
    private float idleTimer = 0f;
    private bool isHurt = false;
    private bool isAttacking = false;

    // 애니메이션 파라미터 이름들
    private readonly string SPEED_PARAM = "Speed";
    private readonly string IS_GROUNDED_PARAM = "isGrounded";
    private readonly string IS_RUNNING_PARAM = "isRunning";
    private readonly string IS_HURT_PARAM = "isHurt";
    private readonly string IS_LIE_DOWN_PARAM = "isLieDown";
    private readonly string JUMP_TRIGGER = "Jump";
    private readonly string ATTACK_TRIGGER = "Attack";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool(IS_GROUNDED_PARAM, isGrounded);

        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Handle movement and animations
        HandleMovement();
        HandleAnimations();

        // Handle jump
        if (Input.GetButtonDown("Jump") && isGrounded && !IsLieDown())
        {
            Jump();
        }

        // Handle attack
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isAttacking)
        {
            Attack();
        }

        // Handle lie down
        HandleLieDown();
    }

    void HandleMovement()
    {
        if (!IsLieDown())
        {
            float moveVelocity = moveInput * moveSpeed;
            rb.linearVelocity = new Vector2(moveVelocity, rb.linearVelocity.y);

            // Flip character based on movement direction
            if (moveInput > 0 && !facingRight)
                Flip();
            else if (moveInput < 0 && facingRight)
                Flip();
        }
        else
        {
            // When lying down, stop horizontal movement
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void HandleAnimations()
    {
        // Set speed parameter
        animator.SetFloat(SPEED_PARAM, Mathf.Abs(moveInput));

        // Handle running animation
        bool isRunning = Mathf.Abs(moveInput) > 0.1f;
        animator.SetBool(IS_RUNNING_PARAM, isRunning);

        // Handle idle timer for sitting animation
        if (Mathf.Abs(moveInput) < 0.1f && isGrounded && !IsLieDown())
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTimeToSit)
            {
                // Transition to sitting will be handled by animator
                idleTimer = 0f;
            }
        }
        else
        {
            idleTimer = 0f;
        }
    }

    void HandleLieDown()
    {
        bool lieDownInput = Input.GetKey(KeyCode.DownArrow);
        animator.SetBool(IS_LIE_DOWN_PARAM, lieDownInput);
    }

    bool IsLieDown()
    {
        return animator.GetBool(IS_LIE_DOWN_PARAM);
    }

    void Jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
        animator.SetTrigger(JUMP_TRIGGER);
        idleTimer = 0f;
    }

    void Attack()
    {
        isAttacking = true;
        animator.SetTrigger(ATTACK_TRIGGER);
        Invoke(nameof(ResetAttack), 0.5f);
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void Flip()
    {
        facingRight = !facingRight;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
        else
        {
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }

    public void TakeDamage()
    {
        if (!isHurt)
        {
            StartCoroutine(HurtCoroutine());
        }
    }

    IEnumerator HurtCoroutine()
    {
        isHurt = true;
        animator.SetBool(IS_HURT_PARAM, true);

        // 피격 시 잠시 무적 및 넉백 효과
        yield return new WaitForSeconds(0.5f);

        isHurt = false;
        animator.SetBool(IS_HURT_PARAM, false);
    }

    void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ground 레이어와의 충돌 확인
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            animator.SetBool(IS_GROUNDED_PARAM, true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Ground 레이어와의 충돌 해제 확인
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
            animator.SetBool(IS_GROUNDED_PARAM, false);
        }
    }
}