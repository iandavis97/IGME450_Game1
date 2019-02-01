using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    public Vector2[] pointList;
    public float speed = 3.0f;
    public bool drawDebug;


    //get the parent object for lerping
    GameObject platform;
    int currentPoint;
    int destinationPoint;
    int numPoints;
    float startTime;
    float distance;

	// Use this for initialization
	void Awake () {
        platform = gameObject;
        currentPoint = 0;
        destinationPoint = currentPoint + 1;
        startTime = Time.time;
        distance = Vector3.Distance(pointList[currentPoint], pointList[currentPoint + 1]);

        platform.transform.position = pointList[currentPoint];
        numPoints = pointList.Length;

        if (!gameObject.GetComponent<Rigidbody>())
            Debug.Log("true");
	}


    void Update()
    {

        //start the timer
        float distCovered = (Time.time - startTime) * speed;
        float pctCompleted = distCovered / distance;

        platform.transform.position = Vector3.Lerp(pointList[currentPoint], pointList[destinationPoint], pctCompleted);

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

    private void OnDrawGizmos()
    {
        if (drawDebug)
        {
            Gizmos.color = Color.red;
            foreach (Vector2 point in pointList)
            {
                Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), 0.3f);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.tag == "Player")
        {
            other.transform.parent = transform;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.transform.tag == "Player")
        {
            other.transform.parent = null;
        }
    }
}
