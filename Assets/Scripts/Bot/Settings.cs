using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    /// <summary>
    /// How many moves into the future that are looked at.
    /// </summary>
    public int SearchDepth = 1;

    /// <summary>
    /// A float from 0 to 1 determining how worse draw moves are ordered in move ordering 
    /// </summary>
    public float DrawMoveTolerance = 1;

    /// <summary>
    /// The seed for how the cards are distributed on game start, if left empty all cards are distributed randomly.
    /// </summary>
    public int GameSeed = 0;
}
