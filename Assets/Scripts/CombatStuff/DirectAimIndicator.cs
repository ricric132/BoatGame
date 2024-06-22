using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectAimIndicator : MonoBehaviour
{
    [SerializeField] GameObject box;
    [SerializeField] Transform centre;
    [SerializeField] LineRenderer line;

    public void SetLocation(Vector3 startLocation, Vector3 endLocation, HitSpot targetSpot)
    {
        box.transform.position = endLocation;
        line.transform.position = Vector3.zero;
        line.SetPosition(0, startLocation);
        line.SetPosition(1, targetSpot.transform.position);
    }

    public void Toggle(bool on)
    {
        if (on)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
