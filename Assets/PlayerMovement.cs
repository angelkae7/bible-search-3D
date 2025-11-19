using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Saut / Gravité")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Vérifie si on touche le sol
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // garde le perso collé au sol
        }

        // Récupère les axes (Z/Q/S/D ou WASD)
        float inputX = Input.GetAxis("Horizontal"); // Q/D ou A/D
        float inputZ = Input.GetAxis("Vertical");   // Z/S ou W/S

        // Direction locale (avant / côté)
        Vector3 move = transform.right * inputX + transform.forward * inputZ;

        // Applique le mouvement sur le sol
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Saut
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravité
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
