using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    //Array of all players, each player has their list of cards and their points
    public Player[] players;

    //The deck of all cards before distribution
    public List<int> deck;


    //Start of the game
    void Start()
    {

        //Create all 44 cards (10 9 is excluded)
        deck = CreateCards();


        //shuffles the deck of cards
        deck = ScoutBotFunctions.ShuffleCards(deck);


        //Distribute 11 cards to all players
        players = new Player[4];
        for (int i = 0; i < 4; i++)
        {
            players[i] = new Player(DistributeCards());
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }



    //TODO: make this more efficient
    //Code for distribute cards to a player
    private List<int> DistributeCards()
    {

        List<int> playerCards = new List<int>();

        //temporary variable
        List<int> tempCards = new List<int>(deck);

        //Pick the 11 first cards in the deck, and do a 50/50 split to if cards should be flipped or not, then remove those cards from the cards list
        for(int i = 0; i < 11; i++)
        {
            if (Random.Range(0, 2) == 0)
            {
                playerCards.Add(ScoutBotFunctions.FlipCard(deck[i]));
            }
            else
            {
                playerCards.Add(deck[i]);
            }

            
            tempCards.RemoveAt(0);
        }

        deck = tempCards;

        return new List<int>(); 
    }


    //Create all 44 cards for the game
    private List<int> CreateCards()
    {

        List<int> cards = new List<int>();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (j == i)
                {
                    continue;
                }
                if (j >= i)
                {
                    cards.Add(i * 10 + j);
                }
            }
        }


        //removes the card 10 9
        cards.Remove(9);

        return cards;
    }
    
}
