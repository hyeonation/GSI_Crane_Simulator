using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : BaseController
{

    public Camera cam;
    public string camName;
    public Transform target;
    public Vector3 offset;

    public int viewportIdx = -1;
    public int targetDisplayIdx = -1;

    void Start()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        camName = gameObject.name;
        targetDisplayIdx = cam.targetDisplay;
        if (viewportIdx == -1)
        {
            for (int i = 0; i < Define.screenRects.Length; i++)
            {
                if (cam.rect == Define.screenRects[i])
                {
                    viewportIdx = i;
                    break;
                }
            }
        }
    }


    public void CameraOff()
    {
        gameObject.SetActive(false);
    }

    public void CameraOn()
    {
        gameObject.SetActive(true);
    }

    public void SetScreenRect(int viewport)
    {
        cam.rect = Define.screenRects[viewport];
        viewportIdx = viewport;
    }
    public void SetTargetDisplay(int displayIdx)
    {
        cam.targetDisplay = displayIdx;
    }
    public void SetDepth(int depth)
    {
        cam.depth = depth;
    }

    //camera 
    public void SetCameraPan(bool panLeft, bool panRight)
    {
        // 입력이 없으면 연산하지 않음 (최적화)
        if (!panLeft && !panRight) return;

        // 방향 결정 (-1: 왼쪽, 1: 오른쪽)
        float direction = 0f;
        if (panLeft) direction = -1f;
        if (panRight) direction = 1f;

        if (direction == 0f) return;

        // 핵심 구현: Vector3.up(Y축)을 기준으로 회전
        cam.transform.Rotate(Vector3.right, direction * Define.CameraSpeed * Time.deltaTime, Space.World);
    }

    public void SetCameraTilt(bool tiltUp, bool tiltDown)
    {
        // 입력이 없으면 연산하지 않음 (최적화)
        if (!tiltUp && !tiltDown) return;

        // 방향 결정 (-1: 왼쪽, 1: 오른쪽)
        float direction = 0f;
        if (tiltUp) direction = -1f;
        if (tiltDown) direction = 1f;

        if (direction == 0f) return;

        // 핵심 구현: Vector3.up(Y축)을 기준으로 회전
        cam.transform.Rotate(Vector3.back, direction * Define.CameraSpeed * Time.deltaTime, Space.World);
    }

    public void SetCameraCW(bool cw, bool ccw)
    {
        // 입력이 없으면 연산하지 않음 (최적화)
        if (!cw && !ccw) return;

        // 방향 결정 (-1: 왼쪽, 1: 오른쪽)
        float direction = 0f;
        if (cw) direction = -1f;
        if (ccw) direction = 1f;

        if (direction == 0f) return;

        // 핵심 구현: Vector3.up(Y축)을 기준으로 회전
        cam.transform.Rotate(Vector3.up, direction * Define.CameraSpeed * Time.deltaTime, Space.World);
    }
    public void SetCamerZoom(bool zoomIn, bool zoomOut)
    {
        if (!zoomIn && !zoomOut) return;

        float direction = 0f;
        if (zoomIn) direction = -1f;
        if (zoomOut) direction = 1f;

        if (direction == 0f) return;

        float newFOV = cam.fieldOfView + (direction * Define.CameraSpeed * Time.deltaTime);
        cam.fieldOfView = Mathf.Clamp(newFOV, 1f, 180f);
    }



}
