using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GUIManager : MonoBehaviour
{

    List<GameObject>[] cardObjects = new List<GameObject>[5];

    public List<int> selectedCardIndexes = new List<int>();
    [HideInInspector] public List<GameObject> selectedCards = new List<GameObject>();


    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject parentObject;

    void Awake()
    {
        for (int i = 0; i < cardObjects.Length; i++)
        {
            cardObjects[i] = new List<GameObject>();
        }
    }

    public void CreateCards()
    {
        if (cardPrefab == null || parentObject == null) { return; }

        int[] cards = SBU.gameState.cards;

        for (int i = 0; i < cards.Length; i++)
        {
            int owner = SBU.getCardOwner(SBU.gameState.cards[i]);
            if (SBU.getCardHandIndex(SBU.gameState.cards[i]) == 15)
            {
                continue;
            }
            Transform parent = parentObject.transform.GetChild(owner);
            GameObject card = Instantiate(cardPrefab, 
                parent.position, 
                Quaternion.identity, 
                parent
            );
            card.name = i.ToString();

            CardBehavior cardBehavior = card.GetComponent<CardBehavior>();
            cardBehavior.upperDigit = SBU.getCurrentCardValue(SBU.getValueOfCard(cards, i));
            cardBehavior.lowerDigit = SBU.getValueOfCard(cards, i) - (16 * SBU.getCurrentCardValue(SBU.getValueOfCard(cards, i)));

            cardObjects[owner].Add(card);

        }

        for (int i = 0; i < cardObjects.Length; i++)
        {
            cardObjects[i].Sort((a, b) => handIndex(a).CompareTo(handIndex(b)));
            int o = 0;
            foreach (GameObject card in cardObjects[i])
            {
                card.transform.SetSiblingIndex(o);
                card.transform.localPosition = new Vector3(o * 40, 0, 0);
                o++;
            }
        }
        
    }

    public void selectCard(GameObject card)
    {

        int inverter = -1;
        if (card.GetComponent<CardBehavior>().selected) 
        {
            inverter = 1;
            selectedCardIndexes.Add(int.Parse(card.name));
            selectedCards.Add(card);
        }
        else 
        { 
            selectedCardIndexes.Remove(int.Parse(card.name)); 
            selectedCards.Remove(card);
        }

        int cardIndex = -1;
        List<GameObject> cards = new List<GameObject>();
        for (int i = 0; i < cardObjects.Length; i++)
        {
            cardIndex = cardObjects[i].IndexOf(card);

            if (cardIndex != -1) { cards = cardObjects[i]; break; }
        }


        ScaleCard(card, inverter * 0.16f);

        //First card / previous card is selected => only shift other
        if (cardIndex != 0 && !cards[cardIndex - 1].GetComponent<CardBehavior>().selected) { ShiftCards(cards, cardIndex, inverter * 25); }

        //Last card / next card is selected => only shift self
        if (cardIndex != cards.Count - 1 && !cards[cardIndex + 1].GetComponent<CardBehavior>().selected) { ShiftCards(cards, cardIndex + 1, inverter * 25); }

    }

    public void ScaleCard(GameObject card, float scale)
    {
        card.transform.localScale = new Vector3(card.transform.localScale.x + scale, card.transform.localScale.y + scale, 1);
    }


    public void ShiftCards(List<GameObject> cards, int index, int shift)
    {
        for (int i = index; i < cards.Count; i++)
        {
            cards[i].transform.localPosition = new Vector3(cards[i].transform.localPosition.x + shift, 0, 0);
        }
    }

    public void DeleteCards()
    {
        foreach (List<GameObject> hand in cardObjects)
        {
            while (hand.Count > 0)
            {
                Destroy(hand.ElementAt(0));
                hand.RemoveAt(0);
            }
        }
        selectedCardIndexes.Clear();
        selectedCards.Clear();
    }


    private int handIndex(GameObject obj)
    {
        int cardIndex;
        if (!int.TryParse(obj.name, out cardIndex)) { Debug.Log("Invalid Card Name");  return 0; }

        return SBU.getCardHandIndex(SBU.gameState.cards[cardIndex]);
    }

}
