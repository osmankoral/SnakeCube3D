using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickRb : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 50f;

    private Joystick joystick;
    private Rigidbody rb;
    private Transform tails;
    private Transform snakeTail;


    private List<Vector3> positionHistory = new();
    private List<Quaternion> rotationHistory = new();
    private List<GameObject> tailsList = new();

    public bool isLoadGame;

    public int gap;
    private int maxHistoryListCount = 150;
    private int tailCount;
    private bool isGameStart;
    private bool isGamePaused;

    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }
    public bool IsGameStart { get { return isGameStart; } set { isGameStart = value; } }
    public bool IsGamePasued { get { return isGamePaused; } set { isGamePaused = value; } }
     

    private void Awake()
    {
        joystick = GameObject.FindGameObjectWithTag("Joystick").GetComponent<Joystick>();
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        isGamePaused = false;
        tails = GameObject.FindGameObjectWithTag("Tail").transform;
        snakeTail = GameObject.FindGameObjectWithTag("SnakeTail").transform;

        if (PlayerPrefs.GetString("IsGameSaved") == "true")
        {
            gap = PlayerPrefs.GetInt("SnakeGap");
            moveSpeed = PlayerPrefs.GetFloat("SnakeSpeed");
            LoadGame();
        }
    }
  
    private void LoadGame()
    {

        ListLoad();
        UpdateTail();
        TailMove();
        transform.rotation = rotationHistory[0];
        transform.position = positionHistory[0];
    }
  
    public void SetJoystick(float value)
    {
        moveSpeed = value;

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    

    public void FixedUpdate()
    {
        if (tails.childCount != tailCount) UpdateTail();
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;

        rb.angularVelocity = Vector3.zero;

        if (Input.GetMouseButton(0) && !isGamePaused)
        {
            isGameStart = true;
            if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                transform.rotation = Quaternion.Euler(0f, (Mathf.Atan2(horizontal * 180, vertical * 180) * Mathf.Rad2Deg), 0f);
            }
        }
        if(isGameStart && !isGamePaused)
        { 
            PlayerMove();
            TailMove();
        }
        
    }

    private void PlayerMove()
    {
        rb.velocity = moveSpeed * Time.fixedDeltaTime * transform.forward;
        positionHistory.Insert(0, transform.position);
        rotationHistory.Insert(0, transform.rotation);

        maxHistoryListCount = Mathf.Max(150, gap * (tailCount+1) +1);

        if (positionHistory.Count > maxHistoryListCount)
        {
            positionHistory.RemoveAt(positionHistory.Count - 1);
            rotationHistory.RemoveAt(positionHistory.Count - 1);
        }

        if (positionHistory.Count > 2)
        {
            float tempGap = 0.8f / (Vector3.Distance(positionHistory[0], positionHistory[1]));
            gap = Mathf.RoundToInt(tempGap);
        }
    }

    private void TailMove()
    {
        int index = 1;
        for(int i=0; i< tailCount;i++)
        {
           // int tempGap = (i > 0) ? gap : gap * 2;
            int tailIndex = Mathf.Min(index * (gap), positionHistory.Count - 1);

            Vector3 point = positionHistory[tailIndex];
            point.y = tailsList[0].transform.position.y;


            tailsList[i].transform.position = point;

            tailsList[i].transform.rotation = rotationHistory[tailIndex];
            index++;
        }
        SnakeTailUpdate();
    }

    private void SnakeTailUpdate()
    {
        int snakeTailIndex = (tailCount * gap)+2;
        if(snakeTailIndex < positionHistory.Count )
        {
            Vector3 temp = positionHistory[snakeTailIndex];
            temp.y = 0.2f;
            snakeTail.position = temp;
          
            snakeTail.rotation = rotationHistory[snakeTailIndex];
        }
        else
        {
            snakeTail.position = new Vector3(0,0,0);
        }
    }

    private void UpdateTail()
    {
        int countDif = tails.childCount - tailCount;
        for(int i =0; i< countDif; i++)
        {
            tailsList.Add(tails.GetChild(tailCount+ i).gameObject);
        }
        tailCount += countDif;
    }

    public void ListSave()
    {
        int lastCount = 0;
        if (PlayerPrefs.HasKey("PositionListCont")) lastCount = PlayerPrefs.GetInt("PositionListCont");
        PlayerPrefs.SetInt("PositionListCont", positionHistory.Count);
        for(int i=0; i<positionHistory.Count; i++)
        {
            Vector3 tempVec = positionHistory[i];
            Quaternion tempQua = rotationHistory[i];
            PlayerPrefs.SetFloat("PositionHistortX"+i, tempVec.x);
            PlayerPrefs.SetFloat("PositionHistortZ"+i, tempVec.z);
            PlayerPrefs.SetFloat("PositionHistortY"+i, tempQua.y);
            PlayerPrefs.SetFloat("PositionHistortW"+i, tempQua.w);
        }
        if(positionHistory.Count < lastCount)
        {
            for(int j= positionHistory.Count-1; j< lastCount; j++)
            {
                PlayerPrefs.DeleteKey("PositionHistortX" + j);
                PlayerPrefs.DeleteKey("PositionHistortZ" + j);
                PlayerPrefs.DeleteKey("PositionHistortY" + j);
                PlayerPrefs.DeleteKey("PositionHistortW" + j);
            }
        }
    }

    private void ListLoad()
    {
        for(int i = 0; i<PlayerPrefs.GetInt("PositionListCont"); i++)
        {
            Vector3 tempVec = new Vector3(PlayerPrefs.GetFloat("PositionHistortX" + i), transform.position.y, PlayerPrefs.GetFloat("PositionHistortZ" + i));
            Quaternion tempQua = transform.rotation;
            tempQua.y = PlayerPrefs.GetFloat("PositionHistortY" + i);
            tempQua.w = PlayerPrefs.GetFloat("PositionHistortW" + i);
            positionHistory.Add(tempVec);
            rotationHistory.Add(tempQua);
        }
    }
}
