using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class GUIManager : MonoBehaviour
{

    List<GameObject>[] cardObjects = new List<GameObject>[5];

    public List<int> selectedCards = new List<int>();

    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject parentObject;

    // Start is called before the first frame update
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
        int cardIndex = -1;
        int cardOwner = -1;
        for (int i = 0; i < cardObjects.Length; i++)
        {
            cardIndex = cardObjects[i].IndexOf(card);
            if (cardIndex != -1)
            {
                cardOwner = i;
                break;
            }
        }

        //Code for shifting cards around when they are selected
        int shift = 50;
        int inverter = -1;

        if (card.GetComponent<CardBehavior>().selected) 
        { 
            inverter = 1;
            selectedCards.Add(int.Parse(card.name));

        }
        else
        {
            selectedCards.Remove(int.Parse(card.name));
        }
        card.transform.localScale = new Vector3(card.transform.localScale.x + (inverter * 0.16f), card.transform.localScale.y + (inverter * 0.16f), 1);
        int k = 0;
        foreach (GameObject obj in cardObjects[cardOwner])
        {
          
            if (k == cardIndex && k != 0)
            {
                if (cardObjects[cardOwner][k - 1].GetComponent<CardBehavior>().selected) { shift -= 25; }

                else { card.transform.localPosition = new Vector3(card.transform.localPosition.x + (inverter * 25), 0, 0); }
            }

            if (k == cardIndex && k == 0) { shift -= 25; }

            if (k == cardIndex + 1 && obj.GetComponent<CardBehavior>().selected) { shift -= 25; }


            if (k > cardIndex)
            {
                obj.transform.localPosition = new Vector3(obj.transform.localPosition.x + inverter * shift, 0, 0);
            }
            k++;
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
        selectedCards.Clear();
    }


    private int handIndex(GameObject obj)
    {
        int cardIndex;
        if (!int.TryParse(obj.name, out cardIndex)) { Debug.Log("Invalid Card Name");  return 0; }

        return SBU.getCardHandIndex(SBU.gameState.cards[cardIndex]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
