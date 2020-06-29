using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : Singleton<Bot>
{
    public GameData gameData;
    public int coinCount = 0;
    ArenaOption currentArena;

    // Start is called before the first frame update
    void Start()
    {
        gameData = HomeUIController.LoadGameDataJSON();
        string currentArenaName = PlayerPrefs.GetString("currentArena");
        

        foreach(ArenaOption option in gameData.arenaOptions)
        {
            if(option.name == currentArenaName)
            {
                currentArena = option;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CollectCoin(int value)
    {
        coinCount += value;
        Debug.Log("coinCount = " + coinCount);

        if(coinCount > currentArena.highscore)
        {
            currentArena.highscore = coinCount;
            HomeUIController.SaveGameDataJSON(gameData);
        }
    }
}
