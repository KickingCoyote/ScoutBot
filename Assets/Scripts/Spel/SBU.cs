using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public static class SBU
{

    // 0000, 0, 000. (hand) index, flip, owner

    public static int[] cards = new int[44];

    public static int[] scores = new int[4] {0,0,0,0};

    public static int turn = -1;

    public static int[] cardValues = new int[44];


    //Shuffle a list of cards
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


    //Flip a card upside down
    public static int FlipCardValue(int card)
    {
        //Convert card into bit array
        byte[] a = new byte[1] { BitConverter.GetBytes(card)[0] };
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


    //Returns the upwards facing value on a card
    public static int getCurrentCardValue(int cardValue)
    {
        byte[] b = new byte[1] { BitConverter.GetBytes(cardValue)[0] };
        BitArray bits = new BitArray(b);
        bits.RightShift(4);

        bits.CopyTo(b, 0);

        return b[0];
    }


    //The following functions may not work with negative numbers
    /// <summary>
    /// Get the hand index of a card
    /// </summary>
    /// <param name="card">the card</param>
    /// <returns></returns>
    public static int getCardIndex(int card)
    {
        //Uses same logic as getCurrentCardValue
        return getCurrentCardValue(card);
    }
    public static int getCardOwner(int card)
    {

        byte[] b = new byte[1] { BitConverter.GetBytes(card)[0] };
        BitArray bits = new BitArray(b);
        for (int i = 3; i < 8; i++)
        {
            bits[i] = false;
        }
        bits.CopyTo(b, 0);

        return b[0];
    }
    public static int getCardFlip(int card)
    {
        byte[] b = new byte[1] { BitConverter.GetBytes(card)[0] };
        BitArray bits = new BitArray(b);

        if (bits[3]) { return 1; }
        return 0;

    }


    public static int[] Move(int[] cards, int[] move)
    {
        for (int i = 0; i < move.Length; i++)
        {
            cards[i] += move[i];
        }

        return cards;
    }


    //doom, fix this sphagetti code
    //Move Generation for putting down cards
    public static int[,] GetPossibleMoves(int player, int[] cards)
    {

        //Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
        int[] pCards = getPlayerCards(cards, player);
        
        //Get all the possible moves
        List<int[]> temp = new List<int[]>();

        for (int i = 0; i < pCards.Length; i++)
        {
            temp.Add(new int[1] {pCards[i]});

            for (int j = 1; j < pCards.Length - 1; j++)
            {
                if (getCurrentCardValue(getValueOfCard(pCards[i])) == getCurrentCardValue(getValueOfCard(pCards[i + j])))
                {
                    for (int k = 0; k < j + 1; k++)
                    {
                        int[] move = new int[j + 1];
                        move[k] = pCards[i + k];
                        temp.Add(move);
                    }                    
                }
                else { break; }
            }
            for (int j = 1; j < pCards.Length - 1; j++)
            {
                if (getCurrentCardValue(getValueOfCard(pCards[i])) == getCurrentCardValue(getValueOfCard(pCards[i + j])) - 1)
                {
                    for (int k = 0; k < j + 1; k++)
                    {
                        int[] move = new int[j + 1];
                        move[k] = pCards[i + k];
                        temp.Add(move);
                    }
                }
                else { break; }
            }
            for (int j = 1; j < pCards.Length - 1; j++)
            {
                if (getCurrentCardValue(getValueOfCard(pCards[i])) == getCurrentCardValue(getValueOfCard(pCards[i + j])) + 1)
                {
                    for (int k = 0; k < j + 1; k++)
                    {
                        int[] move = new int[j + 1];
                        move[k] = pCards[i + k];
                        temp.Add(move);
                    }
                }
                else { break; }
            }
        }


        //reformat the moves
        int[,] moves = new int[44, temp.Count];

        for (int i = 0; i < temp.Count; i++)
        {
            int[] move = GenerateMove(cards, temp.ElementAt(i), player);

            for (int j = 0; j < 44; j++) { moves[j, i] = move[j]; }

        }


        return moves;
    }

    /// <summary>
    /// Generates a move array from a set of cards (move) and a GameState
    /// </summary>
    /// <param name="cards">the global cards (GameState)</param>
    /// <param name="move">the set of cards that the move consists of, must be in order</param>
    /// <returns></returns>
    public static int[] GenerateMove(int[] cards, int[] move, int player)
    {


        int[] pCards = getPlayerCards(cards, player); 

        int[] moveArray = new int[44];

        int k = 0;
        bool foundMove = false;
        for (int j = 0; j < pCards.Length; j++)
        {
            if (pCards[j] == move[k])
            {
                //Move card to center pile and set its new index in center pile
                moveArray[move[k]] = 16 * (k - getCardIndex(cards[move[k]])) - player;

                foundMove = true;
                k++;
            }
            else if (foundMove)
            {
                //Reduce index by move.length;
                moveArray[pCards[j]] = -16 * move.Length;

            }

        }

        return moveArray;
    }

    /// <summary>
    /// Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    private static int[] getPlayerCards(int[] cards, int player)
    {
        int[] pCards = new int[15];
        for (int i = 0; i < 15; i++) { pCards[i] = -10; }

        for (int i = 0; i < cards.Length; i++)
        {
            if (getCardOwner(cards[i]) == player)
            {
                pCards[getCardIndex(cards[i])] = i;
            }

        }

        return pCards;
    }


    private static void CreateCardValues()
    {
        int n = 0;
        for (int i = 1; i < 11; i++) //I is upper number
        {
            for (int j = 1; j < 11; j++) //J is lower number
            {
                if (j > i)
                {
                    cardValues[n] = i * 16 + j;
                    n++;
                }
            }
        }
    }


    

    public static int getCardFromValue(int value)
    {
        for (int i = 0; i < 44; i++)
        {
            if (getValueOfCard(i) == value)
            {
                return i;
            }
        }

        //íf no card is found cast out of range exception
        Debug.Log("Invalid Value");
        return -1;
    }
    public static int getValueOfCard(int card)
    {
        if (getCardFlip(card) == 0)
        {
            return cardValues[card];
        }
        else
        {
            return FlipCardValue(cardValues[card]);
        }
    }


    public static string CardToString(int card)
    {
        int value = getValueOfCard(card);
        return getCurrentCardValue(value) + ":" + (value - (16 * getCurrentCardValue(value)));
    }
    public static int CardFromString(string cardStr)
    {
        string[] str = cardStr.Split(':');
        return getCardFromValue(int.Parse(str[0]) * 16 + int.Parse(str[1]));
    }


    //gets the points of a move
    public static int moveValue(int[] move)
    {

        int length = 0;
        int startingValue = -1;
        int isMatch = 1;
        int previousCard = -2;

        for (int i = 0; i < move.Length; i++)
        {
            //if the card owner isnt 0 means that the card owner is being changed. aka that card is being moved
            if (getCardOwner(move[i]) != 0)
            {
                if (startingValue == -1) { startingValue = getCurrentCardValue(getValueOfCard(i)); }
                length++;
                if(i != move.Length - 1 && getCurrentCardValue(getValueOfCard(i)) == getCurrentCardValue(getValueOfCard(previousCard)) + 1)
                {
                    isMatch = 0;
                }

                previousCard = i;
            }
        }

        return (length - 1) * 100 + isMatch * 10 + startingValue - 1;
    }


}
