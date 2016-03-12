using UnityEngine;

public abstract class Utils
{
    public static bool IsColorLike(Color colorA, Color colorB, float tolerancePerc = 0.01f)
    {
        var r = Mathf.Abs(colorA.r - colorB.r);
        if (r > tolerancePerc)
            return false;

        var g = Mathf.Abs(colorA.g - colorB.g);
        if (g > tolerancePerc)
            return false;

        var b = Mathf.Abs(colorA.b - colorB.b);
        if (b > tolerancePerc)
            return false;

        var a = Mathf.Abs(colorA.a - colorB.a);
        if (a > tolerancePerc)
            return false;

        return true;
    }

}
