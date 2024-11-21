using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Scout Bot Heuristics.
/// The code for calculating how beneficial a position is for the bot
/// using the assumptions from the paranoid algorithm
/// </summary>
public static class SBH
{

    //Takes in the state of the game and calculates its value
    public static int Evaluate(GameState g, int maximizer)
    {

        int currentScore = 0;

        for (int i = 1; i < 5; i++)
        {
            if (i == maximizer) { continue; }

            int weight = 100;
            
            currentScore -= weight * g.getPlayerPoints(i);
            currentScore -= g.EstimatePossibleMoveScore(i) / 10;
        }

        currentScore += g.EstimatePossibleMoveScore(maximizer) / 10;

        return currentScore + 300 * g.getPlayerPoints(maximizer);
    }
    

}
//If the function returns a value greater than zero, that generally means the player has the best position