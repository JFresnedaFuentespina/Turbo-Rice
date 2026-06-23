using System;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform lookpoint;
    public Transform head;

    [Header("Mouse")]
    public float sensitivity = 2f;
    [Header("Look Point")]
    public float horizontalRange = 3f;
    public float verticalRange = 2f;
    public float forwardDistance = 2f;

    [Header("Head rotation")]
    public float maxHeadAngle = 60f;
    public float headSpeed = 8f;

    private float lookX;
    private float lookY;

    private Vector3 localStartPos;
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        localStartPos = lookpoint.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Movimiento ratón
        lookX += Input.GetAxis("Mouse X") * sensitivity;
        lookY += Input.GetAxis("Mouse Y") * sensitivity;

        lookX = Mathf.Clamp(lookX, -horizontalRange, horizontalRange);
        lookY = Mathf.Clamp(lookY, -verticalRange, verticalRange);

        lookpoint.localPosition = localStartPos + new Vector3(lookX, lookY, forwardDistance);
    }
    void LateUpdate()
    {
        RotateHead();
    }

    void RotateHead()
    {

        Vector3 dir = lookpoint.position - head.position;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        Quaternion localTarget = Quaternion.Inverse(head.parent.rotation) * targetRot;

        Vector3 euler = localTarget.eulerAngles;

        if (euler.x > 180) euler.x -= 360;
        if (euler.y > 180) euler.y -= 360;

        euler.x = Mathf.Clamp(euler.x, -maxHeadAngle, maxHeadAngle);
        euler.y = Mathf.Clamp(euler.y, -maxHeadAngle, maxHeadAngle);

        Quaternion limitedRot = Quaternion.Euler(euler);

        head.localRotation = limitedRot;
    }
}
