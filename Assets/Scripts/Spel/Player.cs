using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player
{


    //The hand of the player
    public List<int> hand;

    //The points
    public int points;

    //All possible combinations of cards the player can lay out
    public List<byte[]> moves;


    //0010 xxxx, 0010 xxxx, 0010 xxx

    public Player(List<int> hand)
    {
        this.hand = hand;
        points = 0;
        moves = new List<byte[]>();
        CalculatePossibleMoves();
    }


    /*
     Takes cards from hand and puts them in middle pile
     */
    public void LayDownCards(List<int> cards)
    {

        

        //Sees if it is possible to put down that combination of cards
        if (!moves.Contains(SBF.encryptMove(cards)))
        {
            Debug.Log("Not a Valid Move");
            foreach (byte b in SBF.encryptMove(cards))
            {
                Debug.Log(b + " :");
            }

            return;
        }
        


        //Sees if the cards have a higher value then the cards on the table
        if (SBF.tablePile.Count > 0 && SBF.RawEval(cards) <= SBF.RawEval(SBF.tablePile))
        {
            Debug.Log("Invalid Action");
            return;
        }


        points += SBF.tablePile.Count;

        SBF.tablePile = cards;
        foreach (int card in cards) {
            hand.Remove(card);  
        }
        CalculatePossibleMoves();

    }


    /// <summary>
    /// Pick up 1 card from table pile
    /// </summary>
    /// <param name="top">If its the top or bottom card thats picked up</param>
    /// <param name="flip">To flip or not flip the card before putting it the hand</param>
    /// <param name="placement">Where in the hand the card is placed</param>
    public void PickUp(bool top, bool flip, int placement)
    {

        int card;

        //Looks if card exists on the top or bottom of middle stack
        if (top)
        {
            card = SBF.tablePile[SBF.tablePile.Count - 1];
        }
        else
        {
            card = SBF.tablePile[0];
        }

        if (flip) 
        {
            hand.Insert(placement, SBF.FlipCard(card));
        }
        else 
        {
            hand.Insert(placement, card);
        }

        CalculatePossibleMoves();

        //Gives a point to the person that laid down that card
        SBF.players[SBF.tablePileHolder].points++;

        SBF.tablePile.Remove(card);

    }


    //Finds all possible combinations of cards the player can put
    //Encrypts these moves to byte[] form and adds the to the moves list
    public void CalculatePossibleMoves()
    {

        moves.Clear();

        //Adds all possible moves in List<int> form
        List<List<int>> DecryptedMoves = new List<List<int>>();
        for (int i = 0; i < hand.Count; i++) { 
            List<int> temp = new List<int>() { hand[i] };
            DecryptedMoves.Add(temp);

            for (int j = 1; j < hand.Count - i; j++) 
            { 

                if (SBF.getCardValue(hand[i]) == SBF.getCardValue(hand[i + j]))
                {
                    temp.Add(hand[i + j]);
                    DecryptedMoves.Add(temp);
                }
                else
                {
                    break;
                }
            }
            for (int j = 1; j < hand.Count - i; j++)
            {

                if (SBF.getCardValue(hand[i]) == SBF.getCardValue(hand[i + j]) + j)
                {
                    temp.Add(hand[i + j]);
                    DecryptedMoves.Add(temp);
                }
                else
                {
                    break;
                }
            }
            for (int j = 1; j < hand.Count - i; j++)
            {

                if (SBF.getCardValue(hand[i]) == SBF.getCardValue(hand[i + j]) - j)
                {
                    temp.Add(hand[i + j]);
                    DecryptedMoves.Add(temp);
                }
                else
                {
                    break;
                }
            }
        }



        //Compress moves from list<int> into byte[] form for optimization
        foreach (List<int> move in DecryptedMoves)
        {

            moves.Add(SBF.encryptMove(move));


            foreach (byte b in SBF.encryptMove(move))
            {
                Debug.Log(b);
            }
            Debug.Log("///////////////////////////////");

        }
    }


}
