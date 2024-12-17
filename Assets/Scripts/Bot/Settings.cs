using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    /// <summary>
    /// How many moves into the future that are looked at.
    /// </summary>
    public int MaxSearchDepth = 1;

    /// <summary>
    /// A float that determines how it should alocate extra threat weight to the oponents depending on how well they are doing, 1 means no extra weight, higher number means more
    /// </summary>
    public int SimulationCount = 1;

    /// <summary>
    /// The seed for how the cards are distributed on game start, if left empty all cards are distributed randomly.
    /// </summary>
    public int GameSeed = 0;

    /// <summary>
    /// How long a move is allowed to take in seconds.
    /// </summary>
    public float MaxMoveDuration = 0.1f;


    public HeuristicMenu Heuristics;
}
