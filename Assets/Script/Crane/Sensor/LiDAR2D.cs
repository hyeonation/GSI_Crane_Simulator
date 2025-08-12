using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiDAR2D : MonoBehaviour
{

    // outputs
    [HideInInspector] public float[] arrDistance;  // array to store distances for each angle
    [HideInInspector] public bool boolHit;

    // parameters
    [HideInInspector] public float maxDistance = 50f;  // maximum distance for LiDAR detection
    [HideInInspector] public float resolution = 0.1f;  // resolution
    [HideInInspector] public int max_angle = 90;  // maximum angle for LiDAR sweep
    [HideInInspector] public int min_angle = -90;  // minimum angle for LiDAR sweep


    // Width of the laser ray for visualization
    private float laserWidthDrawing = 0.01f;


    // Start is called before the first frame update
    void Start()
    {
        // initialize variables
        int arrLength = (int)((max_angle - min_angle) / resolution);
        arrDistance = new float[arrLength];

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            GetLaserDistance(maxDistance);
        }
    }

    void GetLaserDistance(float maxDistance)
    {
        RaycastHit hit;
        float theta;
        Vector3 dir;
        Color color;

        float distance;
        
        // determine standard vector
        Vector3 point_src = transform.position;
        Vector3 dir_std = transform.forward;
        Vector3 dir_rotate = transform.up;

        for (int count = 0; count < arrDistance.Length; count++)
        {
            // rotate angle
            // rotate to CCW
            theta = -90 + (count * resolution);

            // rotate laser direction
            dir = Quaternion.AngleAxis(theta, dir_rotate) * dir_std;

            // emit laser
            boolHit = Physics.Raycast(point_src, dir, out hit, maxDistance);

            // If an object is hit, set the distance to the hit distance
            if (boolHit)
            {
                color = Color.blue;
                distance = hit.distance;
            }

            // If no object is hit, set the distance to the maximum distance
            else
            {
                color = Color.red;
                distance = maxDistance;
            }

            // Draw the laser ray in the scene view for debugging
            Debug.DrawRay(point_src, dir * distance, color, laserWidthDrawing);

            // Save distance
            arrDistance[count] = distance;
        }
    }
}
