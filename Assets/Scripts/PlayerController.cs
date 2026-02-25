using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;         // keyboard move speed
    public float dragSpeed = 10f;        // mouse/touch drag smoothness
    public float screenPadding = 0.5f;   // how far from edges you can move

    private Camera mainCam;
    private Vector3 targetPos;

    void Start()
    {
        mainCam = Camera.main;
        targetPos = transform.position;
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleMouseDrag();
        MoveSmooth();
    }

    // --- KEYBOARD INPUT (A/D or Arrow Keys) ---
    void HandleKeyboardInput()
    {
        float h = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            h = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            h = 1f;

        if (Mathf.Abs(h) > 0.01f)
        {
            targetPos += Vector3.right * h * moveSpeed * Time.deltaTime;
            ClampToScreen();
        }
    }

    // --- DRAG INPUT (Mouse or Touch) ---
    void HandleMouseDrag()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            targetPos = new Vector3(worldPos.x, transform.position.y, transform.position.z);
            ClampToScreen();
        }
    }

    // --- SMOOTHLY MOVE ---
    void MoveSmooth()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, dragSpeed * Time.deltaTime);
    }

    // --- KEEP PLAYER INSIDE CAMERA BOUNDS ---
    void ClampToScreen()
    {
        float z = Mathf.Abs(mainCam.transform.position.z);
        Vector3 left = mainCam.ScreenToWorldPoint(new Vector3(0, 0, z));
        Vector3 right = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 0, z));

        float minX = left.x + screenPadding;
        float maxX = right.x - screenPadding;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
    }
}
