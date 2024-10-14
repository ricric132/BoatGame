using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine;

public class BoatMovement : MonoBehaviour
{
    Vector3 targetDirection;
    [SerializeField] GameObject targetDirectionIndicator;
    [SerializeField] Transform frontPoint;
    [SerializeField] GameObject boatIndicators;
    [SerializeField] Transform boatCentre;
    float steerSpeed = 30;

    [SerializeField] CameraController camController;

    public Vector3 movement;
    float maxSpeed = 5;
    float acceleration = 2;

    public GameObject steeringWheel;

    CanvasManager canvasManager;

    [SerializeField] CombatController combatController;
    Rigidbody rb;

    TutorialGuy tutorial;
    
    PlayerResources resources;

    bool anchorDown = true;

    [SerializeField] ResourceSO woodSO;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        canvasManager = FindObjectOfType<CanvasManager>();
        tutorial = FindObjectOfType<TutorialGuy>();
        resources = FindObjectOfType<PlayerResources>();
        canvasManager.UpdateAnchor(true);

        targetDirectionIndicator.transform.Rotate(new Vector3(0, 0, 180));
    }

    // Update is called once per frame
    void Update()
    {
        if (combatController.started)
        {
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            return;
        }
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;




        Vector3 COM = GetComponent<Rigidbody>().centerOfMass + transform.position;
        boatIndicators.transform.position = new Vector3(COM.x, transform.position.y + 1.5f, COM.z);
        boatCentre.position = COM;

        if (camController.state == CameraController.CameraState.boatOverview) { 
            if (Input.GetKey(KeyCode.A))
            {
                targetDirectionIndicator.transform.Rotate(new Vector3(0, 0, steerSpeed * Time.deltaTime));
                steeringWheel.transform.Rotate(0, 0, 120 * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D))
            {
                targetDirectionIndicator.transform.Rotate(new Vector3(0, 0, -steerSpeed * Time.deltaTime));
                steeringWheel.transform.Rotate(0, 0, -120 * Time.deltaTime);
            }
        }

        targetDirection = (frontPoint.position - transform.position);
        targetDirection.y = 0;
        targetDirection.Normalize();

        movement += targetDirection * acceleration;
        movement = Vector3.ClampMagnitude(movement, maxSpeed);

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            ToggleAnchor();
        }

        if (!anchorDown)
        {
            rb.velocity = movement;
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
        
        //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 3 * Time.deltaTime, 0));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("hit");
        
        if (collision.gameObject.GetComponent<NPCBoat>() && !combatController.started)
        {
            tutorial.Complete(2);
            tutorial.Complete(3);
            rb.velocity = new Vector3(0, 0, 0);
            List<Grid> grids = new List<Grid>();
            grids.Add(collision.gameObject.GetComponent<NPCBoat>().ownGrid.grid);
            combatController.SetUpCombat(grids, collision.gameObject.GetComponent<NPCBoat>());
        }        

        if(collision.gameObject.tag == "Wood")
        {
            tutorial.Complete(1);
            resources.ChangeResourceAmount(woodSO, 5);
            Destroy(collision.gameObject);
        }
    }

    void ToggleAnchor()
    {
        anchorDown = !anchorDown;

        canvasManager.UpdateAnchor(anchorDown);
    }
}
