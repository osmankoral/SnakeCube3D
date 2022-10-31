using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CubeManager : MonoBehaviour
{

    private TextMeshProUGUI[] nuberTexts = new TextMeshProUGUI[6];
    private MeshRenderer mr;

    GameController gameController;

    private int cubeValue;
    public int cubeIndex;
    public bool isEating;
    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        mr = GetComponentInChildren<MeshRenderer>();
        for(int i=0; i<6; i++){ nuberTexts[i] = transform.GetChild(0).GetChild(i).GetComponent<TextMeshProUGUI>(); }
    }

    public void CubeValueChange(int _change)
    {
        cubeIndex = _change;
        CubeUpdate();
    }

    private void CubeUpdate()
    {
        cubeValue = GameController.valueArray[cubeIndex];
        if (cubeIndex == 0)
            foreach (TextMeshProUGUI text in nuberTexts) { text.text = " "; }

        else
            foreach (TextMeshProUGUI text in nuberTexts) { text.text = cubeValue.ToString(); }

        mr.material = GameController.MaterialList[cubeIndex];

        if(!gameObject.CompareTag("Cube") && cubeIndex != 0)  Effect();
    }

    private void Effect()
    {
        transform.DOScale(1.5f, 0.1f).OnComplete(() => transform.DOScale(1f, 0.1f));
        
    }
    public void Effect(int _select)
    {
        switch(_select)
        {
            case 0:
                StartCoroutine(Bomb());
                break;
            case 1:
                StartCoroutine(TailUp());
                break;
        }
    }   
    IEnumerator Bomb()
    {
        transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
    }
    IEnumerator TailUp()
    {
        transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
    }

   private void OnDisable()
    {
        if (!isEating)
            gameController.IsGameEnd();
    }
}
