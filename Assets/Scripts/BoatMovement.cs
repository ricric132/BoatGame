using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoatMovement : MonoBehaviour
{
    Vector3 targetDirection;
    [SerializeField] GameObject targetDirectionIndicator;
    [SerializeField] Transform frontPoint;
    [SerializeField] GameObject boatIndicators;
    float steerSpeed = 30;

    [SerializeField] CameraController camController;

    public Vector3 movement;
    float maxSpeed = 5;
    float acceleration = 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        boatIndicators.transform.position = new Vector3(transform.position.x, transform.position.y + 0.75f, transform.position.z);

        if (camController.state == CameraController.CameraState.boatOverview) { 
            if (Input.GetKey(KeyCode.A))
            {
                targetDirectionIndicator.transform.Rotate(new Vector3(0, 0, steerSpeed * Time.deltaTime));
            }

            if (Input.GetKey(KeyCode.D))
            {
                targetDirectionIndicator.transform.Rotate(new Vector3(0, 0, -steerSpeed * Time.deltaTime));
            }
        }

        targetDirection = (frontPoint.position - transform.position);
        targetDirection.y = 0;
        targetDirection.Normalize();

        movement += targetDirection * acceleration;
        movement = Vector3.ClampMagnitude(movement, maxSpeed);

        transform.position += movement * Time.deltaTime;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 3 * Time.deltaTime, 0));
    }
}
