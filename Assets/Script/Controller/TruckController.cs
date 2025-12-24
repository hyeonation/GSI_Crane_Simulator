using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;

public class TruckController : BaseController
{
    int row;
    int bay;
    string sRow;
    Vector3 targetPosition;
    public String truckName;
    public int Job;  // 작업 종류
    float truckSpeed = 10f;
    public bool isSelected { get; set; } = false;
    public Define.TruckStatus truckStatus;
    private List<Landed> landedSensors;
    string craneName; // 할당된 크레인 이름
    public string CraneName
    {
        get { return craneName; }
        set { craneName = value; }
    }
    // 트럭 도착 여부
    bool isArrived = false;
    public bool IsArrived
    {
        get { return isArrived; }
        set { isArrived = value; }
    }
    [SerializeField]
    private bool _isLanded;

    public bool IsLanded
    {
        // Sensor들 전부 true 이면 true
        get
        {
            foreach (var sensor in landedSensors)
            {
                if (!sensor.landed_sensor)
                {
                    _isLanded = false;
                    return _isLanded;
                }
            }
            _isLanded = true;
            return _isLanded;
        }
    }

    public ContainerController TargetContainer
    {
        get
        {
            // 센서를 돌면서 컨테이너가 있는 센서를 찾음
            foreach (var sensor in landedSensors)
            {
                if (sensor.containerController != null)
                {
                    return sensor.containerController;
                }
            }
            return null;
        }
    }
    private Rigidbody _rbTruck;
    public override bool Init()
    {
        if (!base.Init())
            return false;

        landedSensors = new List<Landed>(GetComponentsInChildren<Landed>());
        _rbTruck = transform.GetComponent<Rigidbody>();

        return true;
    }

    public void SetInfo(int row, int bay, string sRow, Vector3 targetPosition)
    {
        this.row = row;
        this.bay = bay;
        this.sRow = sRow;
        this.targetPosition = targetPosition;
        truckName = $"{this.sRow}{this.bay}";
    }

    public void FinishJob()
    {
        // TODO 임시 
        Managers.Resource.Destroy(this.gameObject);
    }

    void FixedUpdate()
    {

        // 목표 위치로 이동중일때 작동 
        if (targetPosition != Vector3.zero && !isArrived)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;


            transform.position += direction * truckSpeed * Time.fixedDeltaTime;

            // 목표 위치에 도달했는지 확인
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition; // 정확한 위치로 설정
                targetPosition = Vector3.zero; // 이동 종료
                isArrived = true;

                // 트럭 도착 시 UI_TruckControlPopup 업데이트
                GM.UpdateTruckList();
            }
        }
        if (IsLanded)
        {

        }

        // truck 미세조정 모드일때 선택된 트럭일경우. UI_TruckControlPopup에서 선택된 트럭을 미세조정
        if (isSelected)
        {
            transform.position += Vector3.forward * GM.cmdTruckVel * Time.fixedDeltaTime;
        }



    }


}
