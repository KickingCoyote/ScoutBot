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
/// Scout Bot Utilities, functions required to parse and use the game data
/// </summary>
public static class SBU
{
    public static GameState gameState;

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



    /* Dictionary:
     * 
     *  Card index: the index to a card in the gameState.cards array
     *  Card: the value correlated with a card index, stored at gameState.cards[cardIndex]
     *  Card Value: the value of card stored at cardValues[cardIndex], exclusively acquired through GetCardValue()
     *  Draw move / take card move: a move where a player picks up a card from the middle pile
     *  Put card move: a move where a player places cards in the middle pile
     *  
     */



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
        return cardValue % 16 * 16 + ((cardValue - (cardValue % 16)) / 16);
    }


    public static int getCurrentCardValue(int[] cards, int cardIndex)
    {
        return getCurrentCardValue(GetCardValue(cards, cardIndex));
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
    /// <returns></returns>
    public static int getCardHandIndex(int card)
    {
        //Uses same logic as getCurrentCardValue
        return getCurrentCardValue(card);
    }

    /// <summary>
    /// Gets the owner of the card where 0 is the table pile and 1-4 is player 1-4
    /// </summary>
    /// <returns></returns>
    public static int getCardOwner(int card)
    {
        return card % 8;
    }


    /// <summary>
    /// Gets if the card is flipped or not, flipped cards (1) has the largest value first
    /// </summary>
    /// <param name="card">card</param>
    /// <returns>0 or 1 for unflipped / flipped cards</returns>
    public static int getCardFlip(int card)
    {
        return ((card % 16) - (card % 8)) / 8;
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



    public static int RandomInt()
    {
        System.Random r = new System.Random();
        return r.Next(0, 2147483647);
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
            if (GetCardValue(cards, i) == value)
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
    public static int GetCardValue(int[] cards, int cardIndex)
    {

        if (getCardFlip(cards[cardIndex]) == 0)
        {
            return cardValues[cardIndex];
        }
        return FlipCardValue(cardValues[cardIndex]);
        
    }


    /// <summary>
    /// Gets the card in x:y string format
    /// </summary>
    /// <param name="cardIndex">card index</param>
    /// <returns>string in format [value 1]:[value 2] </returns>
    public static string CardToString(int[] cards, int cardIndex)
    {

        int value = GetCardValue(cards, cardIndex);
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



}
