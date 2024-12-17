using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public struct GameState
{
    public int[] cards;

    public int turn;

    public int round;

    public int currentPileHolder;

    private int[][] playerCards;

    public int[] playerPoints;

    /// <summary>
    /// When checking player cards for the first time store them in playerCards and on cosecutive checks retreive the data from the variable
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public int[] getPlayerCards(int player) { return playerCards[player] = playerCards[player] == null ? getPlayerCards(cards, player) : playerCards[player]; }

    public int getPlayerPoints(int player) { return playerPoints[player - 1]; }

    public GameState(int[] cards, int turn, int currentPileHolder)
    {
        this.cards = cards;
        this.turn = turn;
        this.currentPileHolder = currentPileHolder;
        playerCards = new int[][] { null, null, null, null, null };
        playerPoints = new int[4];
        round = 1;
    }


    public void DoMove(Move move)
    {
        if (move?.cardDif == null)
        {
            playerPoints[turn - 1] -= 1;
        }
        else
        {
            if (!move.isDrawMove) { playerPoints[turn - 1] += getPlayerCards(cards, 0).ArrayLength(); }
            else { playerPoints[currentPileHolder - 1]++; }

            cards = ArrayExtensions.AddArray(cards, move.cardDif);
            currentPileHolder += move.pileHolderDif;

        }

        //Reset the playerCards data for the current player and middle pile
        playerCards = new int[][] { null, null, null, null, null };

        //Increment the turn by 1, if 4 set to 1
        turn = turn == 4 ? 1 : (turn + 1);

        if (turn == 1) { round++; }
    }

    public void UndoMove(Move move)
    {
        if (move?.cardDif == null)
        {
            playerCards = new int[][] { null, null, null, null, null };

            if (turn == 1) { round--; }

            turn = turn == 1 ? 4 : (turn - 1);

            playerPoints[turn - 1] += 1;

            return;
        }

        cards = ArrayExtensions.AddArray(cards, move.cardDif, true);
        currentPileHolder -= move.pileHolderDif;
        playerCards = new int[][] { null, null, null, null, null };


        if (turn == 1) { round--; }

        turn = turn == 1 ? 4 : (turn - 1);

        if (!move.isDrawMove) { playerPoints[turn - 1] -= getPlayerCards(cards, 0).ArrayLength(); }
        else { playerPoints[currentPileHolder - 1]--; }

    }


    /// <summary>
    /// Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="player"></param>
    /// <returns>A 15 long array of card indexes where emtpy values are -10</returns>
    public static int[] getPlayerCards(int[] cards, int player)
    {

        int[] pCards = new int[15].SetArray(-10);

        for (int i = 0; i < cards.Length; i++)
        {
            //HandIndex 15 represents the card being a point and not actually in the hand
            if (SBU.getCardOwner(cards[i]) == player && SBU.getCardHandIndex(cards[i]) != 15)
            {
                pCards[SBU.getCardHandIndex(cards[i])] = i;
            }

        }

        return pCards;
    }

    public int getWinningPlayer()
    {
        int[] points = new int[4];
        for (int i = 0; i < 4; i++)
        {
            points[i] += playerPoints[i] - getPlayerCards(i + 1).ArrayLength();
        }

        return Array.IndexOf(points, Enumerable.Max(points)) + 1;
    }



    /// <summary>
    /// Function that checks if it is game over or not
    /// </summary>
    public bool isGameOver()
    {
        if (currentPileHolder == turn)
        {
            return true;
        }
        //check previous player due to game over being check post turn incrementing
        if (getPlayerCards(turn == 1 ? 4 : (turn - 1)).ArrayLength() == 0)
        {
            return true;
        }

        return false;
    }


    public int EstimatePossibleMoveScore(int player)
    {
        //Adds the players card into an array sorted like it is in the players hand, all empty spots are -10
        int[] pCards = getPlayerCards(player);

        int tCardsValue = Move.getValue(cards, getPlayerCards(0));

        //Get a estimate score for a hand based on how many moves can be done and the length of those moves, this value has no relation to other values
        int score = 0;

        for (int i = 0; i < pCards.Length && pCards[i] != -10; i++)
        {
            int currentCardValue = SBU.getCurrentCardValue(cards, pCards[i]);
            score += 10;

            for (int h = -1; h < 2; h++)
            {
                for (int j = 1; j < pCards.Length - 1; j++)
                {
                    if (i + j >= pCards.Length || pCards[i + j] == -10) { break; }

                    if (currentCardValue != SBU.getCurrentCardValue(cards, pCards[i + j]) + j * h) { break; }

                    score += 10 * (j + 1) + (h == 0 ? 1 : 0);
                }
            }
        }
        return score;
    }

}
