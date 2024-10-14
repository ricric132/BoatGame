using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatTextPopup : MonoBehaviour
{
    [SerializeField] Camera cam;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + (transform.position - cam.transform.position));
    }
}
