using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Scout Bot Algorithm.
/// Plays out as many positions as possible and uses SBH to evaluate them.
/// </summary>
public class SBA
{

    public int[] bestMove = new int[44];


    public int DepthSearch(GameState g, int depth)
    {

        //Paranoid MIN MAX Algorithm 


        //When at the wanted depth return the evalutation of the position
        if (depth == 0)
        {
            return SBH.Evaluate(g);
        }


        //TODO: Fix him skipping his turn when having no possible moves
        //TODO: Fix him maybe acting weirdly on the first move

        List<int[]> moves = SBU.getAllLegalMoves(g.turn, g.cards);



        //Removed for bug testing
        moves.AddRange(SBU.getPossibleDrawCardMoves(g.cards, g.turn));

        if (moves.Count == 0)
        {
            Debug.Log("NO POSSIBLE MOVES AT DEPTH: " + depth);

        }

        //player 2, 3, 4 will be minimizers and therefore try to reduce the score, hence 2, 3, 4 return -1 while player 1 will be the maximizer and therefore return 1
        int inverter = g.turn == 1 ? 1 : -1;


        //Starts of being infinitely terrible for the current player
        int p = inverter * -2147483647;
        int[] bestMove = new int[44];

        foreach (int[] move in moves)
        {
            
            g.Move(move);

            //For each move search deeper and see how good the position is
            int eval = DepthSearch(g, depth - 1);

            //Each time if the position is better then p set p to eval
            //For the the maximizer it will always choose the highest possible value between eval and p, while the minimizer picks the minimal value
            if(inverter == 1) {
                if (p < eval) { bestMove = move; }

                p = Mathf.Max(p, eval); 
            }
            else { 
                if (p > eval) { bestMove = move; }


                p = Mathf.Min(p, eval); 
  
            }

            g.UndoMove(move);
        }

        this.bestMove = bestMove;
        
        return p;
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


    public void Move(int[] move)
    {
        int[] m = ArrayExtensions.AddArray(cards, move, false);
        ////if the table pile got larger or stayed the same size it means someone put down cards and currentPileHolder should change
        //if (SBU.getPlayerCards(cards, 0).Length <= SBU.getPlayerCards(m, 0).Length)
        //{
        //    currentPileHolder = turn;
        //}

        cards = SBU.CopyArray(m);
        turn = turn == 4 ? 1 : (turn + 1);
    }

    public void UndoMove(int[] move)
    {
        int[] previousPos = ArrayExtensions.AddArray(cards, move, true);

        cards = SBU.CopyArray(previousPos);
        turn = turn == 1 ? 4 : (turn - 1);

    }




}
