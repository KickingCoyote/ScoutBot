using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player
{


    //The hand of the player
    public List<int> hand;

    //The points
    int points;

    //All possible combinations of cards the player can lay out
    public List<byte> moves;

    public Player(List<int> hand)
    {
        this.hand = hand;
        points = 0;
        moves = new List<byte>();
        CalculatePossibleMoves();
    }


    /*
     Takes cards from hand and puts them in middle pile
     */
    public void LayDownCards(List<int> cards)
    {
        
        //Sees if the cards have a higher value then the cards on the table
        if (SBF.tablePile.Count > 0 && SBF.RawEval(SBF.MoveEncrypter(cards)) <= SBF.RawEval(SBF.MoveEncrypter(SBF.tablePile)))
        {
            Debug.Log("Invalid Action");
            return;
        }

        SBF.tablePile = cards;
        foreach (int card in cards) {
            hand.Remove(card);  
        }
        CalculatePossibleMoves();

    }


    //Pick Up 1 card from middle pile
    public void PickUp(int card, bool flip, int placement)
    {
        //Looks if card exists on the top / bottom of middle stack
        if (!(SBF.tablePile[0] == card && SBF.tablePile[SBF.tablePile.Count - 1] == card))
        {
            Debug.Log("Invalid Action");
            return;
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
        SBF.tablePile.Remove(card);

    }


    //Finds all possible combinations of cards the player can put
    //Encrypts these moves to byte form and adds the to the moves list
    public void CalculatePossibleMoves()
    {

        moves.Clear();

        List<List<int>> DecryptedMoves = new List<List<int>>();


        for (int i = 0; i < hand.Count; i++) { 
            List<int> temp = new List<int>() { hand[i] };
            DecryptedMoves.Add(temp);

            for (int j = 1; j < hand.Count - i; j++) 
            { 

                if (SBF.CardToValue(hand[i]) == SBF.CardToValue(hand[i + j]))
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

                if (SBF.CardToValue(hand[i]) == SBF.CardToValue(hand[i + j]) + j)
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

                if (SBF.CardToValue(hand[i]) == SBF.CardToValue(hand[i + j]) - j)
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

        //Compress moves into byte form for optimization
        foreach (List<int> move in DecryptedMoves) {
            moves.Add(SBF.MoveEncrypter(move));
        }
    }


}
