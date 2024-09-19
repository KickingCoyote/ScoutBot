using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBase : MonoBehaviour
{
    void Start()
    {
        DistributeCards();
    }

    private void DistributeCards()
    {
        for (int i = 0; i < 44; i++)
        {
            SBU.cards[i] = i % 4 + 8 * Random.Range(0, 2) + 16 * (i / 4);
        }
        SBU.cards = SBU.ShuffleCards(SBU.cards);

    }


    private void GameUpdate()
    {
        SBU.turn++;
    }


}
