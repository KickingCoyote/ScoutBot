using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogStackV1 : SBH
{
    [SerializeField] int bias;
    [SerializeField] int paranoia;
    public override int Evaluate(GameState g, int maximizer)
    {

        int eval = 3 * g.EstimatePossibleMoveScore(maximizer); ;
        eval += 3 * bias * g.getPlayerPoints(maximizer);
        

        for (int i = 1; i < 5; i++)
        {
            if (i == maximizer) { continue; }

            eval -= paranoia * bias * g.getPlayerPoints(i); //Just for testing
            eval -= paranoia * g.EstimatePossibleMoveScore(i);
        }

        return eval;
    }
}
