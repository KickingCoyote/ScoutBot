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
    public static int Evaluate(GameState g, int maximizer)
    {

        return 3 * g.getPlayerPoints(maximizer) - g.getPlayerPoints((maximizer % 4) + 1) - g.getPlayerPoints(((maximizer + 1) % 4) + 1) - g.getPlayerPoints(((maximizer + 2) % 4) + 1);


        //int currentScore = 0;


        //for (int i = 1; i < 5; i++)
        //{
        //    int inverter = maximizer == i ? 3 : -1;

        //    List<Move> possibleComputerMoves = Move.GetPossibleMoves(g, i);

        //    for (int j = 0; j < possibleComputerMoves.Count; j++)
        //    {
        //        currentScore += inverter * possibleComputerMoves[j].getValue(g.cards);
        //    }
        //}

        //return currentScore;
    }
    

}
//If the function returns a value greater than zero, that generally means the player has the best position