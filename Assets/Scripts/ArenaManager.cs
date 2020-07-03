using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaManager : Singleton<ArenaManager>
{

    Dictionary<string, AssetBundle> loadedBundleMap = new Dictionary<string, AssetBundle>();

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator LoadSceneFromBundle(string path, LoadSceneMode mode)
    {

        AssetBundle loadedAssetBundle;

        if (loadedBundleMap.ContainsKey(path))
        {
            loadedAssetBundle = loadedBundleMap[path];
        }
        else
        {
            loadedAssetBundle = AssetBundle.LoadFromFile(path);
            loadedBundleMap.Add(path, loadedAssetBundle);
        }           

        if (loadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            yield return null;
        }
        else
        {
            if (loadedAssetBundle.isStreamedSceneAssetBundle)
            {
                string[] scenePaths = loadedAssetBundle.GetAllScenePaths();
                string sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);
                AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneName, mode);


                while (!sceneLoad.isDone)
                {

                    yield return null;
                }
                loadedAssetBundle.Unload(true);
            }
        }
    }
}
