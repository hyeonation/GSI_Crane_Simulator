using S7.Net;
using S7.Net.Types;
using UnityEngine;

public class Laser : MonoBehaviour
{
    // parameters
    private float maxDistance = 12.0f;  // maximum distance for laser detection
    private float laserWidth = 0.01f;   // Width of the laser ray for visualization

    // Output distance
    [HideInInspector] public float distance;

    private void Update()
    {
        distance = GetLaserDistance(maxDistance);
    }

    public float GetLaserDistance(float maxDistance)
    {
        RaycastHit hit;
        float output;
        Vector3 point_src = gameObject.transform.position;
        Vector3 dir = gameObject.transform.forward;
        Color color;

        // Perform a raycast to detect objects in the direction of the laser
        if (Physics.Raycast(point_src, dir, out hit, maxDistance))
        {
            color = Color.blue;
            output = hit.distance;
        }

        // If no object is hit, set the output to the maximum distance
        else
        {
            color = Color.red;
            output = maxDistance;
        }

        // Draw the laser ray in the scene view for debugging
        Debug.DrawRay(point_src, dir * output, color, laserWidth);

        return output;
    }

}
