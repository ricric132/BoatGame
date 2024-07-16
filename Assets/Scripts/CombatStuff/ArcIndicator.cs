using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class ArcIndicator : MonoBehaviour
{
    [SerializeField] float totalVelocity;
    [SerializeField] float xVelocity;
    [SerializeField] float yVelocity;
    [SerializeField] float angle;
    [SerializeField] float idealAngle;
    [SerializeField] float stepSize;
    [SerializeField] float maxVelocity;

    [SerializeField] Transform source;
    [SerializeField] Transform target;
    [SerializeField] LineRenderer lineRenderer;

    [SerializeField] Material invalidColor;
    [SerializeField] Material validColor;

    bool validThrow;

    float horizontalDist;
    float verticalDist;

    Vector3 prevTargetPos = Vector3.zero;

    [SerializeField] GameObject sphere;


    void Update()
    {
        if(target.position == prevTargetPos)
        {
            return;
        }
        prevTargetPos = target.position;

        validThrow = CalculateThrow();

        lineRenderer.positionCount = Mathf.CeilToInt(horizontalDist / (xVelocity * stepSize));
        int counter = 0;
        Vector3 dir = (new Vector3(target.position.x, 0, target.position.z) - new Vector3(source.position.x, 0, source.position.z)).normalized;
        float dist = Vector3.Distance(new Vector3(target.position.x, 0, target.position.z), new Vector3(source.position.x, 0, source.position.z));
        float yPos = 0;

        while (counter < lineRenderer.positionCount)
        {
            float x = counter * stepSize * xVelocity;
            Vector3 horizontal = dir * x;
            yPos += yVelocity * stepSize;
            yVelocity -= 10 * stepSize;

            lineRenderer.SetPosition(counter, new Vector3(horizontal.x, yPos, horizontal.z) + transform.position);
            counter++;
        }


        if (validThrow)
        {
            lineRenderer.material = validColor;
        }
        else
        {
            lineRenderer.material = invalidColor;
        }
    }

    bool CalculateThrow()
    {
        horizontalDist = Vector3.Distance(new Vector3(target.position.x, 0, target.position.z), new Vector3(source.position.x, 0, source.position.z)); //horizontal displacement
        verticalDist = source.position.y - target.position.y; // vertical displacement;

        float chosenAngle = 0;
        float tooFarAngle = 0;
        bool foundPossible = false; 

        float angle = idealAngle;

        while(angle >= 0)
        {
            float aRad = Mathf.Deg2Rad * angle;
            totalVelocity = AngleToVelocity(aRad);

            if (totalVelocity != -1)
            {
                if (totalVelocity <= maxVelocity)
                {
                    if(CheckCollision(aRad) == false)
                    {
                        chosenAngle = angle;
                        foundPossible = true;
                        break;
                    }
                }
                else if(Mathf.Abs(idealAngle - angle) < Mathf.Abs(idealAngle - tooFarAngle))
                {
                    tooFarAngle = angle;
                }

            }

            angle -= 5;
        }

        angle = idealAngle;

        while (angle < 90)
        {
            float aRad = Mathf.Deg2Rad * angle;
            totalVelocity = AngleToVelocity(aRad);

            if (totalVelocity != -1)
            {
                if ((Mathf.Abs(idealAngle - angle) < Mathf.Abs(idealAngle - chosenAngle) || foundPossible == false) && totalVelocity <= maxVelocity )
                {
                    if(CheckCollision(aRad) == false)
                    {
                        chosenAngle = angle;
                        foundPossible = true;
                        break;
                    }
                }
                else if (Mathf.Abs(idealAngle - angle) < Mathf.Abs(idealAngle - tooFarAngle))
                {
                    tooFarAngle = angle;
                }

            }

            angle += 5;
        }


        if (foundPossible == false)
        {
            //Debug.Log("impossible");
            //Debug.Log("angle : " + tooFarAngle);
            float aRad = Mathf.Deg2Rad * tooFarAngle;
            totalVelocity = AngleToVelocity(aRad);
            xVelocity = totalVelocity * Mathf.Cos(aRad);
            yVelocity = totalVelocity * Mathf.Sin(aRad);
            return false;
        }
        else
        {
            //Debug.Log("angle : " + chosenAngle);
            float aRad = Mathf.Deg2Rad * chosenAngle;
            totalVelocity = AngleToVelocity(aRad);
            xVelocity = totalVelocity * Mathf.Cos(aRad);
            yVelocity = totalVelocity * Mathf.Sin(aRad);

            //Debug.Log("velocity : " + totalVelocity);
            //Debug.Log("x : " + xVelocity);
            //Debug.Log("y : " + yVelocity);

            return true;
        }
    }
    
    float AngleToVelocity(float angle)
    {
        if((Mathf.Sin(angle) * Mathf.Cos(angle) + (verticalDist * Mathf.Pow(Mathf.Cos(angle), 2)) / horizontalDist) <= 0)
        {
            return -1;
        }
        return Mathf.Sqrt((5 * horizontalDist) / (Mathf.Sin(angle) * Mathf.Cos(angle) + (verticalDist * Mathf.Pow(Mathf.Cos(angle), 2)) / horizontalDist));
    }

    bool CheckCollision(float testRad)
    {
        float tempV = AngleToVelocity(testRad);
        float tempX = totalVelocity * Mathf.Cos(testRad);
        float tempY = totalVelocity * Mathf.Sin(testRad);

        int layermask = LayerMask.GetMask("OccupiedTile");
        int points = Mathf.CeilToInt(horizontalDist / (tempX * stepSize));
        int counter = 0;

        Vector3 dir = (new Vector3(target.position.x, 0, target.position.z) - new Vector3(source.position.x, 0, source.position.z)).normalized;
        float yPos = 0;

        Vector3 newPos = transform.position;
        Vector3 prevPos = transform.position;
        

        while (counter < points)
        {
            float x = counter * stepSize * tempX;
            Vector3 horizontal = dir * x;
            yPos += tempY * stepSize;
            tempY -= 10 * stepSize;

            newPos = new Vector3(horizontal.x, yPos, horizontal.z) + transform.position;

            Debug.Log(newPos);
            if (Physics.Linecast(prevPos, newPos, layermask))
            {
                return true;
            }
            counter++;
            prevPos = newPos;
        }

        return false;
    }

    public void SetUp(Vector3 newSource, Vector3 newTarget, int strength, float weight, float aoe = 0)
    {
        target.position = newTarget;
        source.position = newSource;
        transform.position = newSource;

        
        maxVelocity = (strength*2)/weight;

        if (aoe != 0)
        {
            sphere.SetActive(true);
            sphere.transform.localScale = new Vector3(aoe, aoe, aoe);
        }
        else
        {
            sphere.SetActive(false);
        }
    }

    public void Toggle(bool state)
    {
        gameObject.SetActive(state);
    }

    public Vector3[] GetArcPoints()
    {
        Vector3[] points = new Vector3[lineRenderer.positionCount];

        lineRenderer.GetPositions(points);

        return points;

    }
}
