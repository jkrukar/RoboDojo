using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class UIController : MonoBehaviour
{

    public GameObject thirdPersonCamera;
    public GameObject birdsEyeCamera;
    public TextMeshProUGUI scoreboard;
    public TextMeshProUGUI arenaTitle;
    public Bot bot;
    public bool sensorsShowing = true;

    public Sprite playIcon;
    public Sprite stopIcon;
    public Image playStopButton;

    public GameObject sensorDataPanel;
    public TextMeshProUGUI positionX;
    public TextMeshProUGUI positionY;
    public TextMeshProUGUI rotationLocal;
    public TextMeshProUGUI rotationGlobal;
    public TextMeshProUGUI colorFront;
    public TextMeshProUGUI colorDown;
    public TextMeshProUGUI bumperLeft;
    public TextMeshProUGUI bumperRight;
    public TextMeshProUGUI distance0;
    public TextMeshProUGUI distance45;
    public TextMeshProUGUI velocityDrive;
    public TextMeshProUGUI velocityTurn;
    public TextMeshProUGUI botTime;
    public TextMeshProUGUI arenaTime;

    private void Awake()
    {
        arenaTitle.SetText(PlayerPrefs.GetString("currentArena"));
    }

    // Start is called before the first frame update
    void Start()
    {
        bot = GameObject.FindGameObjectWithTag("Bot").GetComponent<Bot>();
    }

    // Update is called once per frame
    void Update()
    {
        int points = Bot.instance.coinCount;

        scoreboard.text = points.ToString();

        UpdateSensorDataPanel();
    }

    private void UpdateSensorDataPanel()
    {
        positionX.SetText((bot.transform.position.x*1000).ToString("0.000"));
        positionY.SetText((bot.transform.position.z*1000).ToString("0.000"));
        //rotationLocal.SetText(bot.transform.localRotation.eulerAngles.y.ToString("0.000"));
        rotationLocal.SetText((Vector3.SignedAngle(bot.transform.forward, Vector3.forward, Vector3.up)*-1).ToString("0.000"));
        rotationGlobal.SetText(bot.transform.rotation.eulerAngles.y.ToString("0.000"));
        velocityDrive.SetText((DrivetrainController.instance.botDriveVelocity*100).ToString("0.00"));
        velocityTurn.SetText((DrivetrainController.instance.botTurnVelocity*100).ToString("0.00"));
        distance0.SetText((SensingController.instance.distanceSensorValue0 * 1000).ToString("0.000"));
        distance45.SetText((SensingController.instance.distanceSensorValue45 * 1000).ToString("0.000"));
        bumperLeft.SetText(SensingController.instance.leftBumperSensor.collisionState.ToString());
        bumperRight.SetText(SensingController.instance.rightBumperSensor.collisionState.ToString());

        string botTimeString = GetTimeString(EventsController.instance.botTimer);
        botTime.SetText(botTimeString);

        string arenaTimeString = GetTimeString(EventsController.instance.arenaTimer);
        arenaTime.SetText(arenaTimeString);
    }

    public static string GetTimeString(float timeInSeconds)
    {
        int seconds = (int)timeInSeconds;
        int milliseconds = (int)((timeInSeconds - (float)seconds) * 1000f);
        int minutes = seconds / 60;
        return minutes.ToString("D2") + ":" + (seconds % 60).ToString("D2") + ":" + milliseconds.ToString("D3");
    }

    public void SwitchCameras()
    {
        if (thirdPersonCamera.activeSelf)
        {
            thirdPersonCamera.SetActive(false);
            birdsEyeCamera.SetActive(true);
        }
        else
        {
            thirdPersonCamera.SetActive(true);
            birdsEyeCamera.SetActive(false);
        }
    }

    public void GoHome()
    {
        Debug.Log("Go Home");
        SceneManager.LoadScene("Home");
    }

    public void PlayStop()
    {
        BlockParser.gamePlaying = !BlockParser.gamePlaying;
        Debug.Log("gamePlaying = " + BlockParser.gamePlaying);

        if (BlockParser.gamePlaying)
        {                   
            playStopButton.sprite = stopIcon;
        }
        else
        {
            playStopButton.sprite = playIcon;
        }        
    }

    public void Restart()
    {
        if (BlockParser.gamePlaying)
        {
            PlayStop();
        }
        
        Scene activeScene = SceneManager.GetActiveScene();

        Debug.Log("Restart: ");

        SceneManager.LoadScene(activeScene.name);

        StartCoroutine(ArenaManager.instance.LoadSceneFromBundle(Application.dataPath + "\\AssetBundles\\arena ui", LoadSceneMode.Additive));
    }

    public void ToggleSensorDisplay()
    {
        sensorsShowing = !sensorsShowing;

        if (sensorsShowing)
        {
            sensorDataPanel.SetActive(true);
        }
        else
        {
            sensorDataPanel.SetActive(false);
        }
    }
}
