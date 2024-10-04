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
        int[] playercards = SBU.getPlayerCards(g.cards, 1);
        int[] value = new int[playercards.Length];
        for (int i = 0; i < playercards.Length; i++)
        {
            value[i] = SBU.getCurrentCardValue(playercards[i]);
            Debug.Log(value[i].ToString());
        }

        //for (int i = 0; i < playercards.Length; i++)
        //{
        //    playercards[];
        //    SBU.getCardHandIndex;
        //}
        return 0;
    }


}
