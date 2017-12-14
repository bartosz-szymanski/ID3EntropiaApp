using System;

namespace EntropiaApp.Utils
{
    public static class MathExtension
    {
        public static double GetBinaryLogaritm(double occurences, int decisions) => occurences / decisions * Math.Log(occurences / decisions, 2);
    }
}