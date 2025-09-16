using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    //containers
    public GameObject[] prefabs;

    float start_val, x_interval, y_interval, z_interval;
    int num_containers;
    string name_container = "Container";

    int row_max, bay_max, tier_max;
    int num_containers_max, stack_profile_idx_max;
    public static List<List<int>> list_stack_profile;

    void Start()
    {

        // init values
        init();

        // 컨테이너 생성
        mkContainers();
    }

    void init()
    {
        start_val = GM.yard_start_val;
        x_interval = GM.yard_x_interval;
        y_interval = GM.yard_y_interval;
        z_interval = GM.yard_z_interval;
        num_containers = GM.settingParams.yardContainerNumberEA;

        // load data
        row_max = GM.row;
        bay_max = GM.bay;
        tier_max = GM.tier;

        // 컨테이너 최대 개수 계산
        num_containers_max = row_max * bay_max * tier_max;

        // stack profile 생성
        stack_profile_idx_max = row_max * bay_max;
        list_stack_profile = new List<List<int>>();
    }

    // IEnumerator RotationUncheck()
    // {

    //     yield return new WaitForSeconds(20f);
    //     for (int i = 0; i < num_containers; i++)
    //     {
    //         GameObject Containers = GameObject.Find($"{name_container}{i}");
    //         Rigidbody rb = Containers.GetComponent<Rigidbody>();
    //         rb.constraints = RigidbodyConstraints.FreezeAll;
    //     }
    // }

    // IEnumerator kinematicCheck()
    // {

    //     yield return new WaitForSeconds(11f);
    //     for (int i = 0; i < num_containers; i++)
    //     {
    //         GameObject Containers = GameObject.Find($"{name_container}{i}");
    //         Rigidbody rb = Containers.GetComponent<Rigidbody>();
    //         rb.drag = 0.4f;
    //     }
    // }

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
                    i_tier = ++tier;
                    List<int> sp = new List<int> { i_row, i_bay, i_tier };
                    list_stack_profile.Add(sp);

                    // 컨테이너 개수 추가
                    GM.stack_profile[i_row, i_bay] = tier;
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
                        List<int> sp = new List<int> { i_row, i_bay, i_tier };
                        list_stack_profile.Add(sp);
                    }
                }
            }
        }

        // shuffle containers
        List<int> tmp;
        int random_val;
        for (int i = 0; i < num_containers; i++)
        {
            tmp = list_stack_profile[i];
            random_val = Random.Range(0, num_containers);
            list_stack_profile[i] = list_stack_profile[random_val];
            list_stack_profile[random_val] = tmp;
        }

        // make containers
        for (int i = 0; i < num_containers; i++)
        {
            mkContainer(i, list_stack_profile[i]);
        }
    }

    void mkContainer(int num, List<int> idx_pos)
    {
        int i_row = idx_pos[0];
        int i_bay = idx_pos[1];
        int i_tier = idx_pos[2];

        GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];
        GameObject newObject = Instantiate(randomPrefab, Vector3.zero, Quaternion.identity);

        newObject.name = $"{name_container}{num}"; //이름 설정
        newObject.transform.SetParent(transform);

        // 위치 배치
        Vector3 spawnPosition = new Vector3((i_row * x_interval) + start_val,
                                            (i_tier * y_interval) - y_interval / 2 + 0.1f,
                                            (i_bay * z_interval) + 7.75f);

        // 부모인 Containers에 맞춤
        newObject.transform.position = transform.position;
        newObject.transform.localPosition = spawnPosition;
        newObject.transform.rotation = transform.rotation;
    }
}