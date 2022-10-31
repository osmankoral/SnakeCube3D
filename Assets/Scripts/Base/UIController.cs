using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject WinPanel, LosePanel, InGamePanel, TutorialPanel, StartGamePanel;
    [SerializeField] private TextMeshProUGUI moneyText, scoreTxt, endGAmeScore, highScore;
    [SerializeField] private List<string> moneyMulti = new();
    [SerializeField] private GameObject coin, money;

    private Canvas UICanvas;

    private Button Next, Restart;

    private LevelManager levelManager;

    private GameController gameController;

    private Settings settings;

    private void Awake()
    {
        ScriptInitialize();
        ButtonInitialize();
    }

    private void Start()
    {
        GameManager.Instance.OnMoneyChange.Invoke();
    }

    void ScriptInitialize()
    {
        levelManager = FindObjectOfType<LevelManager>();
        gameController = FindObjectOfType<GameController>();
        settings = FindObjectOfType<Settings>();
        UICanvas = GetComponentInParent<Canvas>();
    }

    void ButtonInitialize()
    {
        Next = WinPanel.GetComponentInChildren<Button>();
        Restart = LosePanel.GetComponentInChildren<Button>();

        Next.onClick.AddListener(() => levelManager.LoadLevel(1));
        Restart.onClick.AddListener(() => levelManager.LoadLevel(0));

    }


    public void RestartFunc()
    {
        StartCoroutine(Delay());
    }
    IEnumerator Delay()
    {
        Time.timeScale = 1f;
        yield return new WaitForSeconds(0.1f);
        gameController.isSave = true;
        PlayerPrefs.SetString("IsGameSaved", "false");
        levelManager.LoadLevel(0);
    }

    void ShowPanel(GameObject panel, bool canvasMode = false)
    {
        panel.SetActive(true);
        GameObject panelChild = panel.transform.GetChild(0).gameObject;
        panelChild.transform.localScale = Vector3.zero;
        panelChild.SetActive(true);
        panelChild.transform.DOScale(Vector3.one, 0.5f);

        UICanvas.worldCamera = Camera.main;
        UICanvas.renderMode = canvasMode ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
    }

    void GameReady()
    {
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
        InGamePanel.SetActive(true);
        ShowTutorial();
    }

    void StartGamePanelOn()
    {
        StartGamePanel.SetActive(true);
        //highScore1.text = PlayerPrefs.GetInt("Score1").ToString();
    }



    void SetMoneyText()
    {
        if (coin.activeSelf)
            coin.transform.DOPunchScale(Vector3.one, 0.5f, 2, 1);

        if (money.activeSelf)
            money.transform.DOPunchScale(Vector3.one, 0.5f, 2, 1);

        int moneyDigit = GameManager.Instance.PlayerMoney.ToString().Length;
        int value = (moneyDigit - 1) / 3;
        if (value < 1)
        {
            moneyText.text = GameManager.Instance.PlayerMoney.ToString();
        }
        else
        {
            float temp = GameManager.Instance.PlayerMoney / Mathf.Pow(1000, value);
            moneyText.text = temp.ToString("F2") + " " + moneyMulti[value];
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.LevelFail.AddListener(() => ShowPanel(LosePanel, true));
        GameManager.Instance.LevelSuccess.AddListener(() => ShowPanel(WinPanel, true));
        GameManager.Instance.GameReady.AddListener(GameReady);
        GameManager.Instance.OnMoneyChange.AddListener(SetMoneyText);
    }

    private void OnDisable()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.LevelFail.RemoveListener(() => ShowPanel(LosePanel, true));
            GameManager.Instance.LevelSuccess.RemoveListener(() => ShowPanel(WinPanel, true));
            GameManager.Instance.GameReady.RemoveListener(GameReady);
        }
    }

    public void ShowTutorial()
    {
        TutorialPanel.transform.GetChild(settings.GetTutorialIndex()).gameObject.SetActive(true);
    }

    public void ScoreTxt(int _score)
    {
        scoreTxt.text = _score.ToString();
        endGAmeScore.text = _score.ToString();
        highScore.text = PlayerPrefs.GetInt("Score1").ToString();
    }
}
