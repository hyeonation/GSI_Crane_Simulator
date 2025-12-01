using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    //containers
    public GameObject[] prefabs;

    float x_interval, y_interval, z_interval;
    int num_containers;

    int row_max, bay_max, tier_max;
    int num_containers_max, stack_profile_idx_max;

    void Awake() => init();

    void init()
    {
        x_interval = GM.yard_x_interval;
        y_interval = GM.yard_y_interval;
        z_interval = GM.yard_z_interval;
        num_containers = GM.settingParams.yardContainerNumberEA;

        // load data
        row_max = GM.lengthRow;
        bay_max = GM.lengthBay;
        tier_max = GM.lengthTier;

        // 컨테이너 최대 개수 계산
        num_containers_max = row_max * bay_max * tier_max;

        // stack profile 생성
        stack_profile_idx_max = row_max * bay_max;
        GM.list_stack_profile = new List<int[]>();

        // stack profile
        GM.stack_profile = new int[GM.lengthRow + 2, GM.lengthBay];  // WS, LS row 추가

        // 컨테이너 생성
        mkContainers();
    }

    // Container 만드는 방식 변경
    // 초기 시작에서 Container 공중에 뜨는 현상 없애기 위함
    void mkContainers()
    {
        //// 컨테이너 입력 개수가 Yard 최대 개수 초과했는지 구분

        // container 개수 범위 안일 때
        if (num_containers < num_containers_max)
        {
            // stack_profile idx list 생성
            List<int> list_sp_idx = new List<int>();
            for (int i = 0; i < stack_profile_idx_max; i++)
            {
                list_sp_idx.Add(i);
            }

            // stack_profile 만들기
            int idx, i_row, i_bay, i_tier, tier, random_i;

            // container 개수 입력값에 도달할 때까지
            // stack_profile 만들기
            int num_containers_tmp = 0;
            while (num_containers_tmp < num_containers)
            {
                // random idx 추출
                // Debug.Log(list_sp_idx.Count);
                random_i = Random.Range(0, list_sp_idx.Count);
                idx = list_sp_idx[random_i];

                // random row, bay 추출
                i_row = idx % row_max;
                i_bay = idx / row_max;
                tier = GM.stack_profile[i_row, i_bay];

                // tier 최대 아니면 추가
                if (tier < tier_max)
                {
                    // list_stack_profile
                    i_tier = tier;
                    int[] sp = { i_row, i_bay, i_tier };     // 마지막 0은 container state. None 의미.
                    GM.list_stack_profile.Add(sp);

                    // 컨테이너 개수 추가
                    GM.stack_profile[i_row, i_bay] = ++tier;
                    num_containers_tmp++;
                }

                // 최댓값인 idx 삭제
                else
                {
                    list_sp_idx.Remove(idx);
                }
            }
        }

        // container 개수 범위 초과할 때
        else
        {
            // 최대 개수로 초기화
            num_containers = (short)num_containers_max;

            // stack_profile, list_stack_profile
            for (int i_row = 0; i_row < row_max; i_row++)
            {
                for (int i_bay = 0; i_bay < bay_max; i_bay++)
                {
                    GM.stack_profile[i_row, i_bay] = tier_max;

                    for (int i_tier = 1; i_tier <= tier_max; i_tier++)
                    {
                        int[] sp = { i_row, i_bay, i_tier };
                        GM.list_stack_profile.Add(sp);
                    }
                }
            }
        }

        // shuffle containers
        int[] tmp;
        int random_val;
        for (int i = 0; i < num_containers; i++)
        {
            tmp = GM.list_stack_profile[i];
            random_val = Random.Range(0, num_containers);
            GM.list_stack_profile[i] = GM.list_stack_profile[random_val];
            GM.list_stack_profile[random_val] = tmp;
        }

        // make containers
        for (int i = 0; i < num_containers; i++)
        {
            placementContainer(GM.list_stack_profile[i]);
        }
    }

    void placementContainer(int[] idx_pos)
    {
        // make GameObject
        GameObject newObject = mkRandomPrefab(prefabs);
        newObject.transform.SetParent(transform);

        // stack position data
        int i_row = idx_pos[0];
        int i_bay = idx_pos[1];
        int i_tier = idx_pos[2];

        // 위치 배치
        Vector3 spawnPosition = new Vector3(i_row * x_interval,
                                            i_tier * y_interval,
                                            i_bay * z_interval);

        // 부모인 Containers에 transform 맞춤
        newObject.transform.position = transform.position;      // 절대 좌표 이동
        newObject.transform.localPosition = spawnPosition;      // 상대 좌표 이동
        newObject.transform.rotation = transform.rotation;
    }

    public static GameObject mkRandomPrefab(GameObject[] prefabs)
    {
        // make GameObject
        GameObject randomPrefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
        GameObject newObject = Instantiate(randomPrefab, Vector3.zero, Quaternion.identity);

        // Attaching a container ID
        byte[] containerIDByteArr = mkContainerID();                // 랜덤 ID 생성
        GM.listContainerID.Add(containerIDByteArr);                    // ID 저장
        newObject.name = GM.ByteArrayToString(containerIDByteArr);     // 이름 설정

        return newObject;
    }

    // make container name
    public static byte[] mkContainerID()
    {
        byte[] result = new byte[11];

        // 앞 4자리: 대문자 알파벳 (A=65 ~ Z=90)
        for (int i = 0; i < 4; i++)
        {
            result[i] = (byte)UnityEngine.Random.Range(65, 91);
        }

        // 뒤 7자리: 숫자 (0=48 ~ 9=57)
        for (int i = 4; i < 11; i++)
        {
            result[i] = (byte)UnityEngine.Random.Range(48, 58);
        }

        return result;
    }
}