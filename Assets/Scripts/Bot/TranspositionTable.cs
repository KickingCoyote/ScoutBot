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

    public static ulong HashGameState(GameState g)
    {
        ulong h = 0;

        h ^= (ulong)turnHash[g.turn - 1];

        for (int x = 0; x < 44; x++)
        {
            h ^= (ulong)hashKey[x, g.cards[x]];
        }
        return h;
    }

    public static Transposition TryGetTransposition(GameState g)
    {
        ulong hash = HashGameState(g);

        if (table.TryGetValue(hash, out Transposition result))
        {
            return result;
        }
        return new Transposition { isEmpty = true };
    }

    public static TranspositonData TryGetTransposition(GameState g, Move m)
    {
        Transposition t = TryGetTransposition(g);

        if (t.isEmpty) { return new TranspositonData { isEmpty = true }; }

        return t.TryGetPosition(m);
    }


    public static void AddTransposition(GameState g, int eval, int depth, int depthFromRoot, Quality quality = Quality.Low)
    {
        const int maxTableSize = 16000000;
        ulong hash = HashGameState(g);
        if (table.Count > maxTableSize)
        {
            table.Remove(table.ElementAt(0).Key);
        }

        Transposition transpositionData = new Transposition(depth, depthFromRoot, hash);

        if (!table.TryAdd(hash, transpositionData))
        {

            //If this runs it means that the same hash has been generated for 2 diffrent gamestates which never should happen as the probablility is less than 1 in 17 quadrillion
            //if (transpositionData.depth == table[hash].depth) { Debug.Log("Error: Colliding hashes"); }

            table[hash] = transpositionData;
        }

    }



}
public struct Transposition
{
    public bool isEmpty;

    private readonly ulong hash;

    public readonly int depth;
    public readonly int depthFromRoot;
    private Dictionary<ulong, TranspositonData> positions;


    public Transposition(int depth, int depthFromRoot, ulong hash)
    {
        this.depth = depth;
        this.depthFromRoot = depthFromRoot;
        this.hash = hash;
        positions = new Dictionary<ulong, TranspositonData>();
        isEmpty = false;
    }


    public ulong HashMove(Move m)
    {
        ulong h = 0;

        foreach (int i in m.cardDif)
        {
            h ^= (ulong)i ^ hash;
        }

        return h;
    }

    public void AddPosition(Move m, int evaluation, Quality quality)
    {
        ulong h = HashMove(m);
        if (!positions.TryAdd(h, new TranspositonData(evaluation, quality)))
        {
            positions[h] = new TranspositonData(evaluation, quality);
        }
    }

    public TranspositonData TryGetPosition(Move m)
    {
        ulong h = HashMove(m);
        if (positions.TryGetValue(hash, out TranspositonData result))
        {
            return result;
        }
        return new TranspositonData { isEmpty = true };
    }

}


public struct TranspositonData
{
    public bool isEmpty;

    public int evaluation;
    public Quality quality;

    public TranspositonData(int evaluation, Quality quality)
    {
        this.evaluation = evaluation;
        this.quality = quality;
        isEmpty = false;
    }
}

public enum Quality
{
    Low,
    High
}