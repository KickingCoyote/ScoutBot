using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Everymove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i <= 10; i++)
        {
            for (int u = 0; u <= 1; u++)
            {
                if (i == 1)
                {
                    u = 1;
                }
                if (u == 0)
                {
                    for (int v = 1; v <= (10-(i-1)); v++)
                    {
                        Debug.Log((i-1) + " " + u + " " + (v-1));
                    }
                }
                else
                {
                    for (int v = 1; v <= 10; v++)
                    {
                        if (i == 10 && u == 1)
                        {
                            break;
                        }
                        else
                        {
                            Debug.Log((i - 1) + " " + u + " " + (v - 1));
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
