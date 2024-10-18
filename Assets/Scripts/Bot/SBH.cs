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

        int[] value = new int[playercards1.Length];
        int[] temp = null;
        List<int[]> possibleMoves = new List<int[]>(); //SBU.GetPossibleMoves(1, g.cards);

        int currentScore = 0;

        for (int i = 0; i < playercards1.Length; i++)
        {
            if (playercards1[i] != -10)
            {
                value[i] = SBU.getCurrentCardValue(SBU.getValueOfCard(SBU.cards, playercards1[i]));
                Debug.Log(value[i]);
            }
            else
            {
                break;
            }

            //Kollar om n�stkommande v�rden i handen �r 1 st�rre �n det f�rsta och sedan det efter det �r 2 st�rre osv. Checkar efter �kande stege
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

            ////Kollar om n�stkommande v�rden i handen �r 1 mindre �n det f�rsta och sedan om det efter det �r 2 mindre osv. Checkar efter minskande stege
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

            ////Kollar om n�stkommande v�rden �r lika med det f�rsta. Kollar efter 
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

            for (int j = 0; j < 12; j++)
            {
                //int[] moveValue = SBU.MoveValue(playercards1, temp);
                
            }
        }
        
        return currentScore;
    }


}
