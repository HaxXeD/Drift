using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WheelElements{
    public Transform leftWheel;
    public Transform rightWheel;

    public GameObject leftTrail;
    public GameObject rightTrail;

    public WheelCollider leftWheelCollider;
    public WheelCollider rightWheelCollider;

    public bool addWheelTorque;
    public bool shouldSteer;
    public bool addBreakTorque;

    public bool hasTrail;
}

public class CarController : MonoBehaviour
{
    private CarInputs carInputs;
    private CarInputs.CarActions Car;
    private Rigidbody carRigidBody;


    [SerializeField] List<WheelElements> wheelData;
    [SerializeField] private float _maxTorque = 700;
    [SerializeField] private float _maxSteerAngle = 60;

    [SerializeField] private float _maxBreakingTorque = 500;

    private Vector2 playerInputs;
    private bool breakActive;

    [SerializeField] private float lerpSpeed = 2f;
    private float lerpTime;


    private void OnEnable(){
        Car.Enable();
    }

    private void Awake(){
        carInputs = new CarInputs();
        Car = carInputs.Car;
    }

    private void Start(){
        Car.Break.started += ContextMenu => breakActive = true;
        Car.Break.canceled += ContextMenu => breakActive = false;

        foreach(WheelElements elements in wheelData){
            elements.leftTrail.SetActive(elements.hasTrail);
            elements.rightTrail.SetActive(elements.hasTrail);
        }
    }

    private void FixedUpdate(){
        playerInputs = Car.CarMovements.ReadValue<Vector2>();

        float speed = playerInputs.y * _maxTorque;
        float steer = playerInputs.x * _maxSteerAngle;
        float breakValue = breakActive?1:0;
        float breaking = breakValue * _maxBreakingTorque;
        Debug.Log(breaking);

        foreach(WheelElements elements in wheelData)
        {
            if(elements.addWheelTorque){
                elements.leftWheelCollider.motorTorque = speed;
                elements.rightWheelCollider.motorTorque = speed;
            }

            if(elements.shouldSteer){
                lerpTime += Time.fixedDeltaTime;
                float percentCompleted = lerpTime/lerpSpeed;

                elements.leftWheelCollider.steerAngle = Mathf.Lerp(elements.leftWheelCollider.steerAngle, steer, percentCompleted);
                elements.rightWheelCollider.steerAngle = Mathf.Lerp(elements.rightWheelCollider.steerAngle, steer, percentCompleted);
            }

            if(elements.addBreakTorque){
                elements.leftWheelCollider.brakeTorque = breaking;
                elements.rightWheelCollider.brakeTorque = breaking;
            }

            if(elements.hasTrail){
                elements.leftTrail.SetActive(elements.leftWheelCollider.isGrounded);
                elements.rightTrail.SetActive(elements.rightWheelCollider.isGrounded);
            }

            Debug.Log(elements.leftWheelCollider.brakeTorque + elements.rightWheelCollider.brakeTorque);

            RotateWheel(elements.leftWheelCollider, elements.leftWheel);
            RotateWheel(elements.rightWheelCollider, elements.rightWheel);
        }

    }

    private void RotateWheel(WheelCollider col, Transform transform){
        Vector3 position;
        Quaternion rotation;

        col.GetWorldPose(out position, out rotation);

        transform.position = position;
        transform.rotation = rotation;
    }
    private void OnDisable(){
        Car.Disable();
    }
}
