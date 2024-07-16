using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;


public class CameraController : MonoBehaviour
{
    [SerializeField] Vector3 defaultEulerRotation; 
    [SerializeField] float horizontalSpeed;
    [SerializeField] float verticalSpeed;
    [SerializeField] float zoomSpeed;
    [SerializeField] float turnSpeed;
    [SerializeField] Transform pivotPoint;
    [SerializeField] Transform nonRotatePivotPoint;
    [SerializeField] BuildingScript buildingScript;
    float zoom; 
    float targetZoom; 
    int storyLevel;
    float pivotHeight;

    [SerializeField] float lateralMovementSpeed; 
    [SerializeField] float verticalMovementSpeed;
    [SerializeField] Transform lateralTrolly;
    [SerializeField] UITest UITest;
    [SerializeField] Transform boatCentre;
    [SerializeField] Transform boat;


    [SerializeField] Cinemachine.CinemachineVirtualCamera freeMoveView;
    [SerializeField] Cinemachine.CinemachineVirtualCamera boatMovementView;
    public CameraState state;


    public enum CameraState
    {
        freeMove,
        boatOverview
    }



    //[SerializeField] Transform boatCentre;

    // Start is called before the first frame update
    void Start()
    {
        state = CameraState.freeMove;
        SetCameraState(CameraState.freeMove);
    }

    // Update is called once per frame
    void Update()
    {
        //pivotPoint.position = boatCentre.position;

        //Single pivot at centre camera controller
        /* 
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forwardMove = transform.forward;
        forwardMove.y = 0;
        forwardMove.Normalize();

        if(Input.GetKeyDown(KeyCode.A)){
            KeyValuePair<float, int> newHeight = buildingScript.GetHeight(storyLevel-1);
            storyLevel = newHeight.Value;
            pivotHeight = newHeight.Key;
        }
        if(Input.GetKeyDown(KeyCode.S)){
            KeyValuePair<float, int> newHeight = buildingScript.GetHeight(storyLevel+1);
            storyLevel = newHeight.Value;
            pivotHeight = newHeight.Key;
        }


        pivotPoint.localPosition = new Vector3(pivotPoint.localPosition.x, Mathf.Lerp(pivotPoint.localPosition.y, pivotHeight, 5f * Time.deltaTime), pivotPoint.localPosition.z);
        //transform.position += (forwardMove * z + transform.right * x).normalized * horizontalSpeed * Time.deltaTime;
        //transform.position += new Vector3(0, 1, 0) * Input.mouseScrollDelta.y * verticalSpeed * Time.deltaTime;

        //if(Input.GetKeyDown(KeyCode.))
        transform.LookAt(pivotPoint);

        targetZoom = Mathf.Clamp(targetZoom + Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime, 5, 1000);
        zoom = Mathf.Lerp(zoom, targetZoom, 5f * Time.deltaTime);
        Vector3 dirFromPivot = (transform.position - pivotPoint.position).normalized;
        transform.position = pivotPoint.position + (dirFromPivot * zoom);


        if(Input.GetKey(KeyCode.Mouse1)){
            float turnAmountX = Input.GetAxis("Mouse X");
            //float turnAmountY = -Input.GetAxis("Mouse Y");
            pivotPoint.rotation = Quaternion.Euler(pivotPoint.rotation.eulerAngles + new Vector3(0, turnAmountX * turnSpeed * Time.deltaTime, 0));
            //pivotPoint.rotation = Quaternion.Euler(pivotPoint.rotation.eulerAngles + new Vector3(turnAmountY * turnSpeed * Time.deltaTime, 0, 0));
        }
        */

        //lateral camera movement\

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(state == CameraState.freeMove)
            {
                SetCameraState(CameraState.boatOverview);
            }
            else if (state == CameraState.boatOverview)
            {
                SetCameraState(CameraState.freeMove);
            }

        }

        if (state == CameraState.freeMove)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            lateralTrolly.position += lateralTrolly.TransformDirection(0, 0, 1) * z * lateralMovementSpeed * Time.deltaTime;
            lateralTrolly.position += lateralTrolly.TransformDirection(1, 0, 0) * x * lateralMovementSpeed * Time.deltaTime;

            if (UITest.IsPointerOverUIElement() == false)
            {
                lateralTrolly.position += new Vector3(0, Input.mouseScrollDelta.y, 0) * verticalMovementSpeed * Time.deltaTime;

                if (Input.GetKey(KeyCode.Mouse1))
                {
                    float turnAmountX = Input.GetAxis("Mouse X");
                    lateralTrolly.rotation = Quaternion.Euler(lateralTrolly.rotation.eulerAngles + new Vector3(0, turnAmountX * turnSpeed * Time.deltaTime, 0));

                }
            }
        }
    }

    void SetCameraState(CameraState _state)
    {
        state = _state;
        freeMoveView.gameObject.SetActive(false);
        boatMovementView.gameObject.SetActive(false);

        if(state == CameraState.freeMove){freeMoveView.gameObject.SetActive(true); }
        if(state == CameraState.boatOverview){boatMovementView.gameObject.SetActive(true); }
    }
}
