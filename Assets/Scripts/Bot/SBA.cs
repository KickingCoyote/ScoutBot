using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
/// <summary>
/// Scout Bot Algorithm.
/// Plays out as many positions as possible and uses SBH to evaluate them.
/// </summary>
public class SBA
{

    public Move bestMove;

    private float fearBias;

    public int searchedPositions = 0;

    private GameState g;

    private int maximizer;

    private int maxDepth;


    public SBA(GameState g, int maxDepth, int maximizer, float fearBias) 
    {
        this.g = g;
        this.maxDepth = maxDepth;
        this.fearBias = fearBias;
        this.maximizer = maximizer;
        bestMove = null;
    }


    public void StartSearch()
    {
        DepthSearch(maxDepth, -2147483647, 2147483647);

        //for (int depth = 1; depth <= maxDepth; depth++)
        //{

        //    DepthSearch(depth, -2147483647, 2147483647);
        //}

    }



    //Paranoid MIN MAX Algorithm 
    public int DepthSearch(int depth, int alpha, int beta)
    {

        //Starts of being infinitely terrible for the current player
        int p = (g.turn == maximizer ? 1 : -1) * -2147483647;


        //Checks if it is game over, if the current player is winning return infinity otherwise -infinity as there is nothing better/worse than winning/losing the game.
        //This does not follow the paranoid algorithm as to get reasonable result you must presume that even the minimizers have some sense of self-preservation.
        //Its technically infinity -1 cause losing is still better than not finding a move and casting an error.
        if (g.isGameOver())
        {
            searchedPositions++;
            return g.getWinningPlayer() == g.turn ? 2147483646 : -2147483646;
        }


        //When at the wanted depth return the evalutation of the position
        if (depth == 0)
        {
            searchedPositions++;
            return SBH.Evaluate(g, maximizer);
        }


        List<Move> moves = Move.GetPossibleMoves(g, g.turn);
        
        moves.AddRange(Move.getPossibleDrawCardMoves(g.cards, g.turn));

        //Due to alpha beta pruning working better when the good moves are searched first we estimate how good a move is and then sort them based on that
        //22/11/2024: NPS: approx same, Positions Searched: halved.
        Move priorityMove = depth == maxDepth ? this.bestMove : null;
        MoveOrdering(g, moves, priorityMove);


        if (moves.Count == 0) { Debug.Log("NO POSSIBLE MOVES AT DEPTH: " + depth); return 0; }

        Move bestMove = new Move();

        if (g.turn == maximizer)
        {
            foreach (Move move in moves)
            {
                g.DoMove(move);

                //For each move search deeper and see how good the position is
                int eval = DepthSearch(depth - 1, alpha, beta);


                // > and not >= cause if we assume move ordering puts good moves first it should always pick the first one if they are equal to the evaluation
                if (eval > p) { bestMove = move; }
                p = Math.Max(p, eval);

                g.UndoMove(move);


                //alpha beta pruning
                alpha = Math.Max(alpha, p);
                if (beta <= alpha) { break; }
            }
        }
        else //The same thing but for minimizers so some > become < and Max() becomes Min() 
        {
            foreach (Move move in moves)
            {

                g.DoMove(move);

                int eval = DepthSearch(depth - 1, alpha, beta);


                if (eval < p) { bestMove = move; }
                p = Math.Min(p, eval);

                g.UndoMove(move);

                beta = Math.Min(beta, p);
                if (beta <= alpha) { break; }

            }
        }

        this.bestMove = bestMove;

        return p;
    }


    private void MoveOrdering(GameState g, List<Move> moves, Move priorityMove)
    {
        //the move ordering assumes that moves where more cards are put down are better and that picking up cards generally is worse
        foreach (Move move in moves)
        {
            if (priorityMove is not null && move.CompareMoves(priorityMove)) { move.scoreEstimate = 10000; }
            if (!move.isDrawMove) { move.scoreEstimate = move.moveLength * 100 + move.moveMin; }
            else { move.scoreEstimate = 0; }
        }

        moves.Sort();

    }

}


