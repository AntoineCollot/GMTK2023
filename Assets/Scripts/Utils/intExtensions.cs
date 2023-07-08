using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class intExtensions
{
    public static int AddOrMultiply(this int source, int mult)
    {
        if (source == 0)
            return mult;

        return source * mult;
    }
}
