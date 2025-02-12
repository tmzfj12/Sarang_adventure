using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI Elements")]
    public Image[] healthIcons;
    public TextMeshProUGUI stageText;

    [Header("Power-up Settings")]
    public bool canAttack = false;
    public float attackDuration = 10f;
    public GameObject attackCollider;
    private PlayerController playerController;
    private float powerUpTimer;

    void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        UpdateHealthUI();

        if (attackCollider != null)
            attackCollider.SetActive(false);
    }

    void Update()
    {
        // Handle attack input when powered up
        if (canAttack)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                Attack();
            }

            // Power-up timer
            powerUpTimer -= Time.deltaTime;
            if (powerUpTimer <= 0)
            {
                canAttack = false;
                if (attackCollider != null)
                    attackCollider.SetActive(false);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateHealthUI();
    }

    public void EnableAttack()
    {
        canAttack = true;
        powerUpTimer = attackDuration;
    }

    private void Attack()
    {
        if (attackCollider != null)
        {
            // Activate attack collider briefly
            StartCoroutine(PerformAttack());
        }
    }

    private System.Collections.IEnumerator PerformAttack()
    {
        attackCollider.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        attackCollider.SetActive(false);
    }

    private void UpdateHealthUI()
    {
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (i < currentHealth)
                healthIcons[i].enabled = true;
            else
                healthIcons[i].enabled = false;
        }
    }

    private void GameOver()
    {
        // Implement game over logic here
        Debug.Log("Game Over");
        // You might want to reload the level or show a game over screen
    }

}

