using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;



/// <summary>
/// GameBase, where the actuall game runs, executing the SBU functions and interacting with the unity editor
/// </summary>
public class GameBase : MonoBehaviour
{

    [SerializeField] Settings settings;

    private GUIManager guiManager;

    //Used for UI
    [SerializeField] TextMeshProUGUI[] pText = new TextMeshProUGUI[5];
    public TextMeshProUGUI infoText;

    private List<Move> moveHistory;
    private int moveHistoryPointer;

    private bool gameOver;

    private SBTimer gameTimer;

    void Start()
    {
        gameOver = false;
        moveHistory = new List<Move>();
        moveHistoryPointer = moveHistory.Count - 1;

        TranspositionTable.GenerateZobristHashKey();

        SBU.gameState = new GameState(new int[44], 1, 0);

        //Maps all card indexes (0 - 44) to their actual values
        SBU.CreateCardValues();

        DistributeCards(settings);

        SBU.round = 0;

        guiManager = GetComponent<GUIManager>();

        gameTimer = new SBTimer();
        gameTimer.StartTimer();

        GameUpdate();

    }




    public static void DistributeCards(Settings settings)
    {

        if (settings.GameSeed == 0) { settings.GameSeed = UnityEngine.Random.Range(0, 1000000000); }

        UnityEngine.Random.InitState((int)settings.GameSeed);

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

        if (SBU.gameState.turn == 1) { SBU.round++; }

        UpdateGUI();

    }

    public void BotMove(bool logSearch = true)
    {
        SBA search = new SBA(
            SBU.gameState,
            settings.MaxSearchDepth,
            SBU.gameState.turn,
            settings.FearBias
        );

        SBTimer timer = new SBTimer();
        timer.StartTimer();
        search.StartSearch();


        if (logSearch) { 
            Debug.Log(
            "Searched Positions: " + search.searchedPositions + 
            " \n Time Elapsed: " + MathF.Round(timer.Timer(), 3) + 
            "   ||   Evaluation Speed: " + MathF.Round(search.searchedPositions / (timer.Timer() * 1000)) + "kN/s"
            ); 
        }
        SBU.gameState.DoMove(search.bestMove);

        //Debug.Log(search.bestEval);

        //Store moves
        if (moveHistoryPointer > -1) { moveHistory.RemoveRange(moveHistoryPointer + 1, moveHistory.Count - moveHistoryPointer - 1); }
        moveHistory.Add(search.bestMove);
        moveHistoryPointer += 1;

        GameUpdate();
    }


    public void UndoMove()
    {
        if (moveHistoryPointer < 0) { Debug.Log("No more to moves to undo"); return; }

        gameOver = false;

        SBU.gameState.UndoMove(moveHistory[moveHistoryPointer]);
        moveHistoryPointer -= 1;


        GameUpdate();
    }

    public void RedoMove()
    {
        if (moveHistoryPointer + 1 >= moveHistory.Count) { Debug.Log("No more moves to undo"); return; }
        moveHistoryPointer += 1;
        SBU.gameState.DoMove(moveHistory[moveHistoryPointer]);
    }

    public void SimulateGame()
    {
        if (gameOver) { Start(); }
        while (!gameOver)
        {
            BotMove(false);
        }
    }


    //Activated from buttons ingame
    public void TakeCard(bool flipped)
    {
        bool top = SBU.getCardHandIndex(SBU.gameState.cards[int.Parse(guiManager.selectedCards.Where(c => SBU.getCardOwner(SBU.gameState.cards[int.Parse(c.name)]) == 0).First().name)]) != 0;

        int heldCard = guiManager.selectedCards
            .Select(c => int.Parse(c.name))
            .Where(c => SBU.getCardOwner(SBU.gameState.cards[c]) != 0)
            .DefaultIfEmpty(-1)
            .First();

        int handIndex = heldCard == -1 ? 0 : SBU.getCardHandIndex(SBU.gameState.cards[heldCard]) + 1;

        Move m = new Move(SBU.gameState.cards, top, flipped, SBU.gameState.turn, handIndex);
        if (m != null)
        {
            SBU.gameState.DoMove(m);
            moveHistory.Add(m);
            moveHistoryPointer += 1;
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
        guiManager.selectedCardIndexes.Sort((a, b) => GameObject.Find(a.ToString()).transform.GetSiblingIndex().CompareTo(GameObject.Find(b.ToString()).transform.GetSiblingIndex()));

        Move m = new Move(SBU.gameState, guiManager.selectedCardIndexes.ToArray());

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


        SBU.gameState.DoMove(m);
        moveHistory.Add(m);
        moveHistoryPointer += 1;

        
        GameUpdate();

    }


    private void UpdateGUI()
    {
        guiManager.DeleteCards();
        guiManager.CreateCards();

        for (int i = 0; i < 5; i++)
        {
            string s;
            if (i > 0) { s = "Player " + i + " (" + SBU.gameState.getPlayerPoints(i) + ")"; }
            else { s = "Table Pile: "; }


            pText[i].text = s;
        }


        infoText.text = "Turn: " + SBU.gameState.turn + "   |   Round: " + SBU.round;

    }

    private void GameEnd()
    {
        gameOver = true;
        for (int i = 0; i < 4; i++)
        {
            SBU.gameState.playerPoints[i] -= SBU.gameState.getPlayerCards(i + 1).ArrayLength();
        }
        UpdateGUI();
        float time = MathF.Round(gameTimer.Timer(), 3);
        Debug.Log("GAME OVER, PLAYER " + SBU.gameState.getWinningPlayer() + " WON!    |    Total Time Elapsed: " + time);

    }


    public void StoreGame()
    {
       Statistics.StoreData(new string[4], moveHistory, SBU.gameState.getWinningPlayer(), SBU.gameState, settings.GameSeed, MathF.Round(gameTimer.Timer(), 3), "", gameOver);
    }

}
