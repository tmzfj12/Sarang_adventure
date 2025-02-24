using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float groundCheckRadius = 0.2f;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Transform groundCheck;

    [Header("Idle Settings")]
    public float idleTimeToSit = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool facingRight = true;
    private float moveInput;
    private bool isGrounded;
    private float idleTimer = 0f;
    private bool isHurt = false;
    private bool isAttacking = false;
    private bool isSitting = false;

    // 애니메이션 파라미터 이름들
    private readonly string SPEED_PARAM = "Speed";
    private readonly string IS_GROUNDED_PARAM = "isGrounded";
    private readonly string IS_RUNNING_PARAM = "isRunning";
    private readonly string IS_HURT_PARAM = "isHurt";
    private readonly string IS_LIE_DOWN_PARAM = "isLieDown";
    private readonly string SITTING_PARAM = "Sitting";
    private readonly string JUMP_TRIGGER = "Jump";
    private readonly string ATTACK_TRIGGER = "Attack";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool(IS_GROUNDED_PARAM, isGrounded);

        // Handle animations
        HandleAnimations();

        // Handle jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isSitting && !IsLieDown())
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

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (!IsLieDown() && !isSitting)
        {
            bool isWallAhead = IsWallAhead();

            if (!isWallAhead)
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
                // 벽에 닿았을 때 수평 속도를 0으로
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    bool IsWallAhead()
    {
        if (Mathf.Abs(moveInput) < 0.1f) return false;

        // 이동 방향에 따라 레이캐스트 방향 결정
        Vector2 direction = (moveInput > 0) ? Vector2.right : Vector2.left;

        Vector2[] startPositions = new Vector2[]
        {
        transform.position + Vector3.up * 0.2f,    // 상단
        transform.position,                        // 중간
        transform.position + Vector3.down * 0.2f   // 하단
        };

        foreach (Vector2 startPos in startPositions)
        {
            RaycastHit2D hit = Physics2D.Raycast(startPos, direction, wallCheckDistance, wallLayer);

            // 디버그 시각화
            Debug.DrawRay(startPos, direction * wallCheckDistance, Color.red, 0.1f);
            Debug.Log($"Checking direction: {direction}, Position: {startPos}, Hit: {(hit.collider != null ? hit.collider.name : "Nothing")}");

            if (hit.collider != null)
            {
                return true;
            }
        }

        return false;
    }

    void HandleAnimations()
    {
        animator.SetFloat(SPEED_PARAM, Mathf.Abs(moveInput));
        bool isRunning = Mathf.Abs(moveInput) > 0.1f;
        animator.SetBool(IS_RUNNING_PARAM, isRunning);

        if (Mathf.Abs(moveInput) < 0.1f && isGrounded && !IsLieDown())
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTimeToSit && !isSitting)
            {
                isSitting = true;
                animator.SetBool(SITTING_PARAM, true);
            }
        }
        else
        {
            idleTimer = 0f;
            if (isSitting)
            {
                isSitting = false;
                animator.SetBool(SITTING_PARAM, false);
            }
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
        if (isSitting)
        {
            isSitting = false;
            animator.SetBool(SITTING_PARAM, false);
        }
    }

    void Attack()
    {
        if (!isSitting && !IsLieDown())
        {
            isAttacking = true;
            animator.SetTrigger(ATTACK_TRIGGER);
            Invoke(nameof(ResetAttack), 0.5f);
        }
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
        yield return new WaitForSeconds(0.5f);
        isHurt = false;
        animator.SetBool(IS_HURT_PARAM, false);
    }

    void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        // Ground check visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // Wall check visualization
        Gizmos.color = Color.red;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2[] startPositions = new Vector2[]
        {
            transform.position + Vector3.up * 0.5f,
            transform.position,
            transform.position + Vector3.down * 0.5f
        };

        foreach (Vector2 startPos in startPositions)
        {
            Gizmos.DrawLine(startPos, startPos + direction * wallCheckDistance);
        }
    }
}