using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class HomeUIController : MonoBehaviour
{
    public Transform arenaListContainer;
    public GameObject ArenaOptionPrefab;
    public GameObject fileOptionPrefab;
    public static string inputFileDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\RoboDojo_Files";
    public static string arenasFileDirectory = inputFileDirectory + "\\Arenas";
    private List<string> fileOptions = new List<string>();
    private List<string> arenaBundles = new List<string>();
    private GameData gameData;
    private Dictionary<string, ArenaOption> arenaOptionMap = new Dictionary<string, ArenaOption>();
    private Dictionary<string, int> inputFileIndexMap = new Dictionary<string, int>();
    private Dictionary<ArenaOption, bool> isAssetBundleMap = new Dictionary<ArenaOption, bool>();


    // Start is called before the first frame update
    void Start()
    {

        if(ArenaManager.instance == null)
        {
            GameObject newArenaManager = new GameObject();
            newArenaManager.name = "ArenaManager";
            newArenaManager.AddComponent<ArenaManager>();
        }

        gameData = LoadGameDataJSON();

        foreach(ArenaOption option in gameData.arenaOptions)
        {
            arenaOptionMap.Add(option.name, option);
        }

        GetArenaScenes();
        GetArenaAssetBundles();     
        GetInputFiles();

        foreach (ArenaOption arenaOption in gameData.arenaOptions)
        {
            bool buildOption = false;

            if((arenaOption.name == "Default Arena" || Application.CanStreamedLevelBeLoaded(arenaOption.name)) && arenaOption.name != "ArenaUI")
            {
                buildOption = true;
                isAssetBundleMap.Add(arenaOption, false);
                Debug.Log("Added Option: " + arenaOption.name + " - not a bundle");
            }
            else if(File.Exists(arenasFileDirectory + "\\" + arenaOption.name))
            {
                buildOption = true;
                isAssetBundleMap.Add(arenaOption, true);
                Debug.Log("Added Option: " + arenaOption.name + " - bundle");
            }

            if (buildOption)
            {
                BuildArenaOption(arenaOption);
            }
        }
    }

    public static  GameData LoadGameDataJSON()
    {
        GameData gameData = new GameData();
        string path = Application.persistentDataPath + "/gameData.json";

        if (File.Exists(path))
        {
            //Debug.Log("Found game data file");
            gameData = JsonUtility.FromJson<GameData>(File.ReadAllText(path));
        }
        else
        {
            //Debug.Log("Did not find game data file");
            ArenaOption defaultArena = new ArenaOption();
            defaultArena.name = "Default Arena";
            defaultArena.highscore = 0;
            defaultArena.inputFile = null;

            gameData.arenaOptions = new ArenaOption[] { defaultArena };

            SaveGameDataJSON(gameData);
        }
        
        return gameData;
    }

    public static void SaveGameDataJSON(GameData gameData)
    {
        string gameDataJSON = JsonUtility.ToJson(gameData);
        File.WriteAllText(Application.persistentDataPath + "/gameData.json", gameDataJSON);
        //Debug.Log("persistent data path = " + Application.persistentDataPath);
    }

    private void BuildArenaOption(ArenaOption arenaOption)
    {
        GameObject arenaOptionUI = Instantiate(ArenaOptionPrefab, arenaListContainer);

        Button defaultArenaStartButton = arenaOptionUI.transform.Find("Start").GetComponent<Button>();
        defaultArenaStartButton.onClick.AddListener(delegate { StartSimulation(arenaOption); });

        TextMeshProUGUI arenaName = arenaOptionUI.transform.Find("ArenaName").GetComponent<TextMeshProUGUI>();
        arenaName.SetText(arenaOption.name);

        TextMeshProUGUI highscore = arenaOptionUI.transform.Find("Highscore").GetComponent<TextMeshProUGUI>();
        highscore.SetText("High Score: " + arenaOption.highscore.ToString());

        Dropdown dropdown = arenaOptionUI.transform.Find("FileOptions").GetComponent<Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(fileOptions);

        if (inputFileIndexMap.ContainsKey(arenaOption.inputFile))
        {
            dropdown.value = inputFileIndexMap[arenaOption.inputFile];
        }

        dropdown.onValueChanged.AddListener(delegate { OnInputFileChange(arenaOption.name, dropdown); });
    }

    private void OnInputFileChange(string arenaName, Dropdown dropdown)
    {
        ArenaOption arena = arenaOptionMap[arenaName];
        arena.inputFile = dropdown.options[dropdown.value].text;
        SaveGameDataJSON(gameData);
    }

    private void GetArenaScenes()
    {
        List<string> scenesInBuild = new List<string>();
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            int lastSlash = scenePath.LastIndexOf("/");
            scenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
        }

        List<ArenaOption> arenaOptions = new List<ArenaOption>(gameData.arenaOptions);

        foreach (string scene in scenesInBuild)
        {
            if (!arenaOptionMap.ContainsKey(scene))
            {
                ArenaOption newOption = new ArenaOption();
                newOption.name = scene;
                newOption.inputFile = "";

                arenaOptions.Add(newOption);
                arenaOptionMap.Add(scene, newOption);
            }
        }

        gameData.arenaOptions = arenaOptions.ToArray();
        SaveGameDataJSON(gameData);
    }

    private void GetArenaAssetBundles()
    {
        if (!Directory.Exists(arenasFileDirectory))
        {
            Directory.CreateDirectory(arenasFileDirectory);
        }
        else //If the directory exists, look for arenas
        {
            foreach (string path in Directory.GetFiles(arenasFileDirectory))
            {
                string[] splitPath = path.Split('\\');
                string fileNameAndExt = splitPath[splitPath.Length - 1];
                string[] splitFileName = fileNameAndExt.Split('.');
                string name = splitFileName[0];
                string extension = "";

                if(splitFileName.Length > 1)
                {
                    extension = splitFileName[1];
                }

                if (extension == "")
                {
                    arenaBundles.Add(name);
                    Debug.Log("Found Arena Bundle: " + name);
                }
            }

            List<ArenaOption> arenaOptions = new List<ArenaOption>(gameData.arenaOptions);

            foreach (string arenaBundle in arenaBundles)
            {
                if (!arenaOptionMap.ContainsKey(arenaBundle))
                {
                    ArenaOption newOption = new ArenaOption();
                    newOption.name = arenaBundle;
                    newOption.inputFile = "";

                    arenaOptions.Add(newOption);
                    arenaOptionMap.Add(arenaBundle, newOption);
                }
            }

            gameData.arenaOptions = arenaOptions.ToArray();
            SaveGameDataJSON(gameData);
        }
    }

    private void GetInputFiles()
    {
        if (!Directory.Exists(inputFileDirectory))
        {
            Directory.CreateDirectory(inputFileDirectory);
            Directory.CreateDirectory(arenasFileDirectory);
        }

        fileOptions.Add("none");

        int dropdownIndex = 1;

        foreach (string path in Directory.GetFiles(inputFileDirectory))
        {
            string[] splitPath = path.Split('\\');
            string fileNameAndExt = splitPath[splitPath.Length -1];
            string[] splitFileName = fileNameAndExt.Split('.');
            string name = splitFileName[0];
            string extension = splitFileName[1];

            if(extension == "iqblocks")
            {
                fileOptions.Add(name);
                inputFileIndexMap.Add(name, dropdownIndex);
                dropdownIndex++;
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartSimulation(ArenaOption arena)
    {
        PlayerPrefs.SetString("currentArena", arena.name);
        PlayerPrefs.SetString("inputFileName", arena.inputFile);

        if (isAssetBundleMap[arena])
        {
            StartCoroutine(ArenaManager.instance.LoadSceneFromBundle(arenasFileDirectory + "\\" + arena.name, LoadSceneMode.Single));
        }
        else
        {
            SceneManager.LoadScene(arena.name);
            //SceneManager.LoadScene("ArenaUI", LoadSceneMode.Additive);

        }

        StartCoroutine(ArenaManager.instance.LoadSceneFromBundle(Application.dataPath + "\\AssetBundles\\arena ui", LoadSceneMode.Additive));
    }
}
