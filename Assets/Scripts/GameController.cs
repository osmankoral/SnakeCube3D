using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static Material[] MaterialList = new Material[14];
    public static int[] valueArray = { 0, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
    

    [SerializeField] private Material[] materialList;
    [SerializeField] private GameObject playerPref;
    [SerializeField] private GameObject tailPref;
    [SerializeField] private GameObject tailTransform;
    [SerializeField] private GameObject cubePref;
    [SerializeField] private GameObject playerTailPref;
    [SerializeField] private GameObject[] otherPrefabs;
    [SerializeField] private Collider gridArea;
    [SerializeField] private Transform cubesParent;
    [SerializeField] private Transform othersParent;

    UIController UIController;
    JoystickRb playerJRB;

    private List<int> tailsIndexList = new();
    private List<CubeManager> tails = new();
    private List<int[]> queueList = new();
    private Bounds bounds;
    private GameObject player;
    private GameObject playerTail;
    private GameObject cubeObject;

    private float timerCube;
    private float timerOther = 15f;
    private bool isCubeEat;
    public bool isSave;
    private float speedTimer;
    

    private void Awake()
    {
        UIController = FindObjectOfType<UIController>();
    }

    void Start()
    {
        bounds = this.gridArea.bounds;

        for(int j=0;j<14;j++) { MaterialList[j] = materialList[j]; }
        if (!PlayerPrefs.HasKey("IsGameSaved")) PlayerPrefs.SetString("IsGameSaved", "false");
        switch (PlayerPrefs.GetString("IsGameSaved"))
        {
            case "true": LoadGame(); break;
            case "false": NewGame(); break;
        }

    }

    public void IsGameEnd()
    {
        if(!isSave)
        {
            Debug.Log("GameSaved");
            isSave = true;
            SaveGame();
        }
    }
    public void EndGame()
    {
        isSave = true;
        playerJRB.IsGamePasued = true;
        playerJRB.IsGameStart = false;
        ScoreUpdate();
        PlayerPrefs.SetString("IsGameSaved", "false");
        GameManager.Instance.LevelState(false);
    }

    //int _index, float _x, float _z, float _rotation
    public void SaveGame()
    {
        playerJRB.ListSave();
        PlayerPrefs.SetString("IsGameSaved", "true");

        //Snake head Values
        PlayerPrefs.SetInt("SnakeGap", playerJRB.gap);
        PlayerPrefs.SetFloat("SnakeSpeed", playerJRB.MoveSpeed);

        //Tails Values
        PlayerPrefs.SetInt("TailCount", tails.Count);
        for(int i=0; i< tails.Count; i++)
        {
            PlayerPrefs.SetInt("Tails" + i + "Index", tailsIndexList[i]);
        }

        //Cubes Values
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Cube");

        PlayerPrefs.SetInt("CubesCount", temp.Length);
        for (int i = 0; i < temp.Length; i++)
        {
            PlayerPrefs.SetInt("Cubes" + i + "Index", temp[i].GetComponent<CubeManager>().cubeIndex);
            PlayerPrefs.SetFloat("Cubes" + i + "X", temp[i].transform.position.x);
            PlayerPrefs.SetFloat("Cubes" + i + "Z", temp[i].transform.position.z);
        }

        //Other Values

            GameObject[] tempOther = GameObject.FindGameObjectsWithTag("Other");
            PlayerPrefs.SetInt("OthersCount", tempOther.Length);

            for (int i = 0; i < tempOther.Length; i++)
            {
                int tempIndex = tempOther[i].GetComponent<OtherManager>().index - 1;
                PlayerPrefs.SetInt("Others" + i + "Index", tempIndex);
                PlayerPrefs.SetFloat("Others" + i + "X", tempOther[i].transform.position.x);
                PlayerPrefs.SetFloat("Others" + i + "Z", tempOther[i].transform.position.z);
            }


    }


    private void LoadGame()
    {

        playerTail = Instantiate(playerTailPref, Vector3.zero, Quaternion.identity);
        player = Instantiate(playerPref,Vector3.zero, Quaternion.identity);

        playerJRB = player.GetComponent<JoystickRb>();
        playerJRB.isLoadGame = true;

        

        int tailCount = PlayerPrefs.GetInt("TailCount");
        for(int i=0;i<tailCount;i++)
        {
            int index = PlayerPrefs.GetInt("Tails" + i + "Index");
            AddTail(index);
        }
        int cubesCount = PlayerPrefs.GetInt("CubesCount");
        for (int i = 0; i < cubesCount; i++)
        {
            int index = PlayerPrefs.GetInt("Cubes" + i + "Index");
            float x = PlayerPrefs.GetFloat("Cubes" + i + "X");
            float z = PlayerPrefs.GetFloat("Cubes" + i + "Z");
            CreateCubeLoad(index, x, z);
        }
        int otherCount = PlayerPrefs.GetInt("OthersCount");
        for (int i = 0; i < otherCount; i++)
        {
            int index = PlayerPrefs.GetInt("Others" + i + "Index");
            float x = PlayerPrefs.GetFloat("Others" + i + "X");
            float z = PlayerPrefs.GetFloat("Others" + i + "Z");
            CreateOther(index, x, z);
        }
        ScoreUpdate();
        SetGame();
    }

    private void NewGame()
    {
        playerTail = Instantiate(playerTailPref, Vector3.zero, Quaternion.identity);
        player = Instantiate(playerPref, Vector3.zero, Quaternion.identity);
        playerJRB = player.GetComponent<JoystickRb>();
        playerJRB.isLoadGame = false;
        isSave = false;
        for(int i=0; i<4; i++) { AddTail(); }
        for(int k=0; k<5; k++) { CreateCube(); }

        SetGame();

    }
    private void ChallengeGame()
    {

    }

    private void PuzzleGame()
    {

    }

    private void SetGame()
    {
        InvokeRepeating(nameof(CreateCube), timerCube, timerCube);
        InvokeRepeating(nameof(CreateOther), timerOther, timerOther);
    }


    public void GameLoad(int select)
    {
        isSave = false;
        switch(select)
        {
            case 0:
                NewGame();
                    break;
            case 1:
                LoadGame();
                break;
            case 2:
                ChallengeGame();
                break;
            case 3:
                PuzzleGame();
                break;
        }
    }

    public void GamePause()
    {
        //playerJRB.IsGamePasued = true;
        Time.timeScale = 0;
    }    
    public void GameContinue()
    {
        //playerJRB.IsGamePasued = false;
        Time.timeScale = 1;
    }


    private void Update()
    {
        if(playerJRB.IsGameStart && !playerJRB.IsGamePasued)
        {
            //if (isSave) isSave = !isSave;
            speedTimer += Time.deltaTime;  
            if (speedTimer > 10f) SpeedTimer();
        }
    }

    private void SpeedTimer()
    {
        playerJRB.MoveSpeed += (playerJRB.MoveSpeed > 175) ? 1f: 5f;
        speedTimer = 0;
    }




    private void AddTail(int _index = 0)
    {
        GameObject tail = Instantiate(tailPref, tailTransform.transform);
        tail.GetComponent<CubeManager>().CubeValueChange(_index);
        tails.Add(tail.GetComponent<CubeManager>()); tailsIndexList.Add(_index);
    }



    private void CreateCubeLoad(int _index, float _x, float _z)
    {
        float x =  _x ;
        float z =  _z ;

        GameObject cube = Instantiate(cubePref, new Vector3(x, 0.3f, z), Quaternion.identity);
        cube.transform.SetParent(cubesParent);
        int rnd = _index; cube.GetComponent<CubeManager>().CubeValueChange(rnd);
        timerCube = Random.Range(5f, 10f);
    }
    private void CreateOther(int _index, float _x, float _z)
    {
        float x = _x;
        float z = _z;

        GameObject cube = Instantiate(otherPrefabs[_index], new Vector3(x, 0.4f, z), Quaternion.identity);
        cube.transform.SetParent(othersParent);
        timerOther = Random.Range(15f, 15f);
    }

    private void CreateCube()
    {
        Vector2 temp = PositionCheck();
        
        GameObject cube = Instantiate(cubePref, new Vector3(temp.x, 0.4f, temp.y), Quaternion.identity);

        cube.transform.SetParent(cubesParent);

        int rnd = Random.Range(1, 6); cube.GetComponent<CubeManager>().CubeValueChange(rnd);
        timerCube = Random.Range(5f, 10f);
    }


    private void CreateOther()
    {
        Vector2 temp = PositionCheck();
        int rnd = Random.Range(0, 101);
        rnd = 100 - rnd;
        GameObject other = null;
        int _index = 0;
        if(rnd < 10)
            _index = 0;

        else if (rnd < 30)
            _index = 1;
        
        else if (rnd < 60)
            _index = 2;

        else
            _index = 3;
        
        other = Instantiate(otherPrefabs[_index], new Vector3(temp.x, 0.4f, temp.y), Quaternion.identity);
        other.transform.SetParent(othersParent);
        timerOther = Random.Range(15f, 45f);
    }

    private Vector2 PositionCheck()
    {
        float x = Random.Range(bounds.min.x + 1f, bounds.max.x - 1f); x = Mathf.Round(x);
        float z = Random.Range(bounds.min.z + 1f, bounds.max.z - 1f); z = Mathf.Round(z);


        for(int i=0; i<cubesParent.childCount;i++ )
        {
            if (x == cubesParent.GetChild(i).position.x && z == cubesParent.GetChild(i).position.z) {  PositionCheck(); break; }
        }

        foreach (Transform otherTans in othersParent)
        {
            if (x == otherTans.position.x && z == otherTans.position.z) { PositionCheck(); break; }
        }

        if (Mathf.Abs(x - player.transform.position.x) < 4 && Mathf.Abs(z - player.transform.position.z) < 4) { PositionCheck(); }


        return new Vector2(x, z);
    }

    

    public void EatCube(int _funcIndex, int _valueIndex = 0)
    {
        int[] tempArray = { _funcIndex , _valueIndex };
        queueList.Add(tempArray);
        if (!isCubeEat) Queue();
    }
    private void Queue()
    {
        isCubeEat = true;
        int[] tempArray = { queueList[0][0], queueList[0][1] };
        int func1 = tempArray[0];
        int func2 = tempArray[1];
        switch (func1)
        {
            case 0:
                StartCoroutine(TailUpdate(func2));
                break;
            case 1:
                AppleFunction();
                break;
            case 2:
                BombFuntion();
                break;
            case 3:
                MultiplicationFuntion();
                break;
            case 4:
                DivisionFuntion();
                break;
        }

    }
    
    private void QueueEnd()
    {
        queueList.RemoveAt(0);
        if (queueList.Count > 0) Queue();
        else isCubeEat = false;
    }

    IEnumerator TailUpdate(int _index, int _listIndex = 0)
    {
        int index = _index;
        for(int i=_listIndex; i<tailsIndexList.Count; i++)
        {
            if (tailsIndexList[0] != 0 && tailsIndexList[0] != index) {EndGame(); break; }
            if (tailsIndexList[i] == 0)
            {
                if (i == tailsIndexList.Count-1)
                {
                    tailsIndexList[i] = index;
                    tails[i].CubeValueChange(index);
                }
                else
                { 
                    tails[i].CubeValueChange(index);
                    if (tailsIndexList[i + 1] == 0)
                    {
                        yield return new WaitForSeconds(0.2f);
                        tails[i].CubeValueChange(0);
                    }
                    else if (tailsIndexList[i + 1] == index)
                    {
                        yield return new WaitForSeconds(0.2f);
                        tails[i].CubeValueChange(0);
                    }
                    else
                    {
                        tailsIndexList[i] = index;
                        break;
                    }

                }
                
            }
            else
            {
                if(i == tailsIndexList.Count-1)
                {
                    if (tailsIndexList[i] == index)
                    {
                        index++;
                        if(index == valueArray.Length)
                        {
                            if(i-1 < 0) { EndGame(); break; }
                            index--;
                            tailsIndexList[i-1] = index;
                            tails[i - 1].Effect(1);
                            tails[i-1].CubeValueChange(index);
                            break;
                        }
                        else
                        {
                            tailsIndexList[i] = index;
                            tails[i].Effect(1);
                            tails[i].CubeValueChange(index);
                        }
                        
                    }
                    
                }
                else
                {
                    if (tailsIndexList[i] == index)
                    {
                        index++;
                        if (index == valueArray.Length)
                        {
                            if (i - 1 < 0) {EndGame(); break; }
                            index--;
                            tailsIndexList[i - 1] = index;
                            tails[i-1].Effect(1);
                            tails[i - 1].CubeValueChange(index);
                            break;
                        }
                        else
                        {
                            tails[i].Effect(1);
                            tails[i].CubeValueChange(index);
                            if (tailsIndexList[i + 1] == index)
                            {
                                yield return new WaitForSeconds(0.2f);
                                tailsIndexList[i] = 0;
                                tails[i].CubeValueChange(0);
                            }
                            else
                            {
                                tailsIndexList[i] = index;
                                break;
                            }
                        }
                    }

                    else
                    {
                        break;
                    }
                }
                  

              
            }
        }

        ScoreUpdate();
        QueueEnd();
    }

    private void AppleFunction()
    {
        AddTail();
        for(int i= tailsIndexList.Count-1; i>=0; i--)
        {
            tailsIndexList[i] = (i == 0) ? 0 : tailsIndexList[i - 1];
            tails[i].CubeValueChange(tailsIndexList[i]);
        }
        ScoreUpdate();
        QueueEnd();
    }

    private void BombFuntion()
    {
        for(int i=0; i< tailsIndexList.Count;i++)
        {
            if(tailsIndexList[i] != 0)
            {
                tailsIndexList[i] = 0;
                tails[i].Effect(0);
                tails[i].CubeValueChange(0);
                break;
            }
        }
        ScoreUpdate();
        QueueEnd();
    }

    private void MultiplicationFuntion()
    {
        for (int i = 0; i < tailsIndexList.Count; i++)
        {
            if (tailsIndexList[i] != 0)
            {
                int tempValue = tailsIndexList[i];
                StartCoroutine(TailUpdate(tempValue, i));
                break;
            }
        }
    }

    private void DivisionFuntion()
    {
        for (int i = 0; i < tailsIndexList.Count; i++)
        {
            if (tailsIndexList[i] != 0)
            {
                int tempValue = tailsIndexList[i]; tempValue--;
                tailsIndexList[i] = 0;
                StartCoroutine(TailUpdate(tempValue, i));
                break;
            }
        }
    }

    private void ScoreUpdate()
    {
        int _score = 0;
        foreach (int _index in tailsIndexList)
        {
            _score += valueArray[_index]; 
        }
        UIController.ScoreTxt(_score);
        if(_score > PlayerPrefs.GetInt("Score1"))
        {
            PlayerPrefs.SetInt("Score1", _score);   
        }
    }

    public void ExitApp()
    {
        SaveGame();
        GameManager.Instance.ExitApp();
    }
        
    
}
