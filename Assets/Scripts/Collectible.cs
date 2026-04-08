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
    public float maxLifetime = 15f;

    [Header("Visual Motion")]
    public float candySpinSpeed  = 120f;  // degrees/sec constant spin for candies
    public float wiggleAngle     = 18f;   // peak angle for trick wiggle
    public float wiggleFrequency = 2.5f;  // wiggles per second

    private Rigidbody2D rb;
    private bool  collected  = false;
    private float lifeTimer  = 0f;
    private float wiggleTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.bodyType     = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        if (GameManager.Instance != null)
            fallSpeed = GameManager.Instance.candyFallSpeedBase;

        rb.linearVelocity = Vector2.down * fallSpeed;

        // Randomise spin direction for variety
        if (!isTrick)
            candySpinSpeed *= Random.value > 0.5f ? 1f : -1f;
    }

    void Update()
    {
        lifeTimer  += Time.deltaTime;
        wiggleTime += Time.deltaTime;

        // Visual motion
        if (!isTrick)
        {
            // Candies: constant rotation
            transform.Rotate(0f, 0f, candySpinSpeed * Time.deltaTime);
        }
        else
        {
            // Tricks: sine-wave wiggle
            float angle = Mathf.Sin(wiggleTime * wiggleFrequency * Mathf.PI * 2f) * wiggleAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

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
