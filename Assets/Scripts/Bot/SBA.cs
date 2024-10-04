using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Scout Bot Algorithm.
/// Plays out as many positions as possible and uses SBH to evaluate them.
/// </summary>
public static class SBA
{

    public static int DepthSearch(GameState g, int depth)
    {
        if(depth == 0)
        {
            return SBH.Evaluate(g);
        }




        return 0;
    }


}

public struct GameState
{
    public int[] cards;

    public int turn;

    public int currentPileHolder;

    public GameState(int[] cards, int turn, int currentPileHolder)
    {
        this.cards = cards;
        this.turn = turn;
        this.currentPileHolder = currentPileHolder;
    }

}
