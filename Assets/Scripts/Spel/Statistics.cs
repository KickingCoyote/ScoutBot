using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Statistics
{
    public static void StoreData(string[] playerIdentification, List<Move> moves, int winningPlayer, GameState EndGameState, int seed, float time, string extraInfo)
    {
        string path = Application.dataPath + "/Statistics";
        
        int fileNumber = Directory.GetFiles(path).Length/2+1;
        
        string textFilePath = path + "/" + fileNumber.ToString() + ".txt";

        string playerIDString = "";
        foreach (string playerId in playerIdentification){
            playerIDString += playerId + ". ";
        }

        

        string textData = time + "\n" + seed + "\n" + playerIDString + "\n";
        
        

        if (!File.Exists(path))
        {
            FileStream stream = null;
            stream = new FileStream(textFilePath, FileMode.OpenOrCreate);
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine("Extra Information: " + extraInfo);
                writer.WriteLine("Time: " + time);
                writer.WriteLine("Seed: " + seed);
                writer.WriteLine("PlayerID: " + playerIDString + "\n");

                AddRoundsDataToTextfile(writer, playerIdentification, moves, EndGameState);
                writer.Close();
            }
            

        }

    }

    public static void AddRoundsDataToTextfile(StreamWriter writer, string[] playerIdentification, List<Move> moves, GameState EndGameState, bool gameOver = true)
    {

        GameState currentState = EndGameState;
        for (int i = 1; i < moves.Count + 2; i++)
        {
            int turn = currentState.turn;
            turn = turn == 1 ? 4 : (turn - 1);
            int playerPoints1 = currentState.playerPoints[0];
            int playerPoints2 = currentState.playerPoints[1];
            int playerPoints3 = currentState.playerPoints[2];
            int playerPoints4 = currentState.playerPoints[3];

            int x = 1;
            int[] playerPoints = new int[4];
            foreach (int point in currentState.playerPoints)
            {
               // playerPoints[x] = po
            }

           

            writer.WriteLine("");
            writer.WriteLine("Move: " + (moves.Count - i+1));
            writer.WriteLine("Turn: " + turn);
            writer.WriteLine("Points: " + playerPoints1 + "," + playerPoints2 + "," + playerPoints3 + "," + playerPoints4);
            writer.WriteLine("Pileholder: " + currentState.currentPileHolder);

            for (int v = 1; v < 5; v++)
            {
                string playerHand = "";
                int[] playerHandArray = currentState.getPlayerCards(v);
                foreach (int b in playerHandArray)
                {
                    if (b != -10)
                    {
                        playerHand += SBU.CardToString(currentState.cards, b) + ", ";
                    }
                }
                writer.WriteLine("Hand" + v + " :" + playerHand);
                if (i == 1 && gameOver)
                {
                    currentState.playerPoints[v-1] += playerHandArray.ArrayLength();

                }
            }
            
            //Debug.Log("Current players turn: " + currentState.turn + " || Player 1s points: " + currentState.playerPoints[0]);
            if (i < moves.Count+1)
            {
                currentState.UndoMove(moves[moves.Count - i]);
            }
        }
    }
}
