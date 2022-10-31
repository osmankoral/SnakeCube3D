using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameController gameController;
    Animator animator;
    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        animator = GetComponentInChildren<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
            animator.SetTrigger("isEat");
            other.GetComponent<CubeManager>().isEating = true;
            gameController.EatCube(0, other.GetComponentInParent<CubeManager>().cubeIndex);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Other"))
        {
            animator.SetTrigger("isEat");
            int temp = other.GetComponentInParent<OtherManager>().index;
            other.GetComponentInParent<OtherManager>().isEating = true;
            gameController.EatCube(temp);
            Destroy(other.transform.parent.gameObject);
        }

        else if (other.CompareTag("Wall"))
        {
            Debug.Log("Game Over!");
            gameController.EndGame();
            GameManager.Instance.LevelState(false);
        }
      
    }

}
