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
    private int currentMaxDepth;

    public int bestEval;

    private int transpositionCounter = 0;

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
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            currentMaxDepth = depth;
            DepthSearch(depth, -2147483647, 2147483647);
        }

        Debug.Log($"Transpositions used: {transpositionCounter}  | Transpositions stored: {TranspositionTable.table.Count()}");
        TranspositionTable.table.Clear();
    }



    //Paranoid MIN MAX Algorithm 
    public int DepthSearch(int depth, int alpha, int beta)
    {

        int inv = g.turn == maximizer ? 1 : -1;

        //Checks if it is game over, if the current player is winning return infinity otherwise -infinity as there is nothing better/worse than winning/losing the game.
        //This does not follow the paranoid algorithm as to get reasonable result you must presume that even the minimizers have some sense of self-preservation.
        //Its technically infinity -1 cause losing is still better than not finding a move and casting an error.
        if (g.isGameOver())
        {
            searchedPositions++;
            return g.getWinningPlayer() == g.turn ? 2147483646 * inv : -2147483646 * inv;
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

        Move priorityMove = this.bestMove; //depth == currentMaxDepth ? this.bestMove : null; //Even if I wanted to I could not tell you why removing the depth check reduces searched positions but it does...
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

                g.UndoMove(move);




                // > and not >= cause if we assume move ordering puts good moves first it should always pick the first one if they are equal to the evaluation
                if (eval > alpha) 
                { 
                    bestMove = move;
                    alpha = eval;
                }


                //alpha beta pruning. 
                if (alpha >= beta) { break; }
            }
        }
        else //The same thing but for minimizers so some > become <
        {
            foreach (Move move in moves)
            {

                g.DoMove(move);
                int eval = DepthSearch(depth - 1, alpha, beta);

                g.UndoMove(move);


                if (eval < beta) 
                { 
                    bestMove = move;
                    beta = eval;
                }

                if (alpha >= beta) { break; }


            }
        }

        this.bestMove = bestMove;
        bestEval = maximizer == g.turn ? alpha : beta;
        return maximizer == g.turn ? alpha : beta;
    }

    /// <summary>
    /// For each position if the position already is stored in the Transposition Table use it, otherwise continue depth search and store the end result in the table.
    /// </summary>
    public int TranspositionSearch(GameState g, int depth, int alpha, int beta)
    {

        Transposition transposition = TranspositionTable.TryGetTransposition(g);
        int eval;
        if (!transposition.isEmpty && depth == transposition.depth)
        {
            transpositionCounter++;
            //eval = transposition.evaluation;
        }
        else
        {
            eval = DepthSearch(depth - 1, alpha, beta);
            TranspositionTable.AddTransposition(g, eval, depth, currentMaxDepth - depth);
        }

        return 1;
    }


    private void MoveOrdering(GameState g, List<Move> moves, Move priorityMove)
    {
        //the move ordering assumes that moves where more cards are put down are better and that picking up cards generally is worse
        foreach (Move move in moves)
        {
            if (priorityMove?.cardDif is not null && move.CompareMoves(priorityMove)) { move.scoreEstimate = 100000; continue; }

            if (!move.isDrawMove) { move.scoreEstimate = move.moveLength * 100 + move.moveMin; continue; }

            move.scoreEstimate = 0;
        }

        moves.Sort();

    }

}


