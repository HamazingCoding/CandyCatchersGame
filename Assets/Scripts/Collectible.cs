using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Collectible : MonoBehaviour
{
    [Header("Settings")]
    public bool isTrick = false;
    public bool isBonus = false;
    public int scoreValue = 1;
    public float fallSpeed = 3f;
    public float maxLifetime = 15f; // seconds

    private Rigidbody2D rb;
    private bool collected = false;
    private float lifeTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Make sure body behaves properly
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        // Timer-based despawn
        if (!collected && lifeTimer >= maxLifetime)
        {
            collected = true;

            if (!isTrick)
                GameManager.Instance.HandleCandyMissed(this);

            Destroy(gameObject, 0f);
            return;
        }

        // Off-screen cleanup
        float bottomY = Camera.main
            .ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.main.transform.position.z))).y;

        if (!collected && transform.position.y < bottomY - 1f)
        {
            collected = true;

            if (!isTrick)
                GameManager.Instance.HandleCandyMissed(this);

            Destroy(gameObject, 0f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        if (isTrick)
            GameManager.Instance.HandleTrickCollected(this);
        else
            GameManager.Instance.HandleCandyCollected(this);

        Destroy(gameObject, 0f);
    }
}
