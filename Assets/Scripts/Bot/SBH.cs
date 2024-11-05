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

        List<Move> possibleMoves = Move.getAllLegalMoves(g, 1);

        int currentScore = 0;

        int computerScore = 0;
        for (int i = 2; i < 5; i++)
        {
            List<Move> possibleComputerMoves = Move.getAllLegalMoves(g, i);

            for (int j = 0; j < possibleComputerMoves.Count; j++)
            {
                computerScore += possibleComputerMoves[j].getValue(g.cards);
            }
        }

        for (int j = 0; j < possibleMoves.Count; j++)
        {
            currentScore = currentScore + possibleMoves[j].getValue(g.cards);
        }

        return currentScore - (computerScore / 3);
    }
    

}
//If the function returns a value greater than zero, that generally means the player has the best position