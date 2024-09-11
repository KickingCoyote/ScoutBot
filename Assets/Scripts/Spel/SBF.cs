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
    // Where the biggest digits are the number of cards,
    // middle digit (1 or 0) is if its a match (1) or a ladder (0),
    // last digits is the smallest value in the move.
    // This means that the integer will be bigger then the integer for all worse moves and smaller then the integer for all better moves,

    //TO BE DONE: // this is then used with a lookup table of all possible 133 values to convert the number into a percentile representing the move quality
    public static int RawEval(byte move)
    {
        byte[] b = new byte[1] { move };
        BitArray bits = new BitArray(b);
        int n = move;


        if (bits[8]) { n -= 128; } //If match, remove the indicator bit (last 128)

        // (n / 10) extracts the leading digit as integer division deletes all decimals
        n = (n / 10) * 100 + (n - (n / 10));  

        if (bits[8]){n += 10;}  //Add back the indicator incase of a match to make matches valued over ladders

        return n;
    }

    //Encrypts a set of cards into a move, the move must be valid for this to work.
    //this is done to compress the size of moves for optimization
    public static byte MoveEncrypter(List<int> cards)
    {
        List<byte> b = new List<byte>();
        foreach(int card in cards)
        {
            b.Add(Convert.ToByte(card));
        }
        return MoveEncrypter(b); 
    }

    //a move is stored in a byte in the format 0 0000000,
    //where the first 7 digits are (card count * 10) + lowest card value, and the last digit is 1 incase of matching and 0 incase of a ladder
    public static byte MoveEncrypter(List<byte> cards)
    {
        //Compresses the list to a 2 digit number between 00 representing 1 one, and 88 representing 9 nines.
        //The first digit represents how many cards the move consists of, the second digit represents what the lowest value card in the move is.
        //Both values are reduced by 1 to take use of the zeros
        int i = (cards.Count -1) * 10 + CardToValue(cards.Min()) - 1;

        //if cards are matching make the last digit 1, single digit moves are still counted as matching, otherwise if the cards are in a ladder the digit is a 0
        //The last digit in a byte is worth 2^7 hence the + 128.
        if (cards.Count > 1 && cards[0] == cards[1])
        {
            i += 128;
        }
        
        return BitConverter.GetBytes(i)[0];
    }

    public static List<int> MoveDecrypter(byte move)
    {
        int amount;
        int smallest;
        bool match;


        return new List<int>();
    }


}
