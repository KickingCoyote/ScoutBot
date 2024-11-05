using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Scout Bot Algorithm.
/// Plays out as many positions as possible and uses SBH to evaluate them.
/// </summary>
public class SBA
{

    public Move bestMove;

    private float drawMoveTolerance;

    public int searchedPositions = 0;

    public SBA(float drawMoveTolerance) 
    {
        this.drawMoveTolerance = drawMoveTolerance;
    }


    public int DepthSearch(GameState g, int depth)
    {

        //Paranoid MIN MAX Algorithm 


        //When at the wanted depth return the evalutation of the position
        if (depth == 0)
        {
            searchedPositions++;
            return SBH.Evaluate(g);
        }


        //TODO: Fix him skipping his turn when having no possible moves
        List<Move> moves = Move.getAllLegalMoves(g, g.turn);
        
        moves.AddRange(Move.getPossibleDrawCardMoves(g.cards, g.turn));
        


        if (moves.Count == 0)
        {
            Debug.Log("NO POSSIBLE MOVES AT DEPTH: " + depth);

        }

        //player 2, 3, 4 will be minimizers and therefore try to reduce the score, hence 2, 3, 4 return -1 while player 1 will be the maximizer and therefore return 1
        int inverter = g.turn == 1 ? 1 : -1;


        //Starts of being infinitely terrible for the current player
        int p = inverter * -2147483647;
        Move bestMove = new Move();

        foreach (Move move in moves)
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


    /// <summary>
    /// Prunes the amount of moves drastically by removing all obvoulsy terrible draw card moves which massively increases preformance
    /// </summary>
    /// <param name="moves">All draw card moves</param>
    /// <param name="tolerance">Number from 0 to 1 What procent of moves</param>
    private void PruneDrawMoves(List<Move> moves, float? tolerance = null)
    {

    }


}




public struct GameState
{
    public int[] cards;

    public int turn;

    public int currentPileHolder;

    private int[][] playerCards;

    /// <summary>
    /// When checking player cards for the first time store them in playerCards and on cosecutive checks retreive the data from the variable
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public int[] getPlayerCards(int player) { return playerCards[player] = playerCards[player] == null ? getPlayerCards(cards, player) : playerCards[player]; }

    public GameState(int[] cards, int turn, int currentPileHolder)
    {
        this.cards = cards;
        this.turn = turn;
        this.currentPileHolder = currentPileHolder;
        playerCards = new int[][] { null, null, null, null, null};
    }


    public void Move(Move move)
    {
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
        currentPileHolder -= currentPileHolder;

        turn = turn == 1 ? 4 : (turn - 1);

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

}
