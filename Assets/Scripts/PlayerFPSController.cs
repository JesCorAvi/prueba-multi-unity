using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerFPSController : NetworkBehaviour
{
    [Header("Referencias")]
    public CharacterController controller;
    public Camera playerCamera;
    public Transform playerBody;

    [Header("Ajustes de Movimiento")]
    public float speed = 10f;
    public float mouseSensitivity = 15f; 

    private float xRotation = 0f;

    void Start()
    {
        if (!isLocalPlayer)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
            return;
        }

        // Empezamos con el cursor bloqueado para jugar
        LockCursor();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Comprobamos si el jugador quiere liberar o bloquear el ratón
        HandleCursorToggle();

        // SOLO rotamos la cámara si el cursor está bloqueado (oculto)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleMouseLook();
        }
        
        // Podemos seguir moviéndonos aunque el ratón esté libre (opcional)
        HandleMovement();
    }

    // --- NUEVO: Función para liberar/bloquear el ratón ---
    void HandleCursorToggle()
    {
        if (Keyboard.current == null) return;

        // Si presionamos la tecla ESCAPE
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor(); // Liberamos el ratón para usar los menús
            }
            else
            {
                LockCursor();   // Volvemos a ocultar el ratón para jugar
            }
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    // -----------------------------------------------------

    void HandleMouseLook()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.wKey.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed) z -= 1f;
        }

        Vector3 move = transform.right * x + transform.forward * z;

        if (controller != null)
            controller.Move(move.normalized * speed * Time.deltaTime);
    }
}