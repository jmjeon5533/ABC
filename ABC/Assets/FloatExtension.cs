public static class FloatExtension
{
    static float Mod(float x, float n)
    {
        if (x < 0)
            return -Mod(-x, n);
        if (n < 0)
            return Mod(x, -n);

        float m = x / n;
        return x - m * n;
    }

    static float Normalize(float left, float right, float x)
    {
        float period = right - left;
        // float temp = x % period;
        float temp = Mod(x, period); // reinvent the wheel!
        if (temp < left)
            return temp + period;
        if (temp >= right)
            return temp - period;
        return temp;
    }

    public static bool InRange(this float x, float start, float end)
    {
        start = Normalize(0, 360, start);
        end = Normalize(0, 360, end);
        x = Normalize(0, 360, x);

        if (start <= end)
            return start <= x && x <= end;

        else
            return !(end < x && x < start);
    }
}