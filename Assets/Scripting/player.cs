using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Para cambiar de escena

public class player : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private AudioSource audioSource;

    public float speed = 5f;
    public float jumpForce = 10f;
    public int maxHealth = 100;
    public float knockbackForce = 5f;
    public int attackDamage = 20;
    public float attackRange = 1f;
    public float attackDuration = 0.5f;
    public AudioClip attackSound; // Sonido de ataque
    public float fallThreshold = -10f; // Umbral para morir al caer

    private int currentHealth;
    private bool isGrounded;
    private bool isAttacking;
    private bool isCrouching;
    private bool isHurt;

    private Vector3 originalScale;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); // Obtener el componente AudioSource
        currentHealth = maxHealth;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isHurt || currentHealth <= 0) return;

        float move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(move * speed, rb.velocity.y);

        if (move != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move) * originalScale.x, originalScale.y, originalScale.z);
        }
        else
        {
            transform.localScale = originalScale;
        }

        animator.SetBool("isRunning", move != 0 && !isCrouching && !isAttacking);

        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching && !isAttacking)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);
            isGrounded = false;
        }

        if (rb.velocity.y < 0 && !isGrounded)
        {
            animator.SetBool("isFalling", true);
        }
        else if (isGrounded)
        {
            animator.SetBool("isFalling", false);
            animator.SetBool("isJumping", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isAttacking)
        {
            isCrouching = true;
            animator.SetBool("isCrouching", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            animator.SetBool("isCrouching", false);
        }

        if (Input.GetKeyDown(KeyCode.F) && !isAttacking && !isHurt)
        {
            isAttacking = true;
            animator.SetTrigger("isAttacking");
            AttackEnemies();
            audioSource.PlayOneShot(attackSound); // Reproducir sonido de ataque
            StartCoroutine(ResetAttackStateAfterDelay(attackDuration));
        }

        if (move == 0 && isGrounded && !isCrouching && !isAttacking)
        {
            animator.SetBool("idle", true);
        }
        else
        {
            animator.SetBool("idle", false);
        }

        // Verificar si el personaje ha caído al vacío
        if (transform.position.y < fallThreshold && currentHealth > 0)
        {
            Die(); // Morir al caer al vacío
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            isGrounded = true;
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !isHurt)
        {
            TakeDamage(20, collision.transform.position);
        }
    }

    public void TakeDamage(int damage, Vector3 enemyPosition)
    {
        if (isHurt) return;

        currentHealth -= damage;
        isHurt = true;

        animator.SetTrigger("hurt");

        if (currentHealth <= 0)
        {
            Die(); // Morir si la salud llega a 0
        }
        else
        {
            Vector2 knockbackDirection = (transform.position - enemyPosition).normalized;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            StartCoroutine(RecoverFromHurt());
        }
    }

    private void Die()
    {
        animator.SetBool("isDead", true);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        SceneManager.LoadScene("End"); // Cambiar a la escena "End"
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(0.5f);
        isHurt = false;
        TriggerIdleAnimation(); // Activar animación de idle después de recuperarse del daño
    }

    private void AttackEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D enemy in enemiesInRange)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            }
        }
    }

    private IEnumerator ResetAttackStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        TriggerIdleAnimation(); // Activar animación de idle después del ataque
    }

    private void TriggerIdleAnimation()
    {
        animator.SetBool("idle", true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
