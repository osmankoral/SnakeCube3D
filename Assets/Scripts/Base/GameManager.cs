using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public UnityEvent GameStart = new();
    [HideInInspector] public UnityEvent GameReady = new();
    [HideInInspector] public UnityEvent GameEnd = new();
    [HideInInspector] public UnityEvent LevelSuccess = new();
    [HideInInspector] public UnityEvent LevelFail = new();
    [HideInInspector] public UnityEvent OnMoneyChange = new();

    private GameController gameController;

    

    private float playerMoney;
    public float PlayerMoney
    {
        get
        {
            return playerMoney;
        }
        set
        {
            playerMoney = value;
            OnMoneyChange.Invoke();
        }
    }

    private bool hasGameStart;
    public bool HasGameStart
    {
        get
        {
            return hasGameStart;
        }
        set
        {
            hasGameStart = value;
        }
    }


    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        LoadData();
    }

    public void LevelState(bool value)
    {
        if (value) LevelSuccess.Invoke();
        else LevelFail.Invoke();
    }

    private void OnEnable()
    {
        GameStart.AddListener(() => hasGameStart = true);
        GameEnd.AddListener(() => hasGameStart = false);
    }

    private void OnDisable()
    {
        SaveData();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveData();
        }
        else
        {
            LoadData();
        }
    }

    void LoadData()
    {
        PlayerMoney = PlayerPrefs.GetFloat("PlayerMoney", 0);
    }

    void SaveData()
    {
        gameController.IsGameEnd();
        PlayerPrefs.SetFloat("PlayerMoney", playerMoney);
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
