using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;



/// <summary>
/// GameBase, where the actuall game runs, executing the SBU functions and interacting with the unity editor
/// </summary>
public class GameBase : MonoBehaviour
{

    private string inputString;

    [SerializeField] Settings settings;

    //Used for UI
    [SerializeField] TextMeshProUGUI[] pText = new TextMeshProUGUI[5];
    [SerializeField] TextMeshProUGUI infoText;

    private List<Move> moveHistory;

    private int round;
    private bool gameOver;

    void Start()
    {
        gameOver = false;
        moveHistory = new List<Move>();

        SBU.gameState = new GameState(new int[44], 1, 0);

        //Maps all card indexes (0 - 44) to their actual values
        SBU.CreateCardValues();

        DistributeCards();

        round = 0;
        GameUpdate();

    }




    private void DistributeCards()
    {
        if (settings.GameSeed != 0) { UnityEngine.Random.InitState((int)settings.GameSeed); }

        for (int i = 0; i < 44; i++)
        {
            SBU.gameState.cards[i] = i % 4 + 1 + 8 * UnityEngine.Random.Range(0, 2) + 16 * (i / 4);
        }
        SBU.gameState.cards = SBU.ShuffleCards(SBU.gameState.cards);

    }


    private void GameUpdate()
    {
        
        if (SBU.gameState.isGameOver())
        {
            GameEnd();
            return;
        }

        if (SBU.gameState.turn == 1) { round++; }

        UpdateGUI();

    }

    public void BotMove()
    {
        SBA search = new SBA(
            SBU.gameState,
            SBU.gameState.turn,
            settings.FearBias
        );

        SBTimer timer = new SBTimer();
        timer.StartTimer();
        search.DepthSearch(settings.SearchDepth, -2147483647, 2147483647);
        

        Debug.Log("Searched Positions: " + search.searchedPositions + " \n Time Elapsed: " + MathF.Round(timer.Timer(), 3) + "   ||   Evaluation Speed: " + MathF.Round(search.searchedPositions / (timer.Timer() * 1000)) + "kN/s");
        SBU.gameState.Move(search.bestMove);
        moveHistory.Add(search.bestMove);

        GameUpdate();
    }


    public void UndoMove()
    {
        if (moveHistory.Count == 0) { Debug.Log("No more to moves to undo"); return; }

        gameOver = false;

        SBU.gameState.UndoMove(moveHistory.Last());
        moveHistory.Remove(moveHistory.Last());

        GameUpdate();
    }

    public void SimulateGame()
    {
        if (gameOver) { Start(); }
        while (!gameOver)
        {
            BotMove();
        }
    }

    //Activated from buttons ingame
    public void TakeCard()
    {
        string[] s = inputString.Split(' ');



        Move m = new Move(SBU.gameState.cards, bool.Parse(s[0]), bool.Parse(s[1]), SBU.gameState.turn, int.Parse(s[2]));
        if (m != null)
        {
            SBU.gameState.Move(m);
            moveHistory.Add(m);
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
        moveHistory.Add(m);

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
            if (i > 0) { s = "Player " + i + " (" + SBU.gameState.getPlayerPoints(i) + ") : "; }
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


        infoText.text = "Turn: " + SBU.gameState.turn + "   |   Round: " + round;

    }

    private void GameEnd()
    {
        gameOver = true;
        UpdateGUI();
        Debug.Log("GAME OVER, PLAYER " + SBU.gameState.getWinningPlayer() + " WON!");
    }

}
