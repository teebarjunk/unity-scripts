using System;

public static class ArrayHelper
{
    // Returns a sub array.
    public static T[] SubArray<T>(T[] source, int start, int end = -1)
    {
        // If no limit given, take all.
        if (end == -1)
            end = source.Length - 1;

        int count = end - start + 1;

        // Get sub array.
        T[] subarray = new T[count];
        Array.Copy(source, start, subarray, 0, count);

        return subarray;
    }
}
