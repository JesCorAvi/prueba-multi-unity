using UnityEngine;
using Mirror;

public class PlayerFPSController : NetworkBehaviour
{
    [Header("Referencias")]
    public CharacterController controller;
    public Camera playerCamera;
    public Transform playerBody;

    [Header("Ajustes de Movimiento")]
    public float speed = 10f;
    public float mouseSensitivity = 150f;

    private float xRotation = 0f;

    public override void OnStartLocalPlayer()
    {
        // Solo activamos la cámara y bloqueamos el ratón si ESTE es nuestro jugador
        playerCamera.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Si no somos el dueño de este jugador, no podemos controlarlo
        if (!isLocalPlayer) return;

        HandleMouseLook();
        HandleMovement();

        // Presiona ESC para liberar el ratón en el editor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotar cámara arriba/abajo
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Rotar cuerpo del jugador izquierda/derecha
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = playerBody.right * x + playerBody.forward * z;
        controller.Move(move * speed * Time.deltaTime);
        
        // Aquí podrías añadir gravedad si es necesario
    }
}