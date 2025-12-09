using UnityEngine;

public class SceneManagerEx 
{
    public BaseScene CurrentScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }


    public void LoadScene(Define.SceneType sceneType)
    {
        string sceneName = sceneType.ToString() + "Scene";

        // 로딩으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void Clear()
    {

    }
}
