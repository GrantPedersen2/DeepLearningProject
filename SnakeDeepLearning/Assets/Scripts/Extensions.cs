using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static bool Contains(this Bounds container, Bounds other)
    {
        return container.min.x <= other.min.x
            && container.min.y <= other.min.y
            && container.max.x >= other.max.x
            && container.max.y >= other.max.y;
    }
	
}
