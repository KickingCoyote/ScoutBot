using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] 
    private Settings settings;

    [SerializeField] 
    private TMP_InputField moveDurationField;
    [SerializeField] 
    private TMP_InputField moveDepthField;
    [SerializeField] 
    private TMP_InputField gameSeedField;
    [SerializeField] 
    private Toggle randomSeedToggle;

    private float moveDuration;
    private int moveDepth;
    private int gameSeed;
    private bool randomizeSeed;

    private void Awake()
    {
        Load();  
    }

    private bool TryParseSettigns()
    {
        randomizeSeed = randomSeedToggle.isOn;

        return float.TryParse(moveDurationField.text, out moveDuration) &&
               int.TryParse(moveDepthField.text, out moveDepth) &&
               int.TryParse(gameSeedField.text, out gameSeed);
    }

    public void Save()
    {
        if (!TryParseSettigns())
        {
            throw new System.Exception();
        }

        settings.MaxMoveDuration = moveDuration;
        settings.MaxSearchDepth = moveDepth;
        settings.GameSeed = gameSeed;
        settings.randomizeSeed = randomizeSeed;

        ReturnToGame();
    }

    private void Load()
    {
        moveDurationField.text = (settings.MaxMoveDuration.ToString());
        moveDepthField.text = (settings.MaxSearchDepth.ToString());
        gameSeedField.text = (settings.GameSeed.ToString());

        randomSeedToggle.isOn = settings.randomizeSeed;
    }


    public void ReturnToGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
