using UnityEngine;

public class FloatingArrow : MonoBehaviour
{
    [Header("Float movement")]
    public float amplitude = 0.2f;
    public float frequency = 1.5f;

    [Header("Rotation (optional)")]
    public float rotationAmplitude = 5f;
    public float rotationSpeed = 1f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        // Flotación en Y
        float floatOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0f, floatOffset, 0f);

        // Rotación suave tipo "hover retro"
        float rotOffset = Mathf.Sin(Time.time * rotationSpeed) * rotationAmplitude;
        transform.rotation = startRot * Quaternion.Euler(0f, rotOffset, 0f);
    }
}