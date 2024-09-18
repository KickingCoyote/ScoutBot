using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLoop : MonoBehaviour
{
 
    
    public GameObject inputField;

    private string inputString;

    //The deck of all cards before distribution
    public List<int> deck;



    //Used for UI
    [SerializeField] TextMeshProUGUI[] pText = new TextMeshProUGUI[4];
    [SerializeField] TextMeshProUGUI infoText;

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

        UpdateUI();

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

    //Activated from buttons ingame
    public void TakeCard()
    {
        string[] s = inputString.Split(' ');
        // true / false for top/bottom card, true / false for fliping the card, int for card placement
        SBF.players[(SBF.turn - 1) % 4].PickUp(bool.Parse(s[0]), bool.Parse(s[1]), int.Parse(s[2]));
        SBF.turn++;
        UpdateUI();
    }

    public void PutCard()
    {
        string[] s = inputString.Split(' ');
        List<int> cards = new List<int>();
        foreach (string sCard in s)
        {
            cards.Add(SBF.CardFromString(sCard));
        }

        SBF.players[(SBF.turn - 1)% 4].LayDownCards(cards);
        SBF.tablePileHolder = (SBF.turn - 1) % 4;
        SBF.turn++;
        UpdateUI();
    }

    //ran every input field is deselected
    public void UpdateString(string s)
    {
        inputString = s;
    }

    //Renders the ingame UI
    private void UpdateUI()
    {

        for (int i = 0; i < 4; i++)
        {

            pText[i].text = "Player " + (i + 1) + " P:"  + SBF.players[i].points + " |";

            for (int j = 0; j < SBF.players[i].hand.Count; j++)
            {
                pText[i].text += " " + SBF.CardToString(SBF.players[i].hand[j]);
            }
        }


        infoText.text = "Turn: " + SBF.turn + "       |       Table Pile: ";

        for (int i = 0; i < SBF.tablePile.Count; i++)
        {
            infoText.text += " " + SBF.CardToString(SBF.tablePile[i]);
        }

        
    }

}
