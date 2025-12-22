using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Container : MonoBehaviour
{
    //containers
    public GameObject[] prefabs;
    public GameObject[] Testprefabs;


    float x_interval, y_interval, z_interval;
    int num_containers;

    int row_max, bay_max, tier_max;
    int num_containers_max, stack_profile_idx_max;

    void Awake() => init();

    void init()
    {

        // 테스트를 위해서 랜덤 시드 고정
        Random.InitState(42);

        if (GM.CraneType == Define.CraneType.RTGC)
        {
            GM.stackProfile.lengthRow = 5;
            GM.stackProfile.lengthBay = 16;
            GM.stackProfile.lengthTier = 6;
        }

        else if (GM.CraneType == Define.CraneType.RMGC)
        {

            GM.stackProfile.lengthRow = 9;
            GM.stackProfile.lengthBay = 16;
            GM.stackProfile.lengthTier = 6;
        }

        else if (GM.CraneType == Define.CraneType.QC)
        {
            GM.stackProfile.lengthRow = 10;
            GM.stackProfile.lengthBay = 12;
            GM.stackProfile.lengthTier = 6;
        }

        x_interval = GM.yard_x_interval;
        // TODO : yard_y_interval_Test => Test 용도, 나중에 변환 필요
        y_interval = GM.yard_y_interval_Test;
        z_interval = GM.yard_z_interval;
        num_containers = GM.settingParams.yardContainerNumberEA;

        // load data
        row_max = GM.stackProfile.lengthRow;
        bay_max = GM.stackProfile.lengthBay;
        tier_max = GM.stackProfile.lengthTier;

        // 컨테이너 최대 개수 계산
        num_containers_max = row_max * bay_max * tier_max;

        // stack profile 생성
        stack_profile_idx_max = row_max * bay_max;
        GM.stackProfile.listPos = new List<int[]>();

        // stack profile
        GM.stackProfile.arrTier = new int[GM.stackProfile.lengthRow + 2, GM.stackProfile.lengthBay];  // WS, LS row 추가

        // 컨테이너 생성
        mkContainers();
    }

    // Container 만드는 방식 변경
    // 초기 시작에서 Container 공중에 뜨는 현상 없애기 위함
    void mkContainers()
    {
        if (num_containers > (short)num_containers_max)
            num_containers = (short)num_containers_max;


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
            tier = GM.stackProfile.arrTier[i_row, i_bay];

            // tier 최대 아니면 추가
            if (tier < tier_max)
            {
                // list_stack_profile
                i_tier = tier;
                int[] sp = { i_row, i_bay, i_tier };
                GM.stackProfile.listPos.Add(sp);

                // 컨테이너 개수 추가
                GM.stackProfile.arrTier[i_row, i_bay] = ++tier;
                num_containers_tmp++;
            }

            // 최댓값인 idx 삭제
            else
            {
                list_sp_idx.Remove(idx);
            }
        }


        // shuffle containers
        int[] tmp;
        int random_val;
        for (int i = 0; i < num_containers; i++)
        {
            tmp = GM.stackProfile.listPos[i];
            random_val = Random.Range(0, num_containers);
            GM.stackProfile.listPos[i] = GM.stackProfile.listPos[random_val];
            GM.stackProfile.listPos[random_val] = tmp;
        }

        // make containers
        for (int i = 0; i < num_containers; i++)
        {
            placementContainer(GM.stackProfile.listPos[i]);
        }
        Debug.Log($"Container 생성 완료: {num_containers}개");
        Debug.Log($"Container 전체 개수: {Managers.Object.GetGroup<ContainerController>().Count}개");
    }

    void placementContainer(int[] idx_pos)
    {
        // make GameObject
        // TODO : Testprefabs => Test 용도, 나중에 변환 필요
        GameObject newObject = mkRandomPrefab(Testprefabs, Vector3.zero);
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
        // newObject.transform.position = transform.position;      // 절대 좌표 이동
        newObject.transform.localPosition = spawnPosition;      // 상대 좌표 이동
        newObject.transform.rotation = transform.rotation;
    }

    public static GameObject mkRandomPrefab(GameObject[] prefabs, Vector3 position)
    {
        // make GameObject
        GameObject randomPrefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
        GameObject newObject = Instantiate(randomPrefab, position, Quaternion.identity);

        // Attaching a container ID
        byte[] containerIDByteArr = mkContainerID();                // 랜덤 ID 생성
        GM.stackProfile.listID.Add(containerIDByteArr);                    // ID 저장
        newObject.name = GM.ByteArrayToString(containerIDByteArr);     // 이름 설정
        newObject.GetComponent<ContainerController>().strContainerID = GM.ByteArrayToString(containerIDByteArr); // ContainerInfo에 ID 저장

        GM.stackProfile.listContainerGO.Add(newObject);                             // GameObject 리스트에 추가

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