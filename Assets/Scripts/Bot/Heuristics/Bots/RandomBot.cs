using System.Collections;
using System.Collections.Generic;

public class RandomBot : SBH
{
    public override int Evaluate(GameState g, int maximizer)
    {
        return UnityEngine.Random.Range(-10000, 10000);
    }

    public override void MoveOrdering(GameState g, List<Move> moves, Move priorityMove){}

}