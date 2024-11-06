using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// A class for General Extensions / Utilities for dealing with arrays
/// </summary>
public static class ArrayExtensions
{


    /// <summary>
    /// Fills an array with a value
    /// </summary>
    /// <param name="array"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int[] SetArray(this int[] array, int value)
    {
        int[] a = new int[array.Length];

        Array.Fill(a, value);
        return a;
    }


    /// <summary>
    /// Checks how long an array is when removing the -10s at the end
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static int ArrayLength(this int[] a)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] < 0) { return i; }
        }
        return a.Length;
    }


    /// <summary>
    /// checks if a list of arrays contains a array
    /// </summary>
    /// <param name="a">array of arrays</param>
    /// <param name="b">array</param>
    /// <returns></returns>

    public static bool ContainsArray(this List<int[]> a, int[] b)
    {

        for (int i = 0; i < a.Count; i++)
        {
            if (CompareArray(a.ElementAt(i), b))
            {
                return true;
            }
        }

        return false;
    }



    /// <summary>
    /// Add two arrays together, if invertB then subtract b from a
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="invertB">if true, does a - b instead</param>
    /// <returns></returns>
    public static int[] AddArray(int[] a, int[] b, bool invertB = false)
    {

        int[] s = new int[a.Length];

        int inverter = invertB ? -1 : 1;

        for (int i = 0; i < a.Length; i++)
        {
            s[i] = a[i] + b[i] * inverter;
        }

        return s;
    }


    //compare two integer arrays by value 
    /// <summary>
    /// Checks if all values in 2 arrays are the same
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool CompareArray(int[] a, int[] b)
    {
        return Enumerable.SequenceEqual(a, b);
    }




    /// <summary>
    /// Copy a array by value
    /// </summary>
    /// <param name="a">Array to be copied</param>
    /// <returns>Array copied by value</returns>
    public static int[] CopyArray(this int[] a)
    {
        int[] b = new int[a.Length];

        Array.Copy(a, b, a.Length);
        return b;

    }

}
