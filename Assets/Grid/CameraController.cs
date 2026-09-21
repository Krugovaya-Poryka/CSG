using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 15f;

    public float zoomSpeed = 0.01f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector2 move = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed)
            move.y += 1;

        if (Keyboard.current.sKey.isPressed)
            move.y -= 1;

        if (Keyboard.current.aKey.isPressed)
            move.x -= 1;

        if (Keyboard.current.dKey.isPressed)
            move.x += 1;

        if (Keyboard.current.upArrowKey.isPressed)
            move.y += 1;

        if (Keyboard.current.downArrowKey.isPressed)
            move.y -= 1;

        if (Keyboard.current.leftArrowKey.isPressed)
            move.x -= 1;

        if (Keyboard.current.rightArrowKey.isPressed)
            move.x += 1;

        transform.position += new Vector3(move.x, move.y, 0).normalized * moveSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        cam.orthographicSize -= scroll * zoomSpeed;

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
}