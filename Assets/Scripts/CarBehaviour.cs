using UnityEngine;
using UnityEngine.InputSystem;

public class CarBehaviour : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction brakeAction;

    [Header("Wheels")]
    public WheelCollider fl;
    public WheelCollider fr;
    public WheelCollider rl;
    public WheelCollider rr;

    [Header("Car")]
    public Rigidbody rb;
    public Transform centerOfMass;

    [Header("Motor")]
    public float motorPower = 2800f;
    public float maxSpeed = 45f;
    public float steerAngle = 32f;

    [Header("Drift")]
    public float normalGrip = 2f;
    public float driftGrip = 0.6f;
    public float driftSideForce = 18f;
    public float driftTorque = 6f;

    [Header("Brake")]
    public float handbrakePower = 6000f;

    [Header("Downforce")]
    public float downforce = 60f;

    bool drifting;

    void Start()
    {
        moveAction = inputActions.FindAction("Move");
        moveAction.Enable();

        brakeAction = inputActions.FindAction("Brake");
        brakeAction?.Enable();

        if (!rb) rb = GetComponent<Rigidbody>();

        if (centerOfMass != null)
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void FixedUpdate()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        float accel = input.y;
        float steer = input.x;

        bool handbrake = brakeAction != null && brakeAction.ReadValue<float>() > 0.1f;

        float speed = rb.linearVelocity.magnitude;

        drifting = handbrake && Mathf.Abs(steer) > 0.2f && speed > 6f;

        ApplyMotor(accel, speed);
        ApplySteering(steer, speed);
        ApplyGrip();
        ApplyDriftForce(steer, speed);
        ApplyDownforce();
        if (!drifting)
        {
            AntiRoll(fl, fr);
            AntiRoll(rl, rr);
        }
    }

    void ApplyMotor(float accel, float speed)
    {
        if (speed > maxSpeed)
        {
            SetMotor(0);
            return;
        }

        float torque = accel * motorPower;
        SetMotor(torque);
    }

    void SetMotor(float torque)
    {
        rl.motorTorque = torque;
        rr.motorTorque = torque;
        fl.motorTorque = torque * 0.5f;
        fr.motorTorque = torque * 0.5f;
    }

    void ApplySteering(float steer, float speed)
    {
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, speed);
        float angle = Mathf.Lerp(steerAngle, steerAngle * 0.4f, speedFactor);

        float finalSteer = steer * angle;

        fl.steerAngle = finalSteer;
        fr.steerAngle = finalSteer;
    }

    void ApplyGrip()
    {
        float grip = drifting ? driftGrip : normalGrip;

        SetFriction(rl, grip);
        SetFriction(rr, grip);

        SetFriction(fl, normalGrip);
        SetFriction(fr, normalGrip);
    }

    void SetFriction(WheelCollider w, float stiffness)
    {
        WheelFrictionCurve f = w.sidewaysFriction;
        f.stiffness = stiffness;
        w.sidewaysFriction = f;
    }

    void ApplyDriftForce(float steer, float speed)
    {
        if (!drifting) return;

        // limita influencia de velocidad
        float speedFactor = Mathf.InverseLerp(0f, maxSpeed, speed);

        // curva más suave (clave)
        float controlledSteer = Mathf.Lerp(steer * 0.6f, steer * 1.2f, speedFactor);

        // torque reducido y estable
        float torque = controlledSteer * driftTorque * 20f;

        // amortiguación para evitar giro infinito
        float damping = rb.angularVelocity.y * 2.5f;

        rb.AddTorque(Vector3.up * (torque - damping), ForceMode.Acceleration);
    }

    void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downforce, ForceMode.Acceleration);
    }
    void AntiRoll(WheelCollider left, WheelCollider right)
    {
        WheelHit hit;

        float travelL = 0f;
        float travelR = 0f;

        bool groundedL = left.GetGroundHit(out hit);
        if (groundedL)
        {
            float localHitY = left.transform.InverseTransformPoint(hit.point).y;
            travelL = Mathf.Clamp01(1f + (localHitY + left.radius) / left.suspensionDistance);
        }

        bool groundedR = right.GetGroundHit(out hit);
        if (groundedR)
        {
            float localHitY = right.transform.InverseTransformPoint(hit.point).y;
            travelR = Mathf.Clamp01(1f + (localHitY + right.radius) / right.suspensionDistance);
        }

        if (Mathf.Abs(travelL - travelR) < 0.05f)
            return;

        float force = travelL - travelR;

        float antiRoll = 2500f; // MUCHO más bajo

        Vector3 forceVector = transform.up * force * antiRoll;

        if (groundedL)
            rb.AddForceAtPosition(-forceVector, left.transform.position, ForceMode.Acceleration);

        if (groundedR)
            rb.AddForceAtPosition(forceVector, right.transform.position, ForceMode.Acceleration);
    }
    public void GirarCoche()
    {
        // parar rotación rara
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, 0.2f);

        // suavizar rotación hacia arriba
        Quaternion targetRotation =
            Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        transform.rotation =
            Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);

        // pequeña ayuda hacia arriba si está tocando suelo raro
        rb.AddForce(Vector3.up * 5f, ForceMode.Acceleration);
    }
}