using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//If the function returns a value greater than zero, that generally means the player has the best position
/// <summary>
/// Scout Bot Heuristics.
/// The code for calculating how beneficial a position is for the bot
/// using the assumptions from the paranoid algorithm
/// </summary>
[System.Serializable]
public abstract class SBH : MonoBehaviour
{

    //Takes in the state of the game and calculates its value
    public abstract int Evaluate(GameState g, int maximizer);


    public virtual void MoveOrdering(GameState g, List<Move> moves, Move priorityMove)
    {
        //the move ordering assumes that moves where more cards are put down are better and that picking up cards generally is worse
        foreach (Move move in moves)
        {
            if (priorityMove?.cardDif is not null && move.CompareMoves(priorityMove)) { move.scoreEstimate = 100000; continue; }

            if (!move.isDrawMove) { move.scoreEstimate = move.moveLength * 100 + move.moveMin; continue; }

        }

        moves.Sort();
    }

}