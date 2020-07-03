﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{

    public GameObject thirdPersonCamera;
    public GameObject birdsEyeCamera;
    public TextMeshProUGUI scoreboard;
    public Bot bot;

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
}
