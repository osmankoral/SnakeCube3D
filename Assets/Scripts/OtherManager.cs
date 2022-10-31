using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherManager : MonoBehaviour
{
    GameController gameController;

    public int index;
    public bool isEating;
    
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        Destroy(gameObject, 15f);
    }

    private void OnDisable()
    {
        if (!isEating)
            gameController.IsGameEnd();
    }


}
