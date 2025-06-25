
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

}
