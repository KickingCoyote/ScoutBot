using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// GameBase, where the actuall game runs, executing the SBU functions and interacting with the unity editor
/// </summary>
public class GameBase : MonoBehaviour
{

    [SerializeField] Settings settings;

    private GUIManager guiManager;

    //Used for UI
    [SerializeField] TextMeshProUGUI[] pText = new TextMeshProUGUI[5];
    [SerializeField] Toggle flipTakenCardToggle;
    [SerializeField] TextMeshProUGUI saveMenuInputField;
    public TextMeshProUGUI infoText;

    [SerializeField] SBH defaultHeuristic;


    private List<Move> moveHistory;
    private int moveHistoryPointer;

    private bool gameOver;
    [SerializeField] bool randomSeed = false;
    private int startingPlayer = 1;

    private SBTimer gameTimer;

    void Start()
    {
        StartGame(true);
    }
    void StartGame(bool randomizeSeed)
    {
        gameOver = false;
        moveHistory = new List<Move>();
        moveHistoryPointer = moveHistory.Count - 1;

        TranspositionTable.GenerateZobristHashKey();

        SBU.gameState = new GameState(new int[44], startingPlayer, 0);

        //Maps all card indexes (0 - 44) to their actual values
        SBU.CreateCardValues();

        DistributeCards(settings, randomizeSeed);

        guiManager = GetComponent<GUIManager>();

        gameTimer = new SBTimer();
        gameTimer.StartTimer();

        GameUpdate();

    }




    public void DistributeCards(Settings settings, bool randomizeSeed)
    {

        if (randomSeed && randomizeSeed) { settings.GameSeed = UnityEngine.Random.Range(0, 1000000000); }

        UnityEngine.Random.InitState(settings.GameSeed);

        for (int i = 0; i < 44; i++)
        {
            SBU.gameState.cards[i] = i % 4 + 1 + 8 * UnityEngine.Random.Range(0, 2) + 16 * (i / 4);
        }
        SBU.gameState.cards = SBU.ShuffleCards(SBU.gameState.cards);

    }


    private void GameUpdate(bool doGUIUpdate = true)
    {

        if (SBU.gameState.isGameOver())
        {
            GameEnd();
            return;
        }


        if (doGUIUpdate)
        {
            UpdateGUI();
        }

    }

    public void BotMove(bool logSearch = true, int botOwnerIncrement = 0)
    {

        //Sets all undecided bots to FrogStackV1
        if (settings.Heuristics.GetHeuristic(((SBU.gameState.turn - 1 + botOwnerIncrement) % 4) + 1) is null) { settings.Heuristics.SetHeuristic(SBU.gameState.turn, defaultHeuristic); }

        SBA search = new SBA(
            SBU.gameState,
            settings.MaxSearchDepth,
            SBU.gameState.turn,
            settings.Heuristics.GetHeuristic(((SBU.gameState.turn - 1 + botOwnerIncrement) % 4) + 1)
        );

        search.StartSearch();


        if (logSearch) { 
            Debug.Log(
            "Searched Positions: " + search.searchedPositions + 
            " \n Time Elapsed: " + MathF.Round(search.timer.Timer(), 3) + 
            "   ||   Evaluation Speed: " + MathF.Round(search.searchedPositions / (search.timer.Timer() * 1000)) + "kN/s"
            ); 
        }
        SBU.gameState.DoMove(search.bestMove);

        //Store moves
        if (moveHistoryPointer > -1) { moveHistory.RemoveRange(moveHistoryPointer + 1, moveHistory.Count - moveHistoryPointer - 1); }
        moveHistory.Add(search.bestMove);
        moveHistoryPointer += 1;

        GameUpdate(logSearch);
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

    /* Simulates many games of scout letting the bots play against eachother
     * 
     * Every fourth game the seed changes.
     * Who plays as who changes with each round so everybody gets to play all hands before the seed switches.
     */
    public void SimulateGame()
    {
        int[] tally = new int[4];

        SBTimer timer = new SBTimer();
        timer.StartTimer();

        startingPlayer = 1;

        for (int i = 0; i < settings.SimulationCount; i++)
        {
            if (i%4 == 0) //Change seed every fourth game
            {
                StartGame(true);
            }
            else
            {
                StartGame(false);
            }

            while (!gameOver)
            {
                BotMove(false, i % 4);
            }

            tally[SBU.gameState.getWinningPlayer() - 1]++;
            //startingPlayer = startingPlayer == 4 ? 1 : (startingPlayer + 1);

        }
        Debug.Log(string.Join(" : ", tally) + " | " + timer.Timer());
    }


    //Activated from buttons ingame

    public void TakeCard()
    {
        bool top = SBU.getCardHandIndex(SBU.gameState.cards[int.Parse(guiManager.selectedCards.Where(c => SBU.getCardOwner(SBU.gameState.cards[int.Parse(c.name)]) == 0).First().name)]) != 0;

        int heldCard = guiManager.selectedCards
            .Select(c => int.Parse(c.name))
            .Where(c => SBU.getCardOwner(SBU.gameState.cards[c]) != 0)
            .DefaultIfEmpty(-1)
            .First();

        int handIndex = heldCard == -1 ? 0 : SBU.getCardHandIndex(SBU.gameState.cards[heldCard]) + 1;

        Move m = new Move(SBU.gameState.cards, top, flipTakenCardToggle.isOn, SBU.gameState.turn, handIndex);
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


        infoText.text = "Turn: " + SBU.gameState.turn + "   |   Round: " + SBU.gameState.round;

    }

    private void GameEnd()
    {
        gameOver = true;

        float time = MathF.Round(gameTimer.Timer(), 3);
        Debug.Log("GAME OVER, PLAYER " + SBU.gameState.getWinningPlayer() + " WON!    |    Total Time Elapsed: " + time);
        for (int i = 0; i < 4; i++)
        {
            SBU.gameState.playerPoints[i] -= SBU.gameState.getPlayerCards(i + 1).ArrayLength();
        }
        UpdateGUI();

    }


    public void StoreGame()
    {
       Statistics.StoreData(new string[4], moveHistory, SBU.gameState.getWinningPlayer(), SBU.gameState, settings.GameSeed, MathF.Round(gameTimer.Timer(), 3), saveMenuInputField.text, gameOver);
    }

}
