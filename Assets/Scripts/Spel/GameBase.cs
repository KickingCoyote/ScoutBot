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
        //for debuging
        cards = SBU.gameState.cards;

        if (SBU.CheckGameOver(SBU.gameState))
        {
            GameEnd();
        }

        if(SBU.gameState.turn == 1)
        {
            BotMove();
        }

        UpdateGUI();


    }

    private void BotMove()
    {
        SBA search = new SBA(2);

        search.DepthSearch(SBU.gameState, 3);
        Debug.Log("Searched Positions: " + search.searchedPositions);
        SBU.gameState.Move(search.bestMove);
    }


    //Activated from buttons ingame
    public void TakeCard()
    {
        string[] s = inputString.Split(' ');



        Move m = new Move(SBU.gameState.cards, bool.Parse(s[0]), bool.Parse(s[1]), SBU.gameState.turn, int.Parse(s[2]));

        if (m != null)
        {
            SBU.gameState.Move(m);
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


        Move m = new Move(SBU.gameState, move);


        //Check if its a legal move, This can be made faster by not converting them to int[44]s before comparison
        //THIS DOES NOT WORK DUE TO CONTAIN COMPARING BY REF
        //if (SBU.GetPossibleMoves(SBU.gameState.turn, SBU.gameState.cards).Contains(m))
        //{
        //  SBU.gameState.cards = SBU.CopyArray(ArrayExtensions.AddArray(SBU.gameState.cards, m.cardDif));
        //}
        //else
        //{
        //    Debug.Log("Invalid Move, Cannot put down those cards");
        //    return;
        //}


        SBU.gameState.Move(m);

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

            int[] playerCards = GameState.getPlayerCards(SBU.gameState.cards, i);
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
