using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameBase : MonoBehaviour
{

    private string inputString;



    //Used for UI
    [SerializeField] TextMeshProUGUI[] pText = new TextMeshProUGUI[4];
    [SerializeField] TextMeshProUGUI infoText;


    void Start()
    {
        DistributeCards();
    }

    private void DistributeCards()
    {
        for (int i = 0; i < 44; i++)
        {
            SBU.cards[i] = i % 4 + 1 + 8 * Random.Range(0, 2) + 16 * (i / 4);
        }
        SBU.cards = SBU.ShuffleCards(SBU.cards);

    }


    private void GameUpdate()
    {
        SBU.turn++;
        UpdateGUI();
    }




    //Activated from buttons ingame
    public void TakeCard()
    {



        GameUpdate();
    }

    public void PutCard()
    {

        GameUpdate();
    }

    //ran every input field is deselected
    public void UpdateString(string s)
    {
        inputString = s;
    }


    private void UpdateGUI()
    {

    }

}
