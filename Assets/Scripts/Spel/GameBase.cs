using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



/// <summary>
/// GameBase, where the actuall game runs, executing the SBU functions and interacting with the unity editor
/// </summary>
public class GameBase : MonoBehaviour
{

    private string inputString;



    //Used for UI
    [SerializeField] TextMeshProUGUI[] pText = new TextMeshProUGUI[5];
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] int[] cards;

    void Start()
    {

        //Maps all card indexes (0 - 44) to their actual values
        SBU.CreateCardValues();

        DistributeCards();

        GameUpdate();

    }





    private void DistributeCards()
    {
        for (int i = 0; i < 44; i++)
        {
            SBU.gameState.cards[i] = i % 4 + 1 + 8 * Random.Range(0, 2) + 16 * (i / 4);
        }
        SBU.gameState.cards = SBU.ShuffleCards(SBU.gameState.cards);

    }


    private void GameUpdate()
    {

        //Turn is a number between 1 and 4 dictating whose turn it is
        if (SBU.gameState.turn == 4) { SBU.gameState.turn = 1; }
        else { SBU.gameState.turn++; }


        cards = SBU.gameState.cards;


        if (SBU.CheckGameOver(new GameState(SBU.gameState.cards, SBU.gameState.turn, SBU.gameState.currentPileHolder)))
        {
            GameEnd();
        }

        if(SBU.gameState.turn == 1)
        {
            SBA search = new SBA();

            search.DepthSearch(SBU.gameState, 3);

            SBU.gameState.Move(search.bestMove);
            
        }
        UpdateGUI();


    }




    //Activated from buttons ingame
    public void TakeCard()
    {
        string[] s = inputString.Split(' ');



        int[] m = SBU.GenerateDrawCardMove(SBU.gameState.cards, bool.Parse(s[0]), bool.Parse(s[1]), SBU.gameState.turn, int.Parse(s[2]));

        if (m != null)
        {
            SBU.gameState.cards = SBU.CopyArray(SBU.AddArray(SBU.gameState.cards, m, false));
        }
        else
        {
            Debug.Log("Invalid Move, Cannot take that card");
            return;
        }
        GameUpdate();

    }

    public void PutCard()
    {

        string[] s = inputString.Split(' ');


        //the inputed cards (indexes)
        int[] move = new int[s.Length];

        for (int i = 0; i < s.Length; i++)
        {
            move[i] = SBU.CardFromString(SBU.gameState.cards, s[i]);
        }


        int[] m = SBU.GenerateMove(SBU.gameState.cards, move, SBU.gameState.turn);


        //Check if its a legal move, This can be made faster by not converting them to int[44]s before comparison
        if (SBU.ContainsArray(SBU.GetPossibleMoves(SBU.gameState.turn, SBU.gameState.cards), m))
        {
            SBU.gameState.cards = SBU.CopyArray(SBU.AddArray(SBU.gameState.cards, m, false));
        }
        else
        {
            Debug.Log("Invalid Move, Cannot put down those cards");
            return;
        }

        SBU.gameState.currentPileHolder = SBU.gameState.turn;
        GameUpdate();
    }

    //ran every input field is deselected
    public void UpdateString(string s)
    {
        inputString = s;
    }


    private void UpdateGUI()
    {
        for (int i = 0; i < 5; i++)
        {
            string s;
            if (i > 0) { s = "Player " + i + " (" + SBU.getPlayerScore(SBU.gameState.cards, i) + ") : "; }
            else { s = "Table Pile: "; }

            int[] playerCards = SBU.getPlayerCards(SBU.gameState.cards, i);
            for (int j = 0; j < playerCards.Length; j++)
            {

                if (playerCards[j] == -10) { break; }


                s += SBU.CardToString(SBU.gameState.cards, playerCards[j]);
                s += " ";

            }

            pText[i].text = s;
        }


        infoText.text = "Turn: " + SBU.gameState.turn;

    }

    private void GameEnd()
    {
        Debug.Log("GAME OVER, PLAYER " + SBU.getWinningPlayer(SBU.gameState.cards) + " WON!");
    }

}
