using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class CardBehavior : MonoBehaviour
{


    public static List<string> colors = new List<string>() {
        "#6C8DFC", "#0090E3", "#00C6F5", "#009D9F", "#35D557", "#B5F145", "#FFE800", "#FFD52C", "#FFAF36", "#FF566E"
    };

    private Color upperColor;
    private Color lowerColor;

    private Color unselectedColor = new Color(255, 255, 255);
    [SerializeField] private Color selectedColor;

    [SerializeField] bool selected = false;
    [SerializeField] int upperDigit;
    [SerializeField] int lowerDigit;

    [SerializeField] Image upperImage;
    [SerializeField] Image lowerImage;
    [SerializeField] Image frameImage;

    [SerializeField] TextMeshProUGUI upperText;
    [SerializeField] TextMeshProUGUI lowerText;



    // Update is called once per frame
    void Update()
    {
        if (upperDigit > 0 && upperDigit <= 10) { ColorUtility.TryParseHtmlString(colors[upperDigit - 1], out upperColor); }
        if (lowerDigit > 0 && lowerDigit <= 10) { ColorUtility.TryParseHtmlString(colors[lowerDigit - 1], out lowerColor); }

        frameImage.color = selected ? selectedColor : unselectedColor;

        
        upperImage.color = upperColor;
        lowerImage.color = lowerColor;

        //upperText.text = upperDigit.ToString();
        lowerText.text = lowerDigit.ToString();
    }
}
