using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;


[Serializable]
public class HeuristicMenu
{
    [SerializeReference]
    public SBH[] heuristics = new SBH[4] {null, null, null, null};



    public SBH GetHeuristic(int player)
    {
        return heuristics[player - 1];
    }

    public void SetHeuristic(int player, SBH heuristic)
    {
        heuristics[player - 1] = heuristic;
    }

}
