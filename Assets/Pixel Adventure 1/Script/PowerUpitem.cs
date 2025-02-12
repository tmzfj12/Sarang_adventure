using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    [Header("Power-up Settings")]
    public PowerUpType powerUpType;
    public int healAmount = 1;

    [Header("Visual Effects")]
    public float bobHeight = 0.5f;
    public float bobSpeed = 2f;

    private Vector3 startPos;
    private float timeOffset;

    public enum PowerUpType
    {
        HealthSnack,
        AttackSnack
    }

    void Start()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // Make the item bob up and down
        float newY = startPos.y + Mathf.Sin((Time.time + timeOffset) * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                switch (powerUpType)
                {
                    case PowerUpType.HealthSnack:
                        playerHealth.Heal(healAmount);
                        break;
                    case PowerUpType.AttackSnack:
                        playerHealth.EnableAttack();
                        break;
                }

                // Play pickup effect here
                // AudioManager.Instance.PlaySound("PowerUp");

                // Destroy the power-up
                Destroy(gameObject);
            }
        }
    }
}