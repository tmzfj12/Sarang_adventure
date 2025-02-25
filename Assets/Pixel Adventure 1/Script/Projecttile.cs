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

        // ��������Ʈ ���� ����
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

    // OnTriggerEnter2D ��� OnCollisionEnter2D ���
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // ������ ������ �ֱ�
            EnemyBehavior enemy = collision.gameObject.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                // �÷��̾� ���� �������� �˹� (����ü ���� ����)
                Vector2 knockbackDir = rb.velocity.normalized;
                enemy.TakeDamage(damage, knockbackDir);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag != "Player" && collision.gameObject.tag != "DeadEnemy")
        {
            // �÷��̾ �̹� ���� ���� �ƴ� �ٸ� �Ͱ� �浹�ϸ� �ı�
            Destroy(gameObject);
        }
    }

    // ��� ��Ŀ�������� Ʈ���� �浹�� ó��
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Ʈ���� �浹 ����: " + other.gameObject.name);

        if (other.gameObject.tag != "Player")
        {
            Destroy(gameObject);
        }
    }

}
