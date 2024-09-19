using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SBU
{

    // 0000, 0, 000. handPos, flipped, owner

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



    //Returns the upwards facing value on a card
    public static int getCurrentCardValue(int cardValue)
    {
        byte[] b = new byte[1] { BitConverter.GetBytes(cardValue)[0] };
        BitArray bits = new BitArray(b);
        bits.RightShift(4);

        bits.CopyTo(b, 0);

        return b[0];
    }


    public static void Move(int[] cards, int[] move)
    {
        for (int i = 0; i < 44; i++)
        {
            cards[i] += move[i];
        }
    }


    public static void GetPossibleMoves(int player, int[] cards)
    {

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
            if (cardValues[i] == value)
            {
                return i;
            }
        }

        //íf no card is found cast out of range exception
        Debug.Log("Invalid Value");
        return -1;
    }



    public static string CardToString(int card)
    {
        int value = cardValues[card];
        return getCurrentCardValue(value) + ":" + (value - (16 * getCurrentCardValue(value)));
    }
    public static int CardFromString(string cardStr)
    {
        string[] str = cardStr.Split(':');
        return getCardFromValue(int.Parse(str[0]) * 16 + int.Parse(str[1]));
    }

}
