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
        int[] playercards1 = SBU.getPlayerCards(g.cards, 1);
        int[] playercards2 = SBU.getPlayerCards(g.cards, 2);
        int[] playercards3 = SBU.getPlayerCards(g.cards, 3);
        int[] playercards4 = SBU.getPlayerCards(g.cards, 4);

        //int[] value1 = new int[playercards1.Length];
        //int[] value2 = new int[playercards1.Length];
        //int[] value3 = new int[playercards1.Length];
        //int[] value4 = new int[playercards1.Length];

        List<int[]> possibleMoves1 = SBU.GetPossibleMoves(1, g.cards);
        List<int[]> possibleMoves2 = SBU.GetPossibleMoves(2, g.cards);
        List<int[]> possibleMoves3 = SBU.GetPossibleMoves(3, g.cards);
        List<int[]> possibleMoves4 = SBU.GetPossibleMoves(4, g.cards);
        //int[] moveValues = new int[possibleMoves.Count];

        int currentScore = 0;
        int computereScore = 0;

        for (int j = 0; j < possibleMoves1.Count; j++)
        {
            currentScore = currentScore + SBU.MoveValue(g.cards, SBU.MoveIndexesFromMove(g.cards, possibleMoves1[j]));
        }
        for (int k = 0; k < possibleMoves2.Count; k++)
        {
            computereScore = computereScore + SBU.MoveValue(g.cards, SBU.MoveIndexesFromMove(g.cards, possibleMoves2[k]));
        }
        for (int l = 0; l < possibleMoves3.Count; l++)
        {
            computereScore = computereScore + SBU.MoveValue(g.cards, SBU.MoveIndexesFromMove(g.cards, possibleMoves3[l]));
        }
        for (int m = 0; m < possibleMoves4.Count; m++)
        {
            computereScore = computereScore + SBU.MoveValue(g.cards, SBU.MoveIndexesFromMove(g.cards, possibleMoves4[m]));
        }
        computereScore = computereScore / 3;
        currentScore = currentScore - computereScore;

        return currentScore;
    }
    

}
//If the function returns a value greater than zero, that generally means the player has the best position