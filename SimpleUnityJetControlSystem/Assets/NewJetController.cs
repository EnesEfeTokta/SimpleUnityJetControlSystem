using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewJetController : MonoBehaviour
{
    [Header("Wings")]
    [SerializeField] private Transform rightElevator;
    [SerializeField] private Transform leftElevator;
    [SerializeField] private Transform rightFlap;
    [SerializeField] private Transform leftFlap;
    [SerializeField] private Transform rightRudder;
    [SerializeField] private Transform leftRudder;

    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;

    [Header("Angles")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxFlapAngle = 45f;
    [SerializeField] private float maxElevatorAngle = 45f;
    [SerializeField] private float maxRudderAngle = 25f;
    [SerializeField] private float maxRollAngle = 30f;

    private Quaternion rightElevatorInitialRotation;
    private Quaternion leftElevatorInitialRotation;
    private Quaternion rightFlapInitialRotation;
    private Quaternion leftFlapInitialRotation;
    private Quaternion rightRudderInitialRotation;
    private Quaternion leftRudderInitialRotation;

    private Rigidbody rb;

    [Header("Values")]
    [SerializeField] private float throttleIncrement = 0.1f;
    [SerializeField] private float maxThrust = 200f;
    [SerializeField] private float responsiveness = 15000f;
    [SerializeField] private float lift = 135f;

    [Header("UIs")]
    [SerializeField] private Image engineForceValue;
    [SerializeField] private Color slowColor;
    [SerializeField] private Color middleColor;
    [SerializeField] private Color fastColor;
    [SerializeField] private TMP_Text engineForceValueText;

    private float responseModifier {
        get {
            return (rb.mass / 10) * responsiveness;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Elevator, flap ve rudderların başlangıç rotasyonlarını saklıyoruz.
        // Böylece her hangi bir aksiyon olmadığında Elevator, flap ve rudderlar eksi haline gelebilecek.
        rightElevatorInitialRotation = rightElevator.localRotation;
        leftElevatorInitialRotation = leftElevator.localRotation;
        rightFlapInitialRotation = rightFlap.localRotation;
        leftFlapInitialRotation = leftFlap.localRotation;
        rightRudderInitialRotation = rightRudder.localRotation;
        leftRudderInitialRotation = leftRudder.localRotation;
    }

    void Update()
    {
        HandleInputs();
        RotateControlSurface(rightElevator, leftElevator, maxElevatorAngle, pitch, maxRollAngle, roll, rightElevatorInitialRotation, leftElevatorInitialRotation);
        RotateControlSurface(rightFlap, leftFlap, maxFlapAngle, pitch, maxRollAngle, roll, rightFlapInitialRotation, leftFlapInitialRotation);
        RotateRudder(rightRudder, leftRudder, maxRudderAngle, yaw, rightRudderInitialRotation, leftRudderInitialRotation);
    }

    // Kalvyeden girdiler alınıyor.
    // Uçağın motoru gücü kontrolü sağlanıyor.
    void HandleInputs()
    {
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.Space))
        {
            throttle += throttleIncrement;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            throttle -= throttleIncrement;
        }

        throttle = Mathf.Clamp(throttle, 0, 100);
        UpdateThrottleUI(throttle);
    }

    void RotateControlSurface(Transform right, Transform left, float maxAngle, float primaryInput, float maxRollAngle, float rollInput, Quaternion rightInitial, Quaternion leftInitial)
    {
        float targetPrimaryAngle = maxAngle * primaryInput;
        float targetRollAngle = maxRollAngle * rollInput;

        Quaternion rightTargetRotation = Quaternion.Euler(targetPrimaryAngle, rightInitial.y, rightInitial.z);
        Quaternion leftTargetRotation = Quaternion.Euler(targetPrimaryAngle, leftInitial.y, leftInitial.z);

        right.localRotation = Quaternion.Slerp(right.localRotation, rightTargetRotation, Time.deltaTime * rotationSpeed);
        left.localRotation = Quaternion.Slerp(left.localRotation, leftTargetRotation, Time.deltaTime * rotationSpeed);

        if (rollInput != 0)
        {
            Quaternion rightRollRotation = Quaternion.Euler(-targetRollAngle, rightInitial.y, rightInitial.z);
            Quaternion leftRollRotation = Quaternion.Euler(targetRollAngle, leftInitial.y, leftInitial.z);

            right.localRotation = Quaternion.Slerp(right.localRotation, rightRollRotation, Time.deltaTime * rotationSpeed);
            left.localRotation = Quaternion.Slerp(left.localRotation, leftRollRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            right.localRotation = Quaternion.Slerp(right.localRotation, rightInitial, Time.deltaTime * rotationSpeed);
            left.localRotation = Quaternion.Slerp(left.localRotation, leftInitial, Time.deltaTime * rotationSpeed);
        }
    }

    void RotateRudder(Transform right, Transform left, float maxAngle, float yawInput, Quaternion rightInitial, Quaternion leftInitial)
    {
        float targetAngle = maxAngle * yawInput;

        Quaternion rightTargetRotation = rightInitial * Quaternion.Euler(0, targetAngle, 0);
        Quaternion leftTargetRotation = leftInitial * Quaternion.Euler(0, targetAngle, 0);

        right.localRotation = Quaternion.Slerp(right.localRotation, rightTargetRotation, Time.deltaTime * rotationSpeed);
        left.localRotation = Quaternion.Slerp(left.localRotation, leftTargetRotation, Time.deltaTime * rotationSpeed);

        if (yawInput == 0)
        {
            right.localRotation = Quaternion.Slerp(right.localRotation, rightInitial, Time.deltaTime * rotationSpeed);
            left.localRotation = Quaternion.Slerp(left.localRotation, leftInitial, Time.deltaTime * rotationSpeed);
        }
    }

    // Hareket hızına yani motor gücüne göre UI bileşenlerinin kontrolü sağlanıyor.
    void UpdateThrottleUI(float throttle)
    {
        if (throttle > 0 && throttle < 25)
        {
            engineForceValue.color = slowColor;
            engineForceValueText.color = slowColor;
        }
        else if (throttle > 25 && throttle < 75)
        {
            engineForceValue.color = middleColor;
            engineForceValueText.color = middleColor;
        }
        else
        {
            engineForceValue.color = fastColor;
            engineForceValueText.color = fastColor;
        }

        engineForceValue.fillAmount = throttle / 100;
        engineForceValueText.text = ((int)throttle).ToString();
    }

    void FixedUpdate()
    {
        rb.AddForce(transform.forward * maxThrust * throttle, ForceMode.Force); // İleri gitmede kullanılıyor...
        rb.AddTorque(transform.up * yaw * responseModifier * Time.deltaTime, ForceMode.Force); // Yükselmek ve alçalma için kullanılıyor...
        rb.AddTorque(transform.right * pitch * responseModifier * Time.deltaTime, ForceMode.Force); // Yükselme ile uçağın burnu yukarı veya aşağı döndürmekte kullanılıyor...
        rb.AddTorque(-transform.forward * roll * responseModifier * Time.deltaTime, ForceMode.Force);
        rb.AddForce(Vector3.up * rb.velocity.magnitude * lift, ForceMode.Force);
    }
}