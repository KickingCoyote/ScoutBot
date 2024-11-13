using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class DragCards : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    private GUIManager manager;
    private GameBase gameBase;
    private Transform selectedCardsObj;
    private Transform previousParent;
    private int previousHandIndex;
    private bool isDragged;
    private bool isFlipped;

    void Awake()
    {
        
        selectedCardsObj = transform.parent.parent.GetChild(5);
        manager = GameObject.Find("GameManager").GetComponent<GUIManager>();
        gameBase = GameObject.Find("GameManager").GetComponent<GameBase>();

        isDragged = false;
        isFlipped = false;
        previousHandIndex = -1;
       
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!transform.GetComponent<CardBehavior>().selected) {  return; }

        isDragged = true;

        selectedCardsObj.position = transform.position;

        previousParent = transform.parent;

        manager.selectedCards.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        previousHandIndex = manager.selectedCards[0].transform.GetSiblingIndex();

        foreach (GameObject card in manager.selectedCards)
        {
            card.transform.SetParent(selectedCardsObj);

        }
        manager.SetRaycastTransparencyForAll(false);
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!transform.GetComponent<CardBehavior>().selected) { return; }

        selectedCardsObj.position = Input.mousePosition;

    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (!transform.GetComponent<CardBehavior>().selected) { return; }

        isDragged = false;

        int i = 0;

        bool cancel = true;


        //the play hand which was hoverd over when the cards where released
        Transform playerHand = null;
        if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.transform.CompareTag("CardBackground"))
        {

            cancel = false;

            //the card we are hovering over
            playerHand = eventData.pointerCurrentRaycast.gameObject.transform;

            //Take card move
            if (previousParent.GetSiblingIndex() == 0 && playerHand.GetSiblingIndex() == SBU.gameState.turn)
            {
                gameBase.TakeCard(
                    SBU.gameState.getPlayerCards(0)[0] != int.Parse(transform.name),
                    isFlipped,
                    0
                );

            }
            //Put card move
            else if (playerHand.GetSiblingIndex() == 0)
            {
                gameBase.PutCard();
            }
            else
            {
                cancel = true;
            }
        }


        if (cancel)
        {
            foreach (GameObject card in manager.selectedCards)
            {

                card.transform.SetParent(previousParent);
                card.transform.SetSiblingIndex(previousHandIndex + i);

                if (isFlipped)
                {
                    FlipCard(transform);
                }

                card.GetComponent<CardBehavior>().selected = false;

                manager.ScaleCard(card, -0.16f);

                i++;
            }
            manager.SetRaycastTransparencyForAll(true);
        }

      
        manager.selectedCards.Clear();

    }


    void Update()
    {
        if (isDragged && Input.GetMouseButtonDown(1) && previousParent.GetSiblingIndex() == 0)
        {
            FlipCard(transform);
        } 
    }

    private void FlipCard(Transform transform)
    {
        isFlipped = !isFlipped;
        CardBehavior cardBehavior = transform.GetComponent<CardBehavior>();
        int upperDigit = cardBehavior.upperDigit;
        cardBehavior.upperDigit = cardBehavior.lowerDigit;
        cardBehavior.lowerDigit = upperDigit;
    }
}
