using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogStackV1 : SBH
{
    [SerializeField] int bias;

    public override int Evaluate(GameState g, int maximizer)
    {

        int eval = 3 * g.EstimatePossibleMoveScore(maximizer); ;
        eval += 3 * bias * g.getPlayerPoints(maximizer);

        for (int i = 1; i < 5; i++)
        {
            if (i == maximizer) { continue; }

            eval -= bias * g.getPlayerPoints(i);
            eval -= g.EstimatePossibleMoveScore(i);
        }

        return eval;
    }
}
