using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;


//ScoutBotFunctions
public static class SBF
{
    //The cards currently on the table
    static public List<int> tablePile = new List<int>();

    //Array of all players, each player has their list of cards and their points
    static public Player[] players;

    //the current turn
    static public int turn = 1;


    //Flip a card upside down
    public static int FlipCard(int card)
    { 
        //Convert card into bit array
        byte[] a = new byte[1] { BitConverter.GetBytes(card)[0] };
        BitArray b = new BitArray(a);

        //Swaps around the bit array
        for (int i = 0; i < b.Length / 2; i++) {
            bool temp = b[i];
            b[i] = b[i + 4];
            b[i + 4] = temp;
        }

        //convert back the bit array to a integer
        b.CopyTo(a, 0);

        return a[0];

    }

    //converts a int lower than 256 to a byte
    public static byte int8ToByte(int i)
    {
        return BitConverter.GetBytes(i)[0];
    }
    

    //Shuffle a list of cards
    public static List<int> ShuffleCards(List<int> cards)
    {
        int n = cards.Count;
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
    public static int CardToValue(int card)
    {
        byte[] b = new byte[1] { BitConverter.GetBytes(card)[0] };
        BitArray bits = new BitArray(b);
        bits.RightShift(4);

        bits.CopyTo(b, 0);

        return b[0];
    }


    //Evaluate the raw quality a move compared to all other moves
    //this is done by converting the byte formated move into a int.
    // Where the biggest digit is the number of cards -1,
    // middle digit (1 or 0) is if its a match (1) or a ladder (0),
    // last digits is the smallest value card in the move - 1.
    // This means that the integer will be bigger then the integer for all worse moves and smaller then the integer for all better moves,

    //TO BE DONE: // this is then used with a lookup table of all possible 133 values to convert the number into a percentile representing the move quality
    public static int RawEval(List<byte> cards)
    {
        
        //The leading digit is the amount of cards 1, the last digit is the smallest valued cards value - 1
        int i = (cards.Count - 1) * 100 + CardToValue(cards.Min()) - 1;

        //if cards are matching make the middle digit is 1, single digit moves are still counted as matching, otherwise if the cards are in a ladder the digit is a 0
        //The middle digit is in the 10 spot, hence +10.
        if (cards.Count > 1 && cards[0] == cards[1])
        {
            i += 10;
        }

        return i;
    }

    //Makes RawEval compatible with integers
    public static int RawEval(List<int> cards)
    {
        List<byte> b = new List<byte>();
        foreach (int card in cards)
        {
            b.Add(int8ToByte(card));
        }
        return RawEval(b);
    }




}
