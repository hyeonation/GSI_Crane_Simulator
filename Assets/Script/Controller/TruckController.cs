using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class TruckController : BaseController
{
    int row;
    int bay;
    string sRow;
    Vector3 targetPosition;
    String truckName;

    public void SetInfo(int row, int bay, string sRow, Vector3 targetPosition)
    {
        this.row = row;
        this.bay = bay;
        this.sRow = sRow;
        this.targetPosition = targetPosition;
        truckName = $"{this.sRow}{this.bay}";
    }

    void FixedUpdate()
    {
        if (targetPosition != Vector3.zero)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            float speed = 5f; // 이동 속도
            transform.position += direction * speed * Time.fixedDeltaTime;

            // 목표 위치에 도달했는지 확인
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition; // 정확한 위치로 설정
                targetPosition = Vector3.zero; // 이동 종료
            }
        }
    }

}
