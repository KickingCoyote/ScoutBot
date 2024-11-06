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
        int[] pCards = g.getPlayerCards((int)player);

        //The cards on the table
        int[] tCards = g.getPlayerCards(0);

        int[] moveArray = g.cards.CopyArray();

        

        int k = 0;
        bool foundMove = false;
        for (int i = 0; i < tCards.Length; i++)
        {
            if (tCards[i] < 0) { break; }

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
            else if (foundMove) { moveArray[pCards[i]] -= 16 * move.Length; }
        }

        cardDif = ArrayExtensions.AddArray(moveArray, g.cards, true);
        pileHolderDif = (int)player - g.currentPileHolder;
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
        int tCard = -10;
        int[] pCards = GameState.getPlayerCards(cards, player);

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
        int match;
        if (moveIndexes.Length == 1 || moveIndexes[1] == -10 || SBU.getCurrentCardValue(SBU.getValueOfCard(cards, moveIndexes[0])) == SBU.getCurrentCardValue(SBU.getValueOfCard(cards, moveIndexes[1])))
        {
            match = 1;
        }
        else { match = 0; }

        for (int i = 0; i < moveIndexes.Length; i++)
        {
            if (moveIndexes[i] == -10) { break; }
            moveLength++;
            if (moveMinCard > SBU.getCurrentCardValue(SBU.getValueOfCard(cards, moveIndexes[i])))
            {
                moveMinCard = SBU.getCurrentCardValue(SBU.getValueOfCard(cards, moveIndexes[i]));
            }
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
    /// This function generates all possible moves. THIS METHOD NEEDS TO BE EXTREMELY FAST.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>a 2 dimensional array of all possible moves a player can play</returns>
    //Move Generation for putting down cards
    public static List<Move> GetPossibleMoves(GameState g, int player, bool onlyLegalMoves = true)
    {

        //Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
        int[] pCards = g.getPlayerCards(player);

        int tCardsValue = getValue(g.cards, GameState.getPlayerCards(g.cards, 0));
        //Get all the possible moves in the format of card indexes
        List<int[]> temp = new List<int[]>();

        for (int i = 0; i < pCards.Length; i++)
        {

            if (pCards[i] == -10) { break; }


            if (!onlyLegalMoves || getValue(g.cards, new int[1] { pCards[i] }) > tCardsValue) { temp.Add(new int[1] { pCards[i] }); }

            for (int h = -1; h < 2; h++)
            {
                for (int j = 1; j < pCards.Length - 1; j++)
                {
                    if (i + j >= pCards.Length || pCards[i + j] == -10) { break; }
                    if (SBU.getCurrentCardValue(SBU.getValueOfCard(g.cards, pCards[i])) == SBU.getCurrentCardValue(SBU.getValueOfCard(g.cards, pCards[i + j])) + j * h)
                    {
                        int[] move = new int[j + 1].SetArray(-10);
                        move[0] = pCards[i];

                        for (int k = 1; k < j + 1; k++)
                        {
                            move[k] = pCards[i + k];
                            //If onlyLegalMoves = true check if the moveValue is higher than table pile
                            if (!onlyLegalMoves || getValue(g.cards, move) > tCardsValue) { temp.Add(move); }
                        }
                    }
                    else { break; }
                }
            }
        }

        //reformat the moves
        List<Move> moves = new List<Move>();

        for (int i = 0; i < temp.Count; i++)
        {
            moves.Add(new Move(g, temp.ElementAt(i), player));
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

        for (int i = 0; i < pCards.Length; i++)
        {
            if (i != 0 && pCards[i - 1] == -10) { break; }

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

    public int CompareTo(Move other)
    {
        return this.scoreEstimate.CompareTo(other.scoreEstimate);
    }
}
