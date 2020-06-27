using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{

    public GameObject thirdPersonCamera;
    public GameObject birdsEyeCamera;
    public TextMeshProUGUI scoreboard;

    // Start is called before the first frame update
    void Start()
    {
        
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
}
