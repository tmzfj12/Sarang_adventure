using UnityEngine;
using System.Collections;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyType enemyType;
    public float moveSpeed = 3f;
    public int damageAmount = 1;
    public int health = 1;
    public float changeDirectionCooldown = 0.5f;

    [Header("Knockback Settings")]
    public float knockbackForce = 3f;
    public float knockbackDuration = 0.2f;

    [Header("Patrol Settings")]
    public bool startMovingRight = true;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool movingRight;
    private bool canChangeDirection = true;
    private float directionCooldownTimer = 0f;
    private bool isDead = false;
    private bool isKnockedBack = false;

    // 애니메이션 파라미터 이름
    private readonly string HURT_PARAM = "Hurt";
    private readonly string DEATH_PARAM = "Death";

    public enum EnemyType
    {
        Cat,
        Dog
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        movingRight = startMovingRight;

        // 적 타입에 따른 설정
        switch (enemyType)
        {
            case EnemyType.Cat:
                // 고양이는 약간 빠르게 설정
                moveSpeed *= 1.2f;
                break;
            case EnemyType.Dog:
                // 개는 체력이 더 높게 설정
                health = 2;
                break;
        }

        // 초기 방향에 따라 스프라이트 플립
        FlipSprite();
    }

    void Update()
    {
        if (isDead || isKnockedBack)
            return;

        // 방향 전환 쿨다운 관리
        if (!canChangeDirection)
        {
            directionCooldownTimer -= Time.deltaTime;
            if (directionCooldownTimer <= 0f)
            {
                canChangeDirection = true;
            }
        }

        // 이동 처리
        float direction = movingRight ? 1 : -1;
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead)
            return;

        // 벽이나 다른 적과 충돌했을 때 방향 전환
        if (canChangeDirection && (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Enemy")))
        {
            ChangeDirection();
        }

        // 플레이어와 충돌했을 때 데미지 주기
        if (collision.gameObject.CompareTag("Player"))
        {
            HealthSystem playerHealth = collision.gameObject.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                // 넉백 방향 계산 (적으로부터 플레이어 방향으로)
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;

                // 살짝 위로 뜨도록 y 값 조정
                knockbackDir.y = Mathf.Max(0.5f, knockbackDir.y);

                playerHealth.TakeDamage(damageAmount, knockbackDir);
            }
        }
    }

    // 투사체에 맞았을 때 호출될 메서드
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            // 피격 효과와 넉백
            if (animator != null)
            {
                animator.SetTrigger(HURT_PARAM);
            }

            // 넉백 방향 설정 (현재 이동 방향의 반대)
            Vector2 knockbackDir = movingRight ? Vector2.left : Vector2.right;
            knockbackDir.y = 0.5f; // 약간 위로 튀도록

            // 넉백 적용
            StartCoroutine(ApplyKnockback(knockbackDir));
        }
    }

    IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;

        // 현재 이동 중지
        float originalSpeed = moveSpeed;
        moveSpeed = 0;

        // 넉백 적용
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(knockbackDuration);

        // 이동 속도 복구
        moveSpeed = originalSpeed;
        isKnockedBack = false;
    }

    void Die()
    {
        isDead = true;

        // 물리 상호작용 비활성화
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 충돌 비활성화 (트리거로 변경하여 통과 가능하게)
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }

        // 사망 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger(DEATH_PARAM);

            // 애니메이션 길이 가져오기 시도
            float deathAnimLength = 1f; // 기본값

            // 현재 상태의 애니메이션 길이 가져오기
            try
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                deathAnimLength = stateInfo.length;
            }
            catch
            {
                // 예외 발생 시 기본값 사용
                deathAnimLength = 1f;
            }

            // 애니메이션 재생 후 오브젝트 파괴
            Destroy(gameObject, deathAnimLength);
        }
        else
        {
            // 애니메이터가 없으면 약간의 딜레이 후 파괴
            Destroy(gameObject, 0.5f);
        }

        // 태그 변경으로 더 이상 적이 아님을 표시 (선택사항)
        gameObject.tag = "DeadEnemy";
    }

    void ChangeDirection()
    {
        if (canChangeDirection && !isDead && !isKnockedBack)
        {
            movingRight = !movingRight;
            FlipSprite();

            // 방향 전환 쿨다운 설정
            canChangeDirection = false;
            directionCooldownTimer = changeDirectionCooldown;
        }
    }

    void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !movingRight;
        }
    }

    // 가시성을 위한 기즈모 그리기
    void OnDrawGizmos()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }
}