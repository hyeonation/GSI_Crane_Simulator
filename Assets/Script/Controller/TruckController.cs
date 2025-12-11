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
    String truckName;
    string job;  // 작업 종류
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
        if (targetPosition != Vector3.zero)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;

            // 시속 30km/h
            float speed = 8.33f;    
            transform.position += direction * speed * Time.fixedDeltaTime;

            // 목표 위치에 도달했는지 확인
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition; // 정확한 위치로 설정
                targetPosition = Vector3.zero; // 이동 종료
                isArrived = true;

                // 트럭 도착 시 UI_TruckControlPopup 업데이트
                GM.UI_UpdateTruckList();
            }
        }
    }

}
