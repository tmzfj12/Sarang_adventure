using UnityEngine;
using System.Collections;

public class PowerUpItem : MonoBehaviour
{
    [Header("Power-up Settings")]
    public PowerUpType powerUpType;
    public int healAmount = 1;

    [Header("Visual Effects")]
    public float bobHeight = 0.5f;
    public float bobSpeed = 2f;
    public float rotationSpeed = 45f;
    public bool useRotation = false;
    public Color glowColor = Color.white;

    [Header("Pickup Effects")]
    public float destroyDelay = 0.1f;
    public float scaleSpeed = 5f;

    private Vector3 startPos;
    private float timeOffset;
    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;
    private bool isBeingCollected = false;

    public enum PowerUpType
    {
        HealthSnack,
        AttackSnack
    }

    void Start()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null || itemCollider == null)
        {
            Debug.LogWarning("Required components missing on PowerUpItem!");
        }
    }

    void Update()
    {
        if (!isBeingCollected)
        {
            // 상하 움직임
            float newY = startPos.y + Mathf.Sin((Time.time + timeOffset) * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 회전 효과
            if (useRotation)
            {
                transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isBeingCollected && other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                bool shouldCollect = false;

                switch (powerUpType)
                {
                    case PowerUpType.HealthSnack:
                        // 체력 아이템은 언제나 획득 가능
                        shouldCollect = true;
                        // 최대 체력이 아닐 때만 실제로 회복
                        if (playerHealth.currentHealth < playerHealth.maxHealth)
                        {
                            playerHealth.Heal(healAmount);
                        }
                        break;

                    case PowerUpType.AttackSnack:
                        playerHealth.EnableAttack();
                        shouldCollect = true;
                        break;
                }

                if (shouldCollect)
                {
                    StartCoroutine(CollectEffect());
                }
            }
        }
    }

    private IEnumerator CollectEffect()
    {
        isBeingCollected = true;

        // 아이템 획득 효과
        float elapsed = 0;
        Vector3 originalScale = transform.localScale;

        while (elapsed < destroyDelay)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / destroyDelay;

            // 크기 증가 및 페이드 아웃
            transform.localScale = originalScale * (1 + progress * scaleSpeed);
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1 - progress;
                spriteRenderer.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 upperPoint = transform.position + Vector3.up * bobHeight;
        Vector3 lowerPoint = transform.position - Vector3.up * bobHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(lowerPoint, upperPoint);
        Gizmos.DrawWireSphere(upperPoint, 0.1f);
        Gizmos.DrawWireSphere(lowerPoint, 0.1f);
    }
}