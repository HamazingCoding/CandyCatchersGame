using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed    = 6f;
    public float dragSpeed    = 10f;
    public float screenPadding = 0.5f;

    [Header("Tilt / Expression")]
    public float maxTiltAngle = 18f;   // max degrees of lean
    public float tiltSensitivity = 2f; // how quickly tilt reaches max at full speed
    public float tiltSmoothing = 10f;  // how fast tilt eases in/out

    private Camera       mainCam;
    private Vector3      targetPos;
    private SpriteRenderer sr;

    private float _prevX;
    private float _velocity;      // horizontal velocity this frame
    private float _currentTilt;   // current smoothed tilt angle

    void Start()
    {
        mainCam   = Camera.main;
        targetPos = transform.position;
        sr        = GetComponent<SpriteRenderer>();
        _prevX    = transform.position.x;
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleMouseDrag();
        MoveSmooth();
        ApplyExpression();
    }

    void HandleKeyboardInput()
    {
        float h = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  h = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

        if (Mathf.Abs(h) > 0.01f)
        {
            targetPos += Vector3.right * h * moveSpeed * Time.deltaTime;
            ClampToScreen();
        }
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            targetPos = new Vector3(worldPos.x, transform.position.y, transform.position.z);
            ClampToScreen();
        }
    }

    void MoveSmooth()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, dragSpeed * Time.deltaTime);
    }

    void ApplyExpression()
    {
        // Calculate real horizontal velocity from actual position change
        _velocity = (transform.position.x - _prevX) / Mathf.Max(Time.deltaTime, 0.0001f);
        _prevX    = transform.position.x;

        float speed = Mathf.Abs(_velocity);

        // Target tilt magnitude: scales with speed, clamped to max
        float targetTilt = Mathf.Clamp(speed * tiltSensitivity, 0f, maxTiltAngle);

        // Smoothly ease tilt in/out
        _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, tiltSmoothing * Time.deltaTime);

        // Flip sprite to face direction of movement
        if (speed > 0.05f)
            sr.flipX = _velocity < 0f;

        // Apply rotation — always CCW in object space.
        // When flipX is true the image is mirrored, so the same CCW angle
        // visually reads as CW from the player's perspective (matches brief).
        transform.rotation = Quaternion.Euler(0f, 0f, _currentTilt);
    }

    void ClampToScreen()
    {
        float z     = Mathf.Abs(mainCam.transform.position.z);
        Vector3 left  = mainCam.ScreenToWorldPoint(new Vector3(0,            0, z));
        Vector3 right = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 0, z));

        targetPos.x = Mathf.Clamp(targetPos.x, left.x + screenPadding, right.x - screenPadding);
    }
}
