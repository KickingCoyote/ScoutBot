using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Everymove : MonoBehaviour
{

    int[] everyMove = new int[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 100, 101, 102, 103, 104, 105, 106, 107, 108, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 200, 201, 202, 203, 204, 205, 206, 207, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 300, 301, 302, 303, 304, 305, 306, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 400, 401, 402, 403, 404, 405, 410, 411, 412, 413, 414, 415, 416, 417, 418, 419, 500, 501, 502, 503, 504, 510, 511, 512, 513, 514, 515, 516, 517, 518, 519, 600, 601, 602, 603, 610, 611, 612, 613, 614, 615, 616, 617, 618, 619, 700, 701, 702, 710, 711, 712, 713, 714, 715, 716, 717, 718, 719, 800, 801, 810, 811, 812, 813, 814, 815, 816, 817, 818, 819, 900 };


    // Start is called before the first frame update
    void Start()
    {
        int count = 0;
        List<int> everymove = new List<int>();
        string alladrag = "";
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
                        everymove.Add((i-1)*100 + u*10 + v-1);
                        Debug.Log(everymove[count]);
                        alladrag = alladrag + ", " + everymove[count].ToString();
                        count++;
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
                            everymove.Add((i - 1) * 100 + u * 10 + v - 1);
                            Debug.Log(everymove[count]);
                            alladrag = alladrag + ", " + everymove[count].ToString();
                            count++;
                        }
                    }
                }
            }
        }
        Debug.Log(alladrag);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
