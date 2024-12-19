using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeTree : SBH
{
    [SerializeField] int bias;
    [SerializeField] int paranoia;
    public override int Evaluate(GameState g, int maximizer)
    {
        int cardsOutsideGame = 0;
        for (int i = 0; i < g.cards.Length; i++)
        {
            if (SBU.getCardHandIndex(g.cards[i]) == 15)
            {
                cardsOutsideGame++;
            }
        }

        int newBias = bias;

        //if (cardsOutsideGame >= 12)
        //{
        //    newBias += 1000;
        //}

        int handValue = 3 * g.EstimateHandValueBeeTree(maximizer);
        int points = 3 * newBias * g.getPlayerPoints(maximizer);
        points = 0;
        //Debug.Log("Handvalue " + handValue);
        //Debug.Log(" Points " + points);
        int eval = handValue + points;





        for (int i = 1; i < 5; i++)
        {
            if (i == maximizer) { continue; }

            //eval -= paranoia * newBias * g.getPlayerPoints(i);
            eval -= paranoia * g.EstimateHandValueBeeTree(i);
        }
        return eval;
    }
}
