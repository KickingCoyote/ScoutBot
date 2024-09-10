using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    //The hand of the player
    public List<int> hand;


    //The points
    int points;

    public Player(List<int> hand)
    {
        this.hand = hand;
        points = 0;
    }


    /*
     Takes cards from hand and puts them in middle pile
     
     
     */
    public void LayDownCards(List<int> cards)
    {

    }


    //Pick Up 1 card from middle pile
    public void PickUp(int card)
    {

    }


}
