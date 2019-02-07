using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Malcolm Lambrecht
 *  to make a moving platform attach this script to it and make sure to set at least two points to lerp between
 *  also give the platform's box collider the "Platform Mat" material
 */



public class MovingPlatform : MonoBehaviour {

    public Vector2[] pointList;
    public float speed = 3.0f;
    public bool drawDebug;

    //get the parent object for lerping
    int currentPoint;
    int destinationPoint;
    int numPoints;
    float startTime;
    float distance;

	// Use this for initialization
	void Awake () {
        currentPoint = 0;
        destinationPoint = currentPoint + 1;

        startTime = Time.time;
        distance = Vector3.Distance(pointList[currentPoint], pointList[currentPoint + 1]);

        gameObject.transform.position = pointList[currentPoint];
        numPoints = pointList.Length;
	}


    void Update()
    {

        //start the timer
        float distCovered = (Time.time - startTime) * speed;
        float pctCompleted = distCovered / distance;

        gameObject.transform.position = Vector3.Lerp(pointList[currentPoint], pointList[destinationPoint], pctCompleted);

        if(pctCompleted >= 1.0f)
        {
            changeCurrentPoint();
        }
    }

    void changeCurrentPoint()
    {
        startTime = Time.time;
        currentPoint++;
        if(currentPoint == numPoints)
        {
            currentPoint = 0;
        }
        destinationPoint = currentPoint + 1;
        if (destinationPoint == numPoints)
        {
            destinationPoint = 0;
        }
    }

    //for visual points that the platform will lerp between, editor only
    private void OnDrawGizmos()
    {
        if (drawDebug)
        {
            Gizmos.color = Color.red;
            foreach (Vector2 point in pointList)
            {
                Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), 0.2f);
            }
        }
    }

    //parents the player to the platform, this is responsible for most of the not sliding off of them
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.tag == "Player")
        {
            other.transform.parent = transform;
        }
    }

    //unparents the player to the platform
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.transform.tag == "Player")
        {
            other.transform.parent = null;
        }
    }
}
