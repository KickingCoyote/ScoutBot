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
            return SBH.Evaluate(g);
        }


        //TODO: Fix him skipping his turn when having no possible moves
        //TODO: Fix him maybe acting weirdly on the first move

        List<Move> moves = SBU.getAllLegalMoves(g, g.turn);

        moves.AddRange(SBU.getPossibleDrawCardMoves(g.cards, g.turn));

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
    public GameState(int[] cards, int turn, int currentPileHolder)
    {
        this.cards = cards;
        this.turn = turn;
        this.currentPileHolder = currentPileHolder;
    }


    public void Move(Move move)
    {
        cards = ArrayExtensions.AddArray(cards, move.cardDif);
        currentPileHolder += move.pileHolderDif;
        
        turn = turn == 4 ? 1 : (turn + 1);
    }

    public void UndoMove(Move move)
    {
        cards = ArrayExtensions.AddArray(cards, move.cardDif, true);
        currentPileHolder -= currentPileHolder;

        turn = turn == 1 ? 4 : (turn - 1);

    }

}

public class Move
{
    public int[] cardDif;

    public int pileHolderDif;

    public bool isDrawMove;

    public Move() { }
    public Move(int[] cardDif, int pileHolderDif, bool isDrawMove)
    {
        this.cardDif = cardDif;
        this.pileHolderDif = pileHolderDif;
        this.isDrawMove = isDrawMove;
    }

    /// <summary>
    /// Generates a move array from a set of cards (move) and a GameState used for putting down cards
    /// </summary>
    /// <param name="g">gameState</param>
    /// <param name="move">card indexes being put down</param>
    /// <param name="player">who is doing the move, if left empty will become g.turn</param>
    public Move(GameState g, int[] move, int? player = null)
    {
        if (player == null) { player = g.turn; }

        //List of all the players cards by index
        int[] pCards = SBU.getPlayerCards(g.cards, (int) player);

        //The cards on the table
        int[] tCards = SBU.getPlayerCards(g.cards, 0);

        int[] moveArray = g.cards.CopyArray();

        int k = 0;
        bool foundMove = false;
        for (int i = 0; i < tCards.Length; i++)
        {
            if (tCards[i] == -10) { break; }

            //Takes all cards currently in the pile and gives them as points to the player
            //(handIndex = 15 represents it being a point)
            moveArray[tCards[i]] = 16 * 15 + (int)player;
        }
        for (int i = 0; i < pCards.Length; i++)
        {
            if (pCards[i] == -10) { break; }

            if (k < move.Length && pCards[i] == move[k])
            {
                //Move card to center pile and set its new index in center pile
                moveArray[move[k]] = 16 * k + 8 * SBU.getCardFlip(g.cards[move[k]]);
                foundMove = true;
                k++;
            }
            //Reduce index by move.length;
            else if (foundMove && pCards[i] != -10) { moveArray[pCards[i]] = g.cards[pCards[i]] - 16 * move.Length; }
        }

        cardDif = ArrayExtensions.AddArray(moveArray, g.cards, true);
        pileHolderDif = (int) player - g.currentPileHolder;
        isDrawMove = false;
    }


    /// <summary>
    /// Generates a move for taking a card from the middle pile 
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="top">if its the bottom or top card of the middle pile that is taken</param>
    /// <param name="flip">whether to change the rotation of the picked up card</param>
    /// <param name="player">who the card is picked up by</param>
    /// <param name="handIndex">where in the hand the card is placed</param>
    /// <returns>int[44] with values that represent the gameState post move</returns>
    public Move(int[] cards, bool top, bool flip, int player, int handIndex)
    {
        //Gets the cards on the table
        int[] tCards = SBU.getPlayerCards(cards, 0);
        int tCard = -10;
        int[] pCards = SBU.getPlayerCards(cards, player);

        //if there are no cards on the table or the hand is full return 
        if (tCards.ArrayLength() == 0 || pCards[pCards.Length - 1] != -10) { return; }


        if (!top) { tCard = tCards[0]; }
        else
        {
            for (int i = 0; i < tCards.Length; i++)
            {
                if (i == tCards.Length - 1 || tCards[i + 1] == -10)
                {
                    tCard = tCards[i];
                    break;
                }
            }
        }

        //if no card is picked return
        if (tCard == -10) { Debug.Log("GenerateDrawCardMove: No Card Found"); return; }

        int[] move = cards.CopyArray();


        for (int i = pCards.Length - 1; i >= handIndex; i--)
        {
            //Shift all cards after the insertion point by 1 spot (aka 16)
            if (pCards[i] != -10) { move[pCards[i]] += 16; }
        }

        move[tCard] = 16 * handIndex + 8 * SBU.getCardFlip(cards[tCard]) + player;

        //Flip the card incase flip == true
        if (flip && SBU.getCardFlip(cards[tCard]) == 0) { move[tCard] += 8; }

        else if (flip) { move[tCard] -= 8; }


        //if the bottom card is taken, shift all cards on the table 1 spot to not leave the bottom spot empty
        if (!top)
        {
            for (int i = 1; i < tCards.Length; i++)
            {
                if (tCards[i] == -10) { break; }
                move[tCards[i]] -= 16;
            }
        }

        cardDif = ArrayExtensions.AddArray(move, cards, true);
        pileHolderDif = 0;
        isDrawMove = true;
    }


    public int getValue(int[] cards)
    {
        Debug.Log("NOT YET IMPLEMENTED");

        return -10;
    }

    public int[] retreiveMoveIndexes(int[] cards)
    {
        Debug.Log("NOT YET IMPLEMENTED");

        return new int[0];
    }

}