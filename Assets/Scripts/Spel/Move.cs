using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Move : IComparable<Move>
{

    public int[] cardDif;

    public int pileHolderDif;

    public bool isDrawMove;

    public int scoreEstimate;

    public int moveLength;
    public int moveMin;

    ///// <summary>
    ///// The cards used in the move, incase of draw move the drawn card
    ///// </summary>
    //public int[] cards; 

    public Move() { }
    public Move(int[] cardDif, int pileHolderDif, bool isDrawMove)
    {
        this.cardDif = cardDif;
        this.pileHolderDif = pileHolderDif;
        this.isDrawMove = isDrawMove;
        moveLength = 0;
        moveMin = 0;


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
        int[] pCards = g.getPlayerCards((int)player);

        //The cards on the table
        int[] tCards = g.getPlayerCards(0);

        cardDif = new int[g.cards.Length];


        int k = 0;
        bool foundMove = false;
        for (int i = 0; i < tCards.Length; i++)
        {
            if (tCards[i] < 0) { break; }

            //Takes all cards currently in the pile and gives them as points to the player
            //(240 = 16 * 15: handIndex = 15 represents it being a point)
            cardDif[tCards[i]] = 240 + (int)player - g.cards[tCards[i]];
        }
        for (int i = 0; i < pCards.Length; i++)
        {
            if (pCards[i] == -10) { break; }

            if (k < move.Length && pCards[i] == move[k])
            {
                //Move card to center pile and set its new index in center pile
                cardDif[move[k]] = (k << 4) + (g.cards[move[k]] & 8) - g.cards[move[k]];
                foundMove = true;
                k++;
            }
            //Reduce index by move.length;
            else if (foundMove) { cardDif[pCards[i]] = -16 * move.Length; }
        }

        pileHolderDif = (int)player - g.currentPileHolder;
        moveLength = move.ArrayLength();
        moveMin = move[0];
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
        int[] tCards = GameState.getPlayerCards(cards, 0);
        int[] pCards = GameState.getPlayerCards(cards, player);

        //if there are no cards on the table or the hand is full return 
        if (tCards.ArrayLength() == 0 || pCards[pCards.Length - 1] != -10) { return; }

        int tCard = tCards[0];

        if (top)
        {
            for (int i = 0; i < tCards.Length; i++)
            {
                if (tCards[i + 1] != -10 && i != tCards.Length - 1) { continue; }
                
                tCard = tCards[i];
                break;
            }
        }



        cardDif = new int[cards.Length];

        for (int i = pCards.Length - 1; i >= handIndex; i--)
        {
            //Shift all cards after the insertion point by 1 spot (aka 16)
            if (pCards[i] != -10) { cardDif[pCards[i]] = 16; }
        }

        cardDif[tCard] = (16 * handIndex + (cards[tCard] & 8) + player) - cards[tCard];

        //Flip the card incase flip == true
        if (flip && SBU.getCardFlip(cards[tCard]) == 0) { cardDif[tCard] += 8; }

        else if (flip) { cardDif[tCard] -= 8; }


        //if the bottom card is taken, shift all cards on the table 1 spot to not leave the bottom spot empty
        if (!top)
        {
            for (int i = 1; i < tCards.Length; i++)
            {
                if (tCards[i] == -10) { break; }
                cardDif[tCards[i]] = -16;
            }
        }

        pileHolderDif = 0;
        isDrawMove = true;
        moveLength = 1;
        moveMin = 0;
    }


    public bool CompareMoves(Move move)
    {
        if (move.pileHolderDif != pileHolderDif) { return false; }

        for (int i = 0; i < move.cardDif.Length; i++)
        {
            if (move.cardDif[i] != this.cardDif[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the points of a combination of cards (by indexes) by refrencing a premade array
    /// </summary>
    public int getValue(int[] cards)
    {
        int[] moveIndexes = retreiveMoveIndexes(cards);

        return getValue(cards, moveIndexes);
    }

    public static int getValue(int[] cards, int[] moveIndexes)
    {
        int moveLength = 0;
        int moveMinCard = 1000;
        int match = 0;
        if (moveIndexes.Length == 1 || moveIndexes[1] == -10 || SBU.getCurrentCardValue(cards, moveIndexes[0]) == SBU.getCurrentCardValue(cards, moveIndexes[1]))
        {
            match = 1;
        }

        for (int i = 0; i < moveIndexes.Length; i++)
        {
            if (moveIndexes[i] == -10) { break; }
            moveLength++;

            moveMinCard = Mathf.Min(moveMinCard, SBU.getCurrentCardValue(cards, moveIndexes[i]));
        }


        int moveValue = (moveLength - 1) * 100 + match * 10 + moveMinCard - 1;

        int b = Array.BinarySearch(SBU.moveValues, moveValue);
        return b < 0 ? -10 : b;
    }


    public int[] retreiveMoveIndexes(int[] cards)
    {
        int[] cardsAfterMove = ArrayExtensions.AddArray(cards, cardDif, false);
        int[] moveIndexes = new int[15].SetArray(-10);

        int k = 0;

        for (int i = 0; i < 44; i++)
        {
            if (SBU.getCardOwner(cardsAfterMove[i]) == 0 && SBU.getCardOwner(cards[i]) != SBU.getCardOwner(cardsAfterMove[i]))
            {
                moveIndexes[k] = i;
                k++;
            }
        }


        return moveIndexes;
    }



    /// <summary>
    /// This function generates all possible moves.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>a 2 dimensional array of all possible moves a player can play</returns>
    //Move Generation for putting down cards
    public static List<Move> GetPossibleMoves(GameState g, int player, bool onlyLegalMoves = true)
    {

        //Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
        int[] pCards = g.getPlayerCards(player);

        int tCardsValue = getValue(g.cards, g.getPlayerCards(0));
        //Get all the possible moves in the format of card indexes
        List<int[]> moveIndexList = new List<int[]>();

        for (int startCard = 0; startCard < pCards.Length && pCards[startCard] != -10; startCard++)
        {

            if (!onlyLegalMoves || getValue(g.cards, new int[1] { pCards[startCard] }) > tCardsValue) { moveIndexList.Add(new int[1] { pCards[startCard] }); }

            int currentCardValue = SBU.getCurrentCardValue(g.cards, pCards[startCard]);

            for (int h = -1; h < 2; h++)
            {
                for (int n = 1; startCard + n < pCards.Length - 1; n++)
                {
                    if (pCards[startCard + n] == -10) { break; }

                    if (currentCardValue != SBU.getCurrentCardValue(g.cards, pCards[startCard + n]) + n * h){ break; }

                    int[] move = new int[n + 1];

                    Array.Copy(pCards, startCard, move, 0, n + 1);
                    if (!onlyLegalMoves || getValue(g.cards, move) > tCardsValue) { moveIndexList.Add(move); }

                }
            }
        }



        //reformat the moves
        List<Move> moves = new List<Move>();

        for (int i = 0; i < moveIndexList.Count; i++)
        {
            moves.Add(new Move(g, moveIndexList[i], player));
        }

        return moves;
    }

    /// <summary>
    /// Gets all possible draw card moves (All GenerateDrawCardMoves are automatically legal) (This too needs to be very fast)
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public static List<Move> getPossibleDrawCardMoves(int[] cards, int player)
    {
        int tCardsLength = GameState.getPlayerCards(cards, 0).ArrayLength();
        List<Move> moves = new List<Move>();

        if (tCardsLength == 0) return moves;


        int[] pCards = GameState.getPlayerCards(cards, player);

        for (int i = 0; i < pCards.Length && (i == 0 || pCards[i - 1] != -10); i++)
        {
            foreach (bool b1 in new bool[] { true, false })
            {
                foreach (bool b2 in new bool[] { true, false })
                {
                    Move move = new Move(cards, b1, b2, player, i);
                    if (move.cardDif == null) { continue; }

                    moves.Add(move);

                }
                if (tCardsLength == 1) { break; }
            }
        }

        return moves;
    }


    /// <summary>
    /// Sorts moves based on scoreEstimation which is generated in Move Ordering, higher scores is put earlier in the list.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(Move other)
    {
        return other.scoreEstimate.CompareTo(scoreEstimate);
    }
}
