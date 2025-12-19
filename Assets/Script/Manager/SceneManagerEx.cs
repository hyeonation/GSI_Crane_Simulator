using UnityEngine;

public class SceneManagerEx 
{
    public Define.SceneType CurrentSceneType
    {
        get; private set;
    }


    public void LoadScene(Define.SceneType sceneType)
    {
        string sceneName = sceneType.ToString();
        CurrentSceneType = sceneType;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

    }

    public void Clear()
    {

    }
}
