using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.SocialPlatforms.Impl;


/// <summary>
/// Scout Bot Utilities, All functions required to run the base game
/// </summary>
public static class SBU
{
    public static GameState gameState = new GameState(new int[44], 1, 0);

    //gameState.cards
    //All cards are stored in 1 44 length int[], the index represents which card it is and the value has all the needed data about that card 
    //each value is stored such as that when viewed in byte form it looks like XXXX X XXX.
    //the right most 3 digits represents who has the card (0 for middle pile, 1..4 for player 1..4)
    //the middle digit represents if the card is flipped or not, where the LARGEST VALUE is always flipped DOWN if it's 0
    //the left most 4 digit represents the index of where in the players hand (or middle pile) the card is located, if the digits are 1111 that represents the card being a point instead of a card
    
    //gameState.turn
    //A variable between 1 and 4 representing which players turn it is

    //gameState.currentPileHolder
    //Keeps track of who the cards in the middle belonged to, for giving points

    //Array containing the value of each card index, the value is value 1 * 16 + value 2, where value 1 always is the smaller of the two
    public static int[] cardValues = new int[44];




    //All possible moves written in the format X X X
    //where the first number is the amount of cards in the move - 1
    //second digit is 1 if match, 0 if flip
    //third digit is the smallest value in the move - 1
    public static int[] moveValues = new int[] { 
        10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 100, 101, 102, 103, 104, 105, 106, 107, 108, 110,
        111, 112, 113, 114, 115, 116, 117, 118, 119, 200, 201, 202, 203, 204, 205, 206, 207, 210,
        211, 212, 213, 214, 215, 216, 217, 218, 219, 300, 301, 302, 303, 304, 305, 306, 310, 311, 
        312, 313, 314, 315, 316, 317, 318, 319, 400, 401, 402, 403, 404, 405, 410, 411, 412, 413,
        414, 415, 416, 417, 418, 419, 500, 501, 502, 503, 504, 510, 511, 512, 513, 514, 515, 516,
        517, 518, 519, 600, 601, 602, 603, 610, 611, 612, 613, 614, 615, 616, 617, 618, 619, 700, 
        701, 702, 710, 711, 712, 713, 714, 715, 716, 717, 718, 719, 800, 801, 810, 811, 812, 813, 
        814, 815, 816, 817, 818, 819, 900 
    };

    /// <summary>
    /// Shuffle a array of cards
    /// </summary>
    /// <param name="cards">The cards to be shuffled</param>
    /// <returns>The shuffled cards</returns>
    public static int[] ShuffleCards(int[] cards)
    {
        int n = cards.Length;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            int value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
        return cards;
    }


    /// <summary>
    /// Flip a card upside down
    /// </summary>
    /// <param name="cardValue">the value of the card from the cardValues array</param>
    /// <returns>The bottom value of the cardValue</returns>
    public static int FlipCardValue(int cardValue)
    {
        //Convert card into bit array
        byte[] a = new byte[1] { BitConverter.GetBytes(cardValue)[0] };
        BitArray b = new BitArray(a);

        //Swaps around the bit array
        for (int i = 0; i < b.Length / 2; i++)
        {
            bool temp = b[i];
            b[i] = b[i + 4];
            b[i + 4] = temp;
        }

        //convert back the bit array to a integer
        b.CopyTo(a, 0);

        return a[0];

    }


    //Returns the upwards facing value on a card given the card value
    public static int getCurrentCardValue(int cardValue)
    {
        return (cardValue - (cardValue % 16)) / 16; 
    }



    //The following 3 functions takes in the int that is the card data of a card and extracts specific information  
    //The following functions returns incorrect values if anything is negative hence inputing a move array will not return the correct information in most cases
    /// <summary>
    /// Gets where in the player hand the card is located, where 0 is the first (left most) card
    /// </summary>
    /// <param name="cardValue">card</param>
    /// <returns></returns>
    public static int getCardHandIndex(int cardValue)
    {
        //Uses same logic as getCurrentCardValue
        return getCurrentCardValue(cardValue);
    }

    /// <summary>
    /// Gets the owner of the card where 0 is the table pile and 1-4 is player 1-4
    /// </summary>
    /// <param name="cardValue">card</param>
    /// <returns></returns>
    public static int getCardOwner(int cardValue)
    {
        return cardValue % 8;
    }


    /// <summary>
    /// Gets if the card is flipped or not, flipped cards (1) has the largest value first
    /// </summary>
    /// <param name="card">card</param>
    /// <returns>0 or 1 for unflipped / flipped cards</returns>
    public static int getCardFlip(int cardValue)
    {
        return ((cardValue % 16) - (cardValue % 8)) / 8;
    }



    /// <summary>
    /// This function generates all possible moves. THIS METHOD NEEDS TO BE EXTREMELY FAST.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>a 2 dimensional array of all possible moves a player can play</returns>
    //Move Generation for putting down cards
    public static List<Move> GetPossibleMoves(GameState g, int player)
    {

        //Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
        int[] pCards = getPlayerCards(g.cards, player);
        
        //Get all the possible moves in the format of card indexes
        List<int[]> temp = new List<int[]>();

        for (int i = 0; i < pCards.Length; i++)
        {

            if (pCards[i] == -10) { break; }


            temp.Add(new int[1] {pCards[i]});

            for (int j = 1; j < pCards.Length - 1; j++)
            {

                if (pCards[i + j] == -10) { break; }  
                if (getCurrentCardValue(getValueOfCard(g.cards, pCards[i])) == getCurrentCardValue(getValueOfCard(g.cards, pCards[i + j])))
                {
                    int[] move = new int[j + 1].SetArray(-10);
                    move[0] = pCards[i];

                    for (int k = 1; k < j + 1; k++)
                    {                        
                        move[k] = pCards[i + k];
                        temp.Add(move);
                    }
                }
                else { break; }
            }
            for (int j = 1; j < pCards.Length - 1; j++)
            {
                if (pCards[i + j] == -10) { break; }
                if (getCurrentCardValue(getValueOfCard(g.cards, pCards[i])) == getCurrentCardValue(getValueOfCard(g.cards, pCards[i + j])) - j)
                {
                    int[] move = new int[j + 1].SetArray(-10);
                    move[0] = pCards[i];

                    for (int k = 1; k < j + 1; k++)
                    {
                        move[k] = pCards[i + k];
                        temp.Add(move);
                    }
                }
                else { break; }
            }
            for (int j = 1; j < pCards.Length - 1; j++)
            {
                if (pCards[i + j] == -10) { break; }
                if (getCurrentCardValue(getValueOfCard(g.cards, pCards[i])) == getCurrentCardValue(getValueOfCard(g.cards, pCards[i + j])) + j)
                {
                    int[] move = new int[j + 1].SetArray(-10);
                    move[0] = pCards[i];

                    for (int k = 1; k < j + 1; k++)
                    {
                        move[k] = pCards[i + k];
                        temp.Add(move);
                    }
                }
                else { break; }
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
    /// Gets all legal put card moves
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cards"></param>
    /// <returns></returns>
    public static List<Move> getAllLegalMoves(GameState g, int player)
    {
        List<Move> allPossibleMoves = GetPossibleMoves(g, player);

        List<Move> allLegalMoves = new List<Move>(); 

        if (getPlayerCards(g.cards, 0).ArrayLength() == 0) { return allPossibleMoves; }

        foreach (Move move in allPossibleMoves)
        {

            if (MoveValue(g.cards, MoveIndexesFromMove(g.cards, move.cardDif)) > MoveValue(g.cards, getPlayerCards(g.cards, 0)))
            {
                allLegalMoves.Add(move);
            }

        }


        return allLegalMoves;
    }




    /// <summary>
    /// Gets all possible draw card moves (All GenerateDrawCardMoves are automatically legal) (This too needs to be very fast)
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public static List<Move> getPossibleDrawCardMoves(int[] cards, int player)
    {
        List<Move> moves = new List<Move>();

        int[] pCards = getPlayerCards(cards, player);

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
            }

        }

        return moves;
    }


    




    /// <summary>
    /// Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="player"></param>
    /// <returns>A 15 long array of card indexes where emtpy values are -10</returns>
    public static int[] getPlayerCards(int[] cards, int player)
    {
        int[] pCards = new int[15];
        for (int i = 0; i < 15; i++) { pCards[i] = -10; }

        for (int i = 0; i < cards.Length; i++)
        {
            //HandIndex 15 represents the card being a point and not actually in the hand
            if (getCardOwner(cards[i]) == player && getCardHandIndex(cards[i]) != 15)
            {
                pCards[getCardHandIndex(cards[i])] = i;
            }

        }

        return pCards;
    }

    /// <summary>
    /// Creates all the card values that determine what card index relates to what card. This is ran once on start
    /// </summary>
    public static void CreateCardValues()
    {
        int n = 0;
        for (int i = 1; i < 11; i++) //I is top number and always smaller by default
        {
            for (int j = 1; j < 11; j++) //J is lower number and always bigger by default
            {
                if (j > i)
                {
                    if (j == 10 && i == 9) { return; }

                    cardValues[n] = i * 16 + j;
                    n++;
                }
            }
        }
    }


    
    /// <summary>
    /// Gets the card index from a value
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="value"></param>
    /// <returns>Card Index</returns>
    public static int getCardFromValue(int[] cards, int value)
    {
        for (int i = 0; i < 44; i++)
        {
            if (getValueOfCard(cards, i) == value)
            {
                return i;
            }
        }

        //íf no card is found cast out of range exception
        Debug.Log("Invalid Value");
        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="cardIndex">Index of card</param>
    /// <returns></returns>
    public static int getValueOfCard(int[] cards, int cardIndex)
    {

        if (getCardFlip(cards[cardIndex]) == 0)
        {
            return cardValues[cardIndex];
        }
        else
        {
            return FlipCardValue(cardValues[cardIndex]);
        }
    }


    public static int getPlayerScore(int[] cards, int player)
    {
        int score = 0;

        for(int i = 0; i < cards.Length; i++)
        {
            //If the card hand index is equal to 15 the card is counted as a point instead of a card
            if (getCardOwner(cards[i]) == player && getCardHandIndex(cards[i]) == 15)
            {
                score++;
            }
        }
        return score;
    }

    /// <summary>
    /// Gets the card in x:y string format
    /// </summary>
    /// <param name="cardIndex">card index</param>
    /// <returns>string in format [value 1]:[value 2] </returns>
    public static string CardToString(int[] cards, int cardIndex)
    {
        int value = getValueOfCard(cards, cardIndex);
        return getCurrentCardValue(value) + ":" + (value - (16 * getCurrentCardValue(value)));
    }

    /// <summary>
    /// Gets a card index from a string of the format [value 1]:[value 2]
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="cardStr"></param>
    /// <returns>Card Index</returns>
    public static int CardFromString(int[] cards, string cardStr)
    {
        string[] str = cardStr.Split(':');
        return getCardFromValue(cards, int.Parse(str[0]) * 16 + int.Parse(str[1]));
    }


    //public static int[] getMoveIndexes(int[] move, int[] cards)
    //{
    //    int[] moveIndexes = new int[15];
    //    SetArray(moveIndexes, -10);

    //    return moveIndexes;
    //}


    public static int[] MoveIndexesFromMove(int[] cards, int[] move)
    {

        int[] cardsAfterMove = ArrayExtensions.AddArray(cards, move, false);

        int[] moveIndexes = new int[15].SetArray(-10);

        int k = 0;

        for (int i = 0; i < 44; i++)
        {
            if (getCardOwner(cardsAfterMove[i]) == 0 && getCardOwner(cards[i]) != getCardOwner(cardsAfterMove[i]))
            {
                moveIndexes[k] = i;
                k++;
            }
        }


        return moveIndexes;
    }


    //gets the points of a move
    /// <summary>
    /// Gets the points of a combination of cards (by indexes) by refrencing a premade array
    /// </summary>
    /// <param name="move">the set of card indexes to be evaluated</param>
    /// <returns>a int between 0 132 that is the value of that set of cards</returns>
    public static int MoveValue(int[] cards, int[] move)
    {
        int moveLength = 0;
        int moveMinCard = 1000;
        int match;
        if(move.Length == 1 || move[1] == -10 || getCurrentCardValue(getValueOfCard(cards, move[0])) == getCurrentCardValue(getValueOfCard(cards, move[1]))){
            match = 1;
        }
        else { match = 0; }

        for (int i = 0; i < move.Length; i++)
        {
            if (move[i] == -10) { break; }
            moveLength++;
            if (moveMinCard > getCurrentCardValue(getValueOfCard(cards, move[i])))
            {
                moveMinCard = getCurrentCardValue(getValueOfCard(cards, move[i]));
            } 

        }


        int moveValue = (moveLength - 1) * 100 + match * 10 + moveMinCard - 1;

        for (int i = 0; i < moveValues.Length; i++)
        {
            if (moveValues[i] == moveValue)
            {
                return i + 1;
            }
        }

        return -10;
    }


    /// <summary>
    /// Function that checks if its game over or not
    /// </summary>
    /// <param name="g">game state</param>
    /// <returns>true if game over, otherwise false</returns>
    public static bool CheckGameOver(GameState g)
    {
        if (g.currentPileHolder == g.turn)
        {
            return true;
        }
        if(getPlayerCards(g.cards, g.turn)[0] == -10)
        {
            return true;
        }

        return false;
    }


    /// <summary>
    /// Gets which player is in the lead purely on score
    /// </summary>
    /// <param name="cards"></param>
    /// <returns>int for winning player</returns>
    public static int getWinningPlayer(int[] cards)
    {
        int winningPlayer = 0;
        for (int i = 1; i < 5; i++)
        {
            if(SBU.getPlayerScore(cards, i) > SBU.getPlayerScore(cards, winningPlayer))
            {
                winningPlayer = i;
            }
        }

        return winningPlayer;
    }
    


}
