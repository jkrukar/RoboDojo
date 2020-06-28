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
    private string inputFileDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\VEX_Simulator_Code";
    private List<string> fileOptions = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        GetInputFiles();
        BuildArenaOptions();
        BuildFileOptionDropdowns();
    }

    private void BuildArenaOptions()
    {
        //Build for default Arena
        GameObject defaultArena = Instantiate(ArenaOptionPrefab, arenaListContainer);
        Button defaultArenaStartButton = defaultArena.transform.Find("Start").GetComponent<Button>();
        defaultArenaStartButton.onClick.AddListener(delegate { StartSimulation("Arena"); });
        TextMeshProUGUI arenaName = defaultArena.transform.Find("ArenaName").GetComponent<TextMeshProUGUI>();
        arenaName.SetText("Default Arena");


        for (int i = 0; i < 3; i++)
        {
            Instantiate(ArenaOptionPrefab, arenaListContainer);
        }
    }

    private void BuildFileOptionDropdowns()
    {
        foreach (GameObject dropdownObject in GameObject.FindGameObjectsWithTag("FileOptionsDropdown"))
        {
            Dropdown dropdown = dropdownObject.GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(fileOptions);
        }
    }

    private void GetInputFiles()
    {
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
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartSimulation(string arenaName)
    {
        SceneManager.LoadScene(arenaName);
    }

}
