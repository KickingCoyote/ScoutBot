using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoatBoat : SBH
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
        
        int eval = 3 * g.EstimateHandValueGoatBoat(maximizer); ;
        Debug.Log("___");
        Debug.Log("HandValue: " + eval);
        eval += 3 * newBias * g.getPlayerPoints(maximizer);
        Debug.Log("PlayerPoints: " + g.getPlayerPoints(maximizer));




        for (int i = 1; i < 5; i++)
        {
            if (i == maximizer) { continue; }

            eval -= paranoia * newBias * g.getPlayerPoints(i);
            eval -= paranoia * g.EstimateHandValueGoatBoat(i);
        }
        return eval;
    }
}
