using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScoutBotFunctions 
{

    //The cards currently on the table
    static public List<int> tablePile = new List<int>();


    //Flip a card upside down
    //TODO: make faster
    public static int FlipCard(int card)
    {
        if (card < 10)
        {
            return card * 10;
        }

        string s = card.ToString();

        return int.Parse(s.Substring(1, 1) + s.Substring(0 ,1));

    }


    //Shuffle a list of cards
    public static List<int> ShuffleCards(List<int> cards)
    {

        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }



        return cards;
    }

    
       

}
