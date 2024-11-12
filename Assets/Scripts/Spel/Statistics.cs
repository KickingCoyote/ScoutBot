using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Statistics
{
    public static void StoreData(string[] playerIdentification, List<Move> moves, int winningPlayer, GameState EndGameState, int seed, float time)
    {
        string path = Application.dataPath + "/Statistics";
        int fileNumber = Directory.GetFiles(path).Length/2+1;
        string textFile = path + "/" + fileNumber.ToString();
        string playerIDString = "";
        foreach (string playerId in playerIdentification){
            playerIDString += playerId + ". ";
        }
        string textData = time + "\n" + seed + "\n" + playerIDString + "\n";

        if (!File.Exists(path))
        {
            File.WriteAllText(textFile, textData);
        }

    }
}
