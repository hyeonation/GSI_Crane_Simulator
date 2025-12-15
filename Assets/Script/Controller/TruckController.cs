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
    string job;  // 작업 종류
    float truckSpeed = 30f; 
    public bool isSelected {get;set;} = false;
    public string Job
    {
        get { return job; }
        set { job = value; }
    }
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
        Destroy(this.gameObject);
    }

    void FixedUpdate()
    {

        // 목표 위치로 이동중일때 작동 
        if (targetPosition != Vector3.zero)
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

        // truck 미세조정 모드일때 선택된 트럭일경우. UI_TruckControlPopup에서 선택된 트럭을 미세조정
        if(isSelected)
        {
            transform.position += Vector3.forward * GM.cmdTruckVel * Time.fixedDeltaTime;
        }

        
    }


}
