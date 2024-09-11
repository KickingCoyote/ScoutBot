using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    

    //The deck of all cards before distribution
    public List<int> deck;


    //Used for debugging purposes so that the players' cards show up in the editor
    public List<int> p1;
    public List<int> p2;
    public List<int> p3;
    public List<int> p4;

    //Start of the game
    void Start()
    {
        

        //Create all 44 cards (10 9 is excluded)
        deck = CreateCards();
       
        
        //shuffles the deck of cards
        deck = SBF.ShuffleCards(deck);


        //Distribute 11 cards to all players
        SBF.players = new Player[4];
        for (int i = 0; i < 4; i++)
        {
            SBF.players[i] = new Player(DistributeCards());
        }

        //Used for debugging purposes so that the players' cards show up in the editor
        p1 = SBF.players[0].hand;
        p2 = SBF.players[1].hand;
        p3 = SBF.players[2].hand;
        p4 = SBF.players[3].hand;


    }


    //TODO: make this more efficient
    //Code for distribute cards to a player
    private List<int> DistributeCards()
    {

        List<int> playerCards = new List<int>();


        //Pick the 11 first cards in the deck, and do a 50/50 split to if cards should be flipped or not, then remove those cards from the cards list
        for(int i = 0; i < 11; i++)
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                playerCards.Add(SBF.FlipCard(deck[0]));
            }
            else
            {
                playerCards.Add(deck[0]);
            }

            
            deck.RemoveAt(0);
        }

        return playerCards; 
    }


    //Create all 44 cards for the game
    /*
     * Cards are represented as ints in the format (upper number x 16) + (lower number)
    so that in bytes the 4 left zeros are upper number and the 4 right zeros are the lower number
    0010 0011 would be 2 3
        */
    private List<int> CreateCards()
    {

        List<int> cards = new List<int>(); 

        for (int i = 1; i < 11; i++) //I is upper number
        {
            for (int j = 1; j < 11; j++) //J is lower number
            {
                if (j > i)
                {
                    cards.Add(i * 16 + j);
                }
            }
        }


        //removes the card 10 9, 154 hence (9 * 16) + 10 = 154
        cards.Remove(154);

        return cards;
    }
    
}
