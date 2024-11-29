using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public static class TranspositionTable
{
    public static Dictionary<ulong, Transposition> table = new Dictionary<ulong, Transposition>();

    private static int[,] hashKey;
    private static int[] turnHash;

    public static void GenerateZobristHashKey()
    {
        hashKey = new int[44,256];
        turnHash = new int[4];
        System.Random rand = new System.Random();

        for (int i = 0; i < 4;  i++)
        {
            turnHash[i] = rand.Next(0, 256);
        }

        for (int x = 0; x < 44;  x++)
        {
            for (int y = 0; y < 256; y++)
            {
                hashKey[x, y] = rand.Next(0, 2147483647);
            }
        }
    }

    private static ulong HashMove(GameState g, Move m)
    {
        ulong h = 0;

        h ^= (ulong)turnHash[g.turn - 1];

        for (int x = 0; x < 44; x++)
        {
            h ^= (ulong)hashKey[x, g.cards[x]];
        }

        return h;
    }


    public static Transposition TryGetTransposition(GameState g, Move m)
    {
        ulong hash = HashMove(g, m);

        if (table.TryGetValue(hash, out Transposition result))
        {
            return result;
        }
        return new Transposition { isEmpty = true };
    }



    public static void AddTransposition(GameState g, Move m, int eval, int depth, int maxDepth, Quality quality = Quality.Low)
    {
        const int maxTableSize = 6400000;
        ulong hash = HashMove(g, m);
        if (table.Count > maxTableSize)
        {
            table.Remove(table.ElementAt(0).Key);
        }

        Transposition transposition = new Transposition(depth, maxDepth, eval, quality);

        if (!table.TryAdd(hash, transposition))
        {

            //If this runs it means that the same hash has been generated for 2 diffrent gamestates which never should happen as the probablility is miniscule
            if (transposition.maxDepth == table[hash].maxDepth) { Debug.Log($"Error: Colliding hashes {hash}" ); }

            table[hash] = transposition;
        }

    }



}
public struct Transposition
{
    public bool isEmpty;


    public readonly int depth;
    public readonly int maxDepth;
    public readonly int eval;
    public readonly Quality quality;

    public Transposition(int depth, int maxDepth, int eval, Quality quality)
    {
        this.depth = depth;
        this.maxDepth = maxDepth;
        this.eval = eval;
        this.quality = quality;
        isEmpty = false;
    }


}

public enum Quality
{
    Low,
    Medium,
    High
}