using UnityEngine;

public class LookAndMove : MonoBehaviour
{
    public float mouseSensitivity = 200f;  // optionnel à régler dans l'inspector
  
    float yaw, pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // cache/snap la souris
    }

    void Update()
    {
        // Rotation souris
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        // Relâcher le curseur si besoin
        if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
    }
}
