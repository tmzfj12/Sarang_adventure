using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public int damage = 1;
    private bool isMovingRight;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(bool movingRight)
    {
        isMovingRight = movingRight;

        // 스프라이트 방향 설정
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isMovingRight;
        }
    }

    void Update()
    {
        float direction = isMovingRight ? 1 : -1;
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    // OnTriggerEnter2D 대신 OnCollisionEnter2D 사용
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 적에게 데미지 주기
            EnemyBehavior enemy = collision.gameObject.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                // 플레이어 공격 방향으로 넉백 (투사체 진행 방향)
                Vector2 knockbackDir = rb.velocity.normalized;
                enemy.TakeDamage(damage, knockbackDir);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag != "Player" && collision.gameObject.tag != "DeadEnemy")
        {
            // 플레이어나 이미 죽은 적이 아닌 다른 것과 충돌하면 파괴
            Destroy(gameObject);
        }
    }

    // 백업 메커니즘으로 트리거 충돌도 처리
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("트리거 충돌 감지: " + other.gameObject.name);

        if (other.gameObject.tag != "Player")
        {
            Destroy(gameObject);
        }
    }

}
