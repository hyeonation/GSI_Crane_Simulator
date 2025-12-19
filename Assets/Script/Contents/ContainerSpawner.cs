using UnityEngine;

public class ContainerSpawner : MonoBehaviour
{
    
    
    
    private Transform containerRoot;

    [Header("Grid Dimensions (Unit Count)")]
    [SerializeField] private int rowCount = 5;   // X축: 열 개수
    [SerializeField] private int tierCount = 3;  // Y축: 단(높이) 개수
    [SerializeField] private int bayCount = 10;  // Z축: 오행(깊이) 개수
    [SerializeField] private int spawnContainerCount = 100;  // 생성할 컨테이너 총 개수

    [SerializeField] private bool isLandSide = true;
    [SerializeField] private bool isWaterSide = true;

    [Header("Spacing Settings")]
    [Tooltip("각 컨테이너 간의 간격(Center to Center)입니다 (row,tier,bay).")]
    [SerializeField] private Vector3 spacing = new Vector3(3.0f, 2.5f, 6.0f);

    // 런타임에 생성된 컨테이너를 관리하기 위한 3차원 배열
    private ContainerController[,,] spawnedContainers;

    private void Awake()
    {
        // containerRoot가 할당되지 않았다면 현재 오브젝트를 부모로 설정
        if (containerRoot == null) containerRoot = this.transform;
    }

    public void SetSpawnerParameters(int rows, int tiers, int bays, Vector3 gap , int spawnCount, bool landSide = true, bool waterSide = true)
    {
        rowCount = rows;
        tierCount = tiers;
        bayCount = bays;
        spacing = gap;

        if (spawnCount > rows * tiers * bays)
            spawnContainerCount = rows * tiers * bays;
        else
            spawnContainerCount = spawnCount;

        isLandSide = landSide;
        isWaterSide = waterSide;

        // 3차원 배열 초기화 ls, ws 있을경우 추가
    }

   

    private void GenerateContainer(int row, int tier, int bay)
    {
        // 로컬 위치 계산: 인덱스 * 간격
        Vector3 localPos = new Vector3(row * spacing.x, tier * spacing.y, bay * spacing.z);
        
        // 월드 위치 변환: Spawner의 위치를 기준으로 더함
        Vector3 worldPos = this.transform.position + localPos;

        // 생성
        ContainerController newContainer = Managers.Object.SpawnRandomContainer(worldPos);
        // newContainer.name = $"Container_R{r}_T{t}_B{b}";

        // // 배열에 참조 저장
        // spawnedContainers[r, t, b] = newContainer;
    }

 
    [ContextMenu("Clear Containers")]
    public void Clear()
    {
        // 모든 자식 오브젝트 파괴
        foreach (ContainerController child in containerRoot)
        {
            Managers.Object.Despawn(child);
        }
    }

}