﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public GameObject thirdPersonCamera;
    public GameObject birdsEyeCamera;
    public TextMeshProUGUI scoreboard;
    public Bot bot;
    public bool gamePlaying = false;
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
    public TextMeshProUGUI distance;
    public TextMeshProUGUI velocityDrive;
    public TextMeshProUGUI velocityTurn;

    private void Awake()
    {
        //SceneManager.LoadScene("ArenaUI", LoadSceneMode.Additive);
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
        positionY.SetText((bot.transform.position.y*1000).ToString("0.000"));
        rotationLocal.SetText(bot.transform.localRotation.eulerAngles.y.ToString("0.000"));
        rotationGlobal.SetText(bot.transform.rotation.eulerAngles.y.ToString("0.000"));
        velocityDrive.SetText((DrivetrainController.instance.botDriveVelocity*100).ToString("0.000"));
        velocityTurn.SetText((DrivetrainController.instance.botTurnVelocity*100).ToString("0.000"));

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
        gamePlaying = !gamePlaying;
        Debug.Log("gamePlaying = " + gamePlaying);

        if (gamePlaying)
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
        Debug.Log("Restart");
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
