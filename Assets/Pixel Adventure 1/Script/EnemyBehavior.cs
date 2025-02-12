using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyType enemyType;
    public float moveSpeed = 3f;
    public int damageAmount = 1;

    [Header("Patrol Settings")]
    public float patrolDistance = 4f;
    public bool startMovingRight = true;

    private Vector3 startPosition;
    private bool movingRight;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    public enum EnemyType
    {
        Cat,
        Car,
        AggressiveDog
    }

    void Start()
    {
        startPosition = transform.position;
        movingRight = startMovingRight;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Set specific behaviors based on enemy type
        switch (enemyType)
        {
            case EnemyType.Car:
                moveSpeed *= 2f; // Cars move faster
                break;
            case EnemyType.AggressiveDog:
                damageAmount = 2; // Aggressive dogs deal more damage
                break;
        }
    }

    void Update()
    {
        if (enemyType != EnemyType.Car) // Cars only move in one direction
        {
            Patrol();
        }
        else
        {
            MoveInOneDirect();
        }
    }

    void Patrol()
    {
        if (movingRight)
        {
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
            if (transform.position.x >= startPosition.x + patrolDistance)
            {
                movingRight = false;
                FlipSprite();
            }
        }
        else
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
            if (transform.position.x <= startPosition.x - patrolDistance)
            {
                movingRight = true;
                FlipSprite();
            }
        }
    }

    void MoveInOneDirect()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        // If car moves too far left, reset its position
        if (transform.position.x < Camera.main.transform.position.x - 15f)
        {
            transform.position = new Vector3(Camera.main.transform.position.x + 15f, transform.position.y, transform.position.z);
        }
    }

    void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }

        // Handle player's attack
        if (other.CompareTag("PlayerAttack"))
        {
            Destroy(gameObject);
        }
    }
}