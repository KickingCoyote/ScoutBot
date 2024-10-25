using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scout Bot Heuristics.
/// The code for calculating how beneficial a position is for the bot
/// using the assumptions from the paranoid algorithm
/// </summary>
public static class SBH
{

    //Takes in the state of the game and calculates its value
    public static int Evaluate(GameState g)
    {

        List<int[]> possibleMoves = SBU.GetPossibleMoves(1, g.cards);

        int currentScore = 0;

        for (int j = 0; j < possibleMoves.Count; j++)
        {
            currentScore = currentScore + SBU.MoveValue(g.cards, SBU.MoveIndexesFromMove(g.cards, possibleMoves[j]));
        }
        
        return currentScore;
    }
    

}
//If the function returns a value greater than zero, that generally means the player has the best position