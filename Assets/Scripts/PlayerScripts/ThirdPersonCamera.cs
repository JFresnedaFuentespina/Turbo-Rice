using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform cameraTarget;

    public float sensitivity = 150f;
    public float minPitch = -40f;
    public float maxPitch = 70f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 rot = cameraTarget.eulerAngles;
        yaw = rot.y;
        pitch = rot.x;
    }

    void Update()
    {
        yaw += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0);
    }
}