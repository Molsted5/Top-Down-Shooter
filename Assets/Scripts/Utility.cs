using UnityEngine;

public static class Utility {

    public static T[] ShuffleArray<T>( T[] array, int seed ) {
        System.Random prng = new System.Random( seed );

        for( int i = 0; i < array.Length - 1; i++ ) { // -1 because we can ignore the last random card swap, because there is only 1 card left
            int randomIndex = prng.Next( i, array.Length );
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

    public static Color HSVLerp(Color a, Color b, float t) {
        Color.RGBToHSV(a, out float h1, out float s1, out float v1);
        Color.RGBToHSV(b, out float h2, out float s2, out float v2);

        // Hue wrapping
        if (Mathf.Abs(h1 - h2) > 0.5f)
        {
            if (h1 > h2) h2 += 1f;
            else h1 += 1f;
        }

        float h = Mathf.Lerp(h1, h2, t) % 1f;
        float s = Mathf.Lerp(s1, s2, t);
        float v = Mathf.Lerp(v1, v2, t);

        return Color.HSVToRGB(h, s, v);
    }

}
