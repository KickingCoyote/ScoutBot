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

        int[] value1 = new int[playercards1.Length];
        int[] value2 = new int[playercards1.Length];
        int[] value3 = new int[playercards1.Length];
        int[] value4 = new int[playercards1.Length];

        List<int[]> possibleMoves = SBU.GetPossibleMoves(1, g.cards);
        int[] moveValues = new int[possibleMoves.Count];

        int currentScore = 0;
        int computereScore = 0;

        for (int i = 0; i < playercards1.Length; i++)
        {
            if (playercards1[i] != -10)
            {
                value1[i] = SBU.getCurrentCardValue(SBU.getValueOfCard(SBU.cards, playercards1[i]));
                //Debug.Log(value1[i]);
            }
            else
            {
                break;
            }

            if (playercards2[i] != -10)
            {
                value2[i] = SBU.getCurrentCardValue(SBU.getValueOfCard(SBU.cards, playercards2[i]));
                //Debug.Log(value2[i]);
            }
            else
            {
                break;
            }

            if (playercards3[i] != -10)
            {
                value3[i] = SBU.getCurrentCardValue(SBU.getValueOfCard(SBU.cards, playercards3[i]));
                //Debug.Log(value3[i]);
            }
            else
            {
                break;
            }

            if (playercards4[i] != -10)
            {
                value4[i] = SBU.getCurrentCardValue(SBU.getValueOfCard(SBU.cards, playercards4[i]));
                //Debug.Log(value4[i]);
            }
            else
            {
                break;
            }

            //Kollar om nästkommande värden i handen är 1 större än det första och sedan det efter det är 2 större osv. Checkar efter ökande stege
            //if (value[i] == value[i + 1] + 1)
            //{
            //    currentScore += 1;
            //}
            //else if (value[i] == value[i + 1] + 1 && value[i] == value[i + 2] + 2)
            //{
            //    currentScore += 2;
            //}
            //else if (value[i] == value[i + 1] + 1 && value[i] == value[i + 2] + 2 && value[i] == value[i + 3] + 3)
            //{
            //    currentScore += 2;
            //}

            ////Kollar om nästkommande värden i handen är 1 mindre än det första och sedan om det efter det är 2 mindre osv. Checkar efter minskande stege
            //else if (value[i] == value[i + 1] - 1)
            //{
            //    currentScore += 1;
            //}
            //else if (value[i] == value[i + 1] - 1 && value[i] == value[i + 2] - 2)
            //{
            //    currentScore += 2;
            //}
            //else if (value[i] == value[i + 1] + 1 && value[i] == value[i + 2] + 2 && value[i] == value[i + 3] + 3)
            //{
            //    currentScore += 2;
            //}

            ////Kollar om nästkommande värden är lika med det första. Kollar efter 
            //else if (value[i] == value[i + 1])
            //{
            //    currentScore += 2;
            //}
            //else if (value[i] == value[i + 1] && value[i] == value[i + 2])
            //{
            //    currentScore += 2;
            //}
            //else
            //{
            //    break;
            //}

        }
        for (int j = 0; j < possibleMoves.Count; j++)
        {
            moveValues[j] = SBU.MoveValue(playercards1, SBU.MoveIndexesFromMove(g.cards, possibleMoves[j]));
            currentScore = currentScore + moveValues[j];
            Debug.Log(currentScore);
        }
        
        return currentScore;
    }
    

}
//If the function returns a value greater than zero, that generally means the player has the best position