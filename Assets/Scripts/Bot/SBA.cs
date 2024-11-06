using System;
using System.Collections.Generic;
using System.Linq;
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

    public SBA(GameState g, int maximizer, float fearBias) 
    {
        this.g = g;
        this.fearBias = fearBias;
        this.maximizer = maximizer;
    }

    //Paranoid MIN MAX Algorithm 
    public int DepthSearch(int depth, int alpha, int beta)
    {

        int inverter = g.turn == maximizer ? 1 : -1;


        //Starts of being infinitely terrible for the current player
        int p =  inverter * -2147483647;


        //Checks if it is game over, if the current player is winning return (-- = +) infinity otherwise -infinity as there is nothing better/worse than winning/losing the game
        if (g.isGameOver())
        {
            searchedPositions++;
            return g.getWinningPlayer() == maximizer ? 2147483647 : -2147483647;
        }


        //When at the wanted depth return the evalutation of the position
        if (depth == 0)
        {
            searchedPositions++;
            return SBH.Evaluate(g, maximizer);
        }


        List<Move> moves = Move.GetPossibleMoves(g, g.turn);
        
        moves.AddRange(Move.getPossibleDrawCardMoves(g.cards, g.turn));

        //Current move ordering decreases NPS by 25% and doubles amount of searched positions.
        //MoveOrdering(moves);

        if (moves.Count == 0) { Debug.Log("NO POSSIBLE MOVES AT DEPTH: " + depth); }

        Move bestMove = new Move();

        if (g.turn == maximizer)
        {
            foreach (Move move in moves)
            {

                g.Move(move);

                //For each move search deeper and see how good the position is
                int eval = DepthSearch(depth - 1, alpha, beta);


                if (eval >= p) { bestMove = move; }
                p = Math.Max(p, eval);

                g.UndoMove(move);

                alpha = Math.Max(alpha, p);
                if (beta <= alpha) { break; }
            }
        }
        else
        {
            foreach (Move move in moves)
            {

                g.Move(move);

                //For each move search deeper and see how good the position is
                int eval = DepthSearch(depth - 1, alpha, beta);


                if (eval <= p) { bestMove = move; }
                p = Math.Min(p, eval);

                g.UndoMove(move);

                beta = Math.Min(beta, p);
                if (beta <= alpha) { break; }

            }
        }

        this.bestMove = bestMove;
        
        return p;
    }


    private void MoveOrdering(List<Move> moves)
    {
        
        foreach(Move move in moves)
        {
            if (!move.isDrawMove)
            {
                move.scoreEstimate = 100;
            }
            else
            {
                //if the neighbouring cards are not ladder / match asume its bad
            }
        }

        moves.Sort();
    }


}




public struct GameState
{
    public int[] cards;

    public int turn;

    public int currentPileHolder;

    private int[][] playerCards;

    public int[] playerPoints;

    /// <summary>
    /// When checking player cards for the first time store them in playerCards and on cosecutive checks retreive the data from the variable
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public int[] getPlayerCards(int player) { return playerCards[player] = playerCards[player] == null ? getPlayerCards(cards, player) : playerCards[player]; }

    public int getPlayerPoints(int player) { return playerPoints[player - 1]; }

    public GameState(int[] cards, int turn, int currentPileHolder)
    {
        this.cards = cards;
        this.turn = turn;
        this.currentPileHolder = currentPileHolder;
        playerCards = new int[][] { null, null, null, null, null};
        playerPoints = new int[4];
    }


    public void Move(Move move)
    {
        if (!move.isDrawMove) { playerPoints[turn - 1] += getPlayerCards(cards, 0).ArrayLength(); }
        else {  playerPoints[currentPileHolder - 1]++; }

        cards = ArrayExtensions.AddArray(cards, move.cardDif);
        currentPileHolder += move.pileHolderDif;

        //Reset the playerCards data for the current player and middle pile
        playerCards = new int[][] { null, null, null, null, null };

        //Increment the turn by 1, if 4 set to 1
        turn = turn == 4 ? 1 : (turn + 1);
    }

    public void UndoMove(Move move)

    {
        cards = ArrayExtensions.AddArray(cards, move.cardDif, true);
        currentPileHolder -= move.pileHolderDif;
        playerCards = new int[][] { null, null, null, null, null };

        turn = turn == 1 ? 4 : (turn - 1);

        if (!move.isDrawMove) { playerPoints[turn - 1] -= getPlayerCards(cards, 0).ArrayLength(); }
        else { playerPoints[currentPileHolder - 1]--; }

    }


    /// <summary>
    /// Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="player"></param>
    /// <returns>A 15 long array of card indexes where emtpy values are -10</returns>
    public static int[] getPlayerCards(int[] cards, int player)
    {
        int[] pCards = new int[15].SetArray(-10);

        for (int i = 0; i < cards.Length; i++)
        {
            //HandIndex 15 represents the card being a point and not actually in the hand
            if (SBU.getCardOwner(cards[i]) == player && SBU.getCardHandIndex(cards[i]) != 15)
            {
                pCards[SBU.getCardHandIndex(cards[i])] = i;
            }

        }

        return pCards;
    }

    public int getWinningPlayer()
    {
        return Array.IndexOf(playerPoints, Enumerable.Max(playerPoints)) + 1;
    }



    /// <summary>
    /// Function that checks if it is game over or not
    /// </summary>
    public bool isGameOver()
    {
        if (currentPileHolder == turn)
        {
            return true;
        }
        //check previous player due to game over being check post turn incrementing
        if (getPlayerCards(turn == 1 ? 4 : (turn - 1)).ArrayLength() == 0)
        {
            return true;
        }

        return false;
    }

}
