using System;
using System.Diagnostics;

// Enums used to create random arrays.
enum LevelOfSorting { Unsorted = 0, MostlySorted = 1, Sorted = 2, ReverseSorted = 3, ReverseMostlySorted = 4 }
enum KeyDistribution { FewUnique = 0, Many = 1, OnlySmall = 2, MostlySmall = 3 }
enum ArrayLength { Long = 0, Medium = 1, Short = 2 }

#region MAIN
class MainClass
{
    static void Main()
    {
        for (int bits = 4; bits < 26; bits++)
        {
            for (int sort = 0; sort < Enum.GetNames(typeof(LevelOfSorting)).Length; sort++)
            {
                for (int keys = 0; keys < Enum.GetNames(typeof(KeyDistribution)).Length; keys++)
                {
                    for (int length = 0; length < Enum.GetNames(typeof(ArrayLength)).Length; length++)
                    {
                        RunAlgorithms(
                            timesToTest: 10,
                            bits: 26,
                            levelOfSorting: (LevelOfSorting) sort,
                            keyDistribution: (KeyDistribution) keys,
                            length: (ArrayLength) length,
                            printArray: false);
                        Console.WriteLine("------------------");
                    }
                }
            }
        }

        /*
        // This is the pure skylinesort best case scenario.
        RunAlgorithms(
            timesToTest: 10,
            bits: 26,
            levelOfSorting: LevelOfSorting.Unsorted,
            keyDistribution: KeyDistribution.FewUnique,
            length: ArrayLength.Long,
            printArray: false);
        */
    }

    /// <summary>
    /// Tests all algorithms over the same input array.
    /// </summary>
    /// <param name="timesToTest"> How many times should we test each algorithm. The final measured time will be
    /// the smallest number of ticks from all of the tests. We take the smallest to avoid measuring possible extra ticks taken by the
    /// computer on other tasks. </param>
    /// <param name="bits"> How many bits maximum will the biggest number in the input array be using. </param>
    /// <param name="levelOfSorting"> How sorted should the input array be. </param>
    /// <param name="keyDistribution"> Are there many, few or only small numbers to sort (keys)? </param>
    /// <param name="length"> Should we test over a long, medium or short input array? </param>
    /// <param name="printArray"> If set to <c>true</c> print input array. </param>
    static void RunAlgorithms(int timesToTest, int bits, LevelOfSorting levelOfSorting, KeyDistribution keyDistribution, ArrayLength length, bool printArray = false)
    {
        uint[] vectorToOrder = NumberGenerator.GenerateArray(ref bits, levelOfSorting, keyDistribution, length, printArray);

        uint numberOfElements = (uint)vectorToOrder.Length;

        uint[] sortedVector = null;

        // Test Radix with Skylinesort
        RadixWithSkylinesort.Setup(bits, numberOfElements);
        Tester.SpeedTest(
            testName: "RadixWithSkyline",
            action: () =>
            {
                sortedVector = RadixWithSkylinesort.Sort(vectorToOrder);
            },
            times: timesToTest);
        Tester.TestIfSorted(sortedVector);

        // Test Radix with counting bits
        RadixWithCountingBits.Setup(bits, numberOfElements);
        Tester.SpeedTest(
            testName: "RadixWithCountingBits",
            action: () =>
            {
                sortedVector = RadixWithCountingBits.Sort(vectorToOrder);
            },
            times: timesToTest);
        Tester.TestIfSorted(sortedVector);

        // Test Skylinesort
        Skylinesort.Setup(bits, numberOfElements);
        Tester.SpeedTest(
            testName: "Skylinesort",
            action: () =>
            {
                sortedVector = Skylinesort.Sort(vectorToOrder);
            },
            times: timesToTest);
        Tester.TestIfSorted(sortedVector);

        // Test Countingsort
        Countingsort.Setup(bits, numberOfElements);
        Tester.SpeedTest(
            testName: "Countingsort",
            action: () =>
            {
                sortedVector = Countingsort.Sort(vectorToOrder);
            },
            times: timesToTest);
        Tester.TestIfSorted(sortedVector);

        // Test Quicksort
        int lastIndex = (int)(numberOfElements - 1u);
        // We need to clone the input vector before feeding it into quicksort because if we input
        // the original input vector it will come out sorted after the first test.
        uint[] vectorToOrderClone = vectorToOrder.Clone() as uint[];
        Tester.SpeedTest(
            testName: "Quicksort",
            action: () =>
            {
                Quicksort.Sort(vectorToOrderClone, 0, lastIndex);
            },
            times: timesToTest,
            inBetweenAction: () => vectorToOrderClone = vectorToOrder.Clone() as uint[]);
        Tester.TestIfSorted(sortedVector);
    }
}
#endregion

#region NUMBER_GENERATOR
/// <summary>
/// This is the class that creates the input arrays we will feed the different algorithms.
/// </summary>
class NumberGenerator
{
    /// <summary> Numbers with fewer bits than this value are considered small numbers. </summary>
    const int MAX_SMALL_NUMBER_BITS = 4;
    /// <summary> When we create a few unique keys, this is the number of keys we create. </summary>
    const int NUM_UNIQUE_KEYS = 600;
    /// <summary> This is what we consider to be many elements. </summary>
    const int MANY = 10000000;
    /// <summary> This is what we consider a normal amount of elements. </summary>
    const int NORMAL_AMOUNT = 4000;
    /// <summary> This is what we consider to be few elements. </summary>
    const int FEW = 10;
    /// <summary> When we have a mostly sorted array we perform this number of random swaps. </summary>
    const int MOSTLY_SORTED_RANDOM_SWAPS = 10;

    /// <summary> If you want to test modifications to the algorithms over the exact same input array use the same seed every time. </summary>
    const int SEED = 2;
    /// <summary> Don't feed the Random constructor the seed if you want different arrays given the same distribution parameters. </summary>
    static Random random = new Random(SEED);

    /// <summary>
    /// This is an array generator which should cover all of the sorting algorithms benchmark array distributions.
    /// </summary>
    /// <returns>The array.</returns>
    /// <param name="maxBits"> Indicates how large can the biggest number of the array be in bits. This is a ref
    /// parameter, since if the key distribution is "only small" we should truncate it. </param>
    /// <param name="levelOfSorting"> How sorted should the array be. </param>
    /// <param name="keyDistribution"> Are there many, few or only small keys? </param>
    /// <param name="arrayLength"> Should we make a long, medium or short array? </param>
    public static uint[] GenerateArray(ref int maxBits, LevelOfSorting levelOfSorting, KeyDistribution keyDistribution, ArrayLength arrayLength, bool print = false)
    {
        // We set the array length;
        int numberOfElements = -1;
        switch (arrayLength)
        {
            case ArrayLength.Long:
                numberOfElements = MANY;
                break;
            case ArrayLength.Medium:
                numberOfElements = NORMAL_AMOUNT;
                break;
            case ArrayLength.Short:
                numberOfElements = FEW;
                break;
        }

        // We change the way we generate numbers based on our key distribution.
        Func<uint> generateNumber = null;
        switch (keyDistribution)
        {
            case KeyDistribution.FewUnique:
                uint[] uniqueKeys = new uint[NUM_UNIQUE_KEYS];
                for (int i = 0; i < uniqueKeys.Length; i++)
                {
                    uniqueKeys[i] = (uint)random.Next(0, 1 << maxBits);
                }
                generateNumber = () => uniqueKeys[random.Next(0, uniqueKeys.Length)];
                break;
            case KeyDistribution.Many:
                // We need to copy the maxBits value into other variable because lambda
                // expressions can't use ref parameters.
                int mb = maxBits;
                generateNumber = () => (uint)random.Next(0, 1 << mb);
                break;
            case KeyDistribution.MostlySmall:
                // One of every 10 elements can be anywhere in the range
                int counter = 0;
                // We need to copy the maxBits value into other variable because lambda
                // expressions can't use ref parameters.
                int mbs = maxBits;
                generateNumber = () =>
                {
                    counter++;
                    return (uint)(counter % 10 == 0 ? random.Next(0, 1 << mbs) : random.Next(0, 1 << MAX_SMALL_NUMBER_BITS));
                };
                break;
            case KeyDistribution.OnlySmall:
                // If we are using only small numbers then we clamp the max bits we are using to
                // a maximum of MAX_SMALL_NUMBER_BITS.
                maxBits = Math.Min(maxBits, MAX_SMALL_NUMBER_BITS);
                // We need to copy the maxBits value into other variable because lambda
                // expressions can't use ref parameters.
                int mBs = maxBits;
                generateNumber = () => (uint)random.Next(0, 1 << mBs);
                break;
        }

        // We generate an array
        uint[] array = new uint[numberOfElements];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = generateNumber();
        }

        // We pre-sort our array based on the level of sorting
        switch (levelOfSorting)
        {
            case LevelOfSorting.MostlySorted:
                // We sort the array
                Skylinesort.Setup(maxBits, (uint)array.Length);// I had to choose one xD
                array = Skylinesort.Sort(array);
                // Then we randomly swap some elements
                for (int i = 0; i < MOSTLY_SORTED_RANDOM_SWAPS; i++)
                {
                    RandomSwap(ref array, random.Next(0, array.Length));
                }
                break;
            case LevelOfSorting.ReverseMostlySorted:
                // We reverse the array
                Array.Sort(array);
                Array.Reverse(array);
                // Then we randomly swap some elements
                for (int i = 0; i < MOSTLY_SORTED_RANDOM_SWAPS; i++)
                {
                    RandomSwap(ref array, random.Next(0, array.Length));
                }
                break;
            case LevelOfSorting.ReverseSorted:
                Array.Sort(array);
                Array.Reverse(array);
                break;
            case LevelOfSorting.Sorted:
                Skylinesort.Setup(maxBits, (uint)array.Length);
                array = Skylinesort.Sort(array);
                break;
            case LevelOfSorting.Unsorted:
                // We make sure to shuffle the elements in the array, because future
                // generateNumber functions may not necessarily give us elements
                // in a random order.
                for (int i = 0; i < array.Length; i++)
                {
                    RandomSwap(ref array, i);
                }
                break;
        }

        // We always print some basic information about the created array
        Console.Write("Created array of " + numberOfElements + " elements");
        Console.WriteLine(" in the range 0 to " + ((1 << maxBits) - 1).ToString());
        Console.WriteLine("Each element uses max " + maxBits + " bits.");
        Console.WriteLine("Level of sorting: " + levelOfSorting.ToString());
        Console.WriteLine("Key distribution: " + keyDistribution.ToString());
        Console.WriteLine("Array length: " + arrayLength.ToString());
        Console.WriteLine();

        // We print the array in the console if we are asked to
        if (print)
        {
            Console.WriteLine("Generated array:");
            foreach (var e in array)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();
        }

        // We return the array
        return array;
    }

    /// <summary>
    /// We swap the element at index for another random element in the array.
    /// </summary>
    static void RandomSwap(ref uint[] array, int index)
    {
        int secondIndex = random.Next(0, array.Length);
        uint temp = array[index];
        array[index] = array[secondIndex];
        array[secondIndex] = temp;
    }
}
#endregion

#region TESTER
class Tester
{
    public static void SpeedTest(string testName, Action action, int times, Action inBetweenAction = null)
    {
        Stopwatch watch = new Stopwatch();
        long minTicks = long.MaxValue;
        for (int i = 0; i < times; i++)
        {
            if (inBetweenAction != null) inBetweenAction();

            watch.Reset();
            watch.Start();

            action();

            watch.Stop();
            if (watch.ElapsedTicks < minTicks)
            {
                minTicks = watch.ElapsedTicks;
            }
        }

        Console.WriteLine(testName + " min ticks: " + minTicks);
    }

    public static void TestIfSorted(uint[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] < array[i - 1])
            {
                Console.WriteLine("Didn't sort correctly\n");
                return;
            }
        }
        Console.WriteLine("Sorted correctly\n");
    }
}
#endregion

#region RADIX_WITH_COUNTING_BITS
/// <summary>
/// Radixsort with countingsort as a subroutine is generally described on base 10,
/// but we can use any base. This version makes use of the bit representation of numbers
/// in memory to optimize calculations.
/// </summary>
class RadixWithCountingBits
{
    /// <summary> We will countsort every bitsPerChunk bits. This algorithm works better if
    /// this chunk can represent a number close to the number of elements to sort. </summary>
    static int bitsPerChunk = 4; // This is definitely a place you can play around to optimize this algorithm.
    static int elementsPerChunk;
    static int max;
    static int arrayLength;
    static uint[] countArray;
    static uint[] outputArray;
    static int i;
    static int mask;
    static int numberOfBits;

    public static void Setup(int numberOfBits, uint arrayLength)
    {
        RadixWithCountingBits.numberOfBits = numberOfBits;
        RadixWithCountingBits.arrayLength = (int)arrayLength;

        // As you can see I did some testing on optimizing bits per chunk without convincing results
        // so I'll leave this for you to try and improve

        // Dubious success attempt 1
        // We make sure the radix (defined by bits per chunk) can define a max number as close as possible (but lower) to arrayLength.
        //for (bitsPerChunk = 1; (1 << bitsPerChunk) - 1 <= arrayLength; bitsPerChunk++) {}
        //bitsPerChunk--;

        // Dubious success attempt 2
        //bitsPerChunk = (int)Math.Ceiling(numberOfBits / 2d);

        Console.WriteLine("Counting every " + bitsPerChunk + " bits.");

        elementsPerChunk = 1 << bitsPerChunk;

        countArray = new uint[elementsPerChunk];
    }

    public static uint[] Sort(uint[] array)
    {
        max = (1 << numberOfBits) - 1;

        for (int exp = 0; max > 0; exp++)
        {
            array = Countsort(array, exp);

            max >>= bitsPerChunk;
        }

        return array;
    }

    static uint[] Countsort(uint[] array, int exp)
    {
        outputArray = new uint[arrayLength];

        exp *= bitsPerChunk;

        mask = (elementsPerChunk - 1) << exp;

        // Clean the countArray
        for (i = 0; i < elementsPerChunk; i++)
        {
            countArray[i] = 0;
        }

        for (i = 0; i < arrayLength; i++)
        {
            countArray[(array[i] & mask) >> exp]++;
        }

        for (i = 1; i < elementsPerChunk; i++)
        {
            countArray[i] += countArray[i - 1];
        }

        for (i = arrayLength - 1; i >= 0; i--)
        {
            outputArray[--countArray[(array[i] & mask) >> exp]] = array[i];
        }

        return outputArray;
    }
}
#endregion

#region COUNTINGSORT
/// <summary>
/// Countingsort, unbeatable with small keys.
/// </summary>
class Countingsort
{
    static uint[] counts;
    static uint[] sortedArray;

    public static void Setup(int maxPowerOfTwo, uint arrayLength)
    {
        counts = new uint[1 << maxPowerOfTwo];
        sortedArray = new uint[arrayLength];
    }

    public static uint[] Sort(uint[] array)
    {
        for (int i = 0; i < counts.Length; i++)
        {
            counts[i] = 0;
        }

        for (int i = 0; i < array.Length; i++)
        {
            counts[array[i]]++;
        }

        for (int i = 1; i < counts.Length; i++)
        {
            counts[i] += counts[i - 1];
        }

        for (int i = array.Length - 1; i >= 0; i--)
        {
            sortedArray[--counts[array[i]]] = array[i];
        }

        return sortedArray;
    }
}
#endregion

#region QUICKSORT
/// <summary>
/// Many versions of quicksort exist. I encourage you to try the ones you think are more
/// efficient.
/// </summary>
class Quicksort
{
    public static void Sort(uint[] array, int left, int right)
    {
        int i = left, j = right;
        uint pivot = array[(left + right) / 2];

        while (i <= j)
        {
            while (array[i] < pivot)
            {
                i++;
            }

            while (array[j] > pivot)
            {
                j--;
            }

            if (i <= j)
            {
                uint tmp = array[i];
                array[i] = array[j];
                array[j] = tmp;

                i++;
                j--;
            }
        }

        if (left < j)
        {
            Sort(array, left, j);
        }

        if (i < right)
        {
            Sort(array, i, right);
        }
    }
}
#endregion

#region SKYLINESORT
class Skylinesort
{
    static uint mask;
    static uint[] countVector;
    static uint[] auxiliaryVector;
    static uint[] sortedArray;
    static uint arrayLength;

    public static void Setup(int maxPowerOfTwo, uint arrayLength)
    {
        // We use this value to make jumps. It will also represent the counting and aux vector length.
        mask = (uint)((1 << maxPowerOfTwo) - 1);
        countVector = new uint[mask + 1];
        auxiliaryVector = new uint[mask + 1];
        sortedArray = new uint[arrayLength];
        // We cache the array length, it's faster than asking the array itself.
        Skylinesort.arrayLength = arrayLength;
    }

    public static uint[] Sort(uint[] vector)
    {
        // You should understand the use of this values by reading further.
        uint element = mask;
        uint i, j = mask;
        uint position = mask;

        // I don't know why Daniel made the algo travel the input array backwards, but oh well...
        for (i = arrayLength; i > 0; --i)
        {
            // We add to the counting vector
            element = vector[i - 1];
            countVector[element]++;

            // We place ourselves at the position of the element value in the auxiliary array.
            position = element;

            // Start jumping.
            // We will continue making jumps until we find a bigger element or the end of the auxiliary array.
            while (element > auxiliaryVector[position]) // 'position' will be increasing
            {
                // We mark our territory
                auxiliaryVector[position] = element;
                // We jump forward (This piece of code I find beautiful)
                position = (position | (position + 1)) & mask; // The mask is to avoid traveling beyond the auxiliary vector length.
            }
        }

        // The time has come to travel backwards
        position = mask; // We place ourselves at the end of the auxiliary vector.
        i = arrayLength;
        // We stop when the time comes to count the 0s.
        while (i > countVector[0])
        {
            // As long as we reach 0s, we jump backwards in the auxiliary vector
            if (auxiliaryVector[position] == 0)
            {
                // this part I'm confused about, shouldn't we be traveling backwards only one step?
                // I'm sure there is a reason Daniel did it this way.
                position = (((position + 1) & position) - 1) & mask;
            }
            // If we reach a non 0 element we put it on the
            // final array (as many times as the count vector indicates)
            // and then we jump straight to a position in the auxiliary
            // array equal to the element found.
            else
            {
                element = auxiliaryVector[position];
                position = element;

                // Clean the auxiliary vector (note: it is faster to cache and recicle an auxiliary vector
                // than it is to allocate one every time the algorithm operates).
                while (auxiliaryVector[position] != 0)
                {
                    // We just jump forward cleaning up.
                    auxiliaryVector[position] = 0;
                    position |= position + 1;
                    position &= mask;
                }

                // We place our recently found element in the final array the number of
                // times our counting vector indicates.
                for (j = 0; j < countVector[element]; j++)
                {
                    sortedArray[--i] = element;
                }

                // We do this to cleanup our counting vector.
                countVector[element] = 0;

                // We travel backwards one step.
                position = element - 1;
            }
        }

        // We set the 0s, if there are any. We have to do this because we recicle our sorted array
        // so we can't count on it starting full of 0s.
        while (i > 0)
        {
            sortedArray[--i] = 0;
        }

        // We finish cleaning up the counting vector.
        countVector[0] = 0;

        return sortedArray;
    }
}
#endregion

#region RADIX_WITH_SKYLINESORT
static class RadixWithSkylinesort
{
    //TODO: Fix weird mixture of ints and uints

    static int mask;
    static int[] countVector;
    static uint[] auxiliaryVector;
    static uint[] sortedArray;
    static int bitsPerChunk;
    static int elementsPerChunk;
    static int max;
    static int arrayLength;
    static int numberOfBits;

    public static void Setup(int numberOfBits, uint arrayLength)
    {
        RadixWithSkylinesort.numberOfBits = numberOfBits;
        RadixWithSkylinesort.arrayLength = (int)arrayLength;

        // I'm not convinced with this bits per chunk optimization.
        // We make sure the radix (defined by bits per chunk) is as close as possible (but lower) to arrayLength.
        for (bitsPerChunk = 1; (1 << bitsPerChunk) - 1 <= arrayLength; bitsPerChunk++) { }
        bitsPerChunk = (int)Math.Ceiling(numberOfBits / 2d) + 1;

        elementsPerChunk = 1 << bitsPerChunk;
        countVector = new int[elementsPerChunk];
        mask = (elementsPerChunk - 1);
        auxiliaryVector = new uint[elementsPerChunk];

        Console.WriteLine("Radix-Skylinesorting every " + bitsPerChunk + " bits.");
    }

    public static uint[] Sort(uint[] array)
    {
        max = (1 << numberOfBits) - 1;

        for (int exp = 0; max > 0; exp++)
        {
            array = Skylinesort(array, exp);

            max >>= bitsPerChunk;
        }

        return array;
    }

    // For a commented walkthrough of skyline see above.
    static uint[] Skylinesort(uint[] vector, int exp)
    {
        sortedArray = new uint[arrayLength];

        exp *= bitsPerChunk;

        uint element = (uint)mask;
        int i = mask; // Indices
        int position = mask;

        for (i = arrayLength - 1; i >= 0; --i)
        {
            element = (uint)((vector[i] >> exp) & mask);
            countVector[element]++;
            position = (int)element;

            while (element > auxiliaryVector[position])
            {
                auxiliaryVector[position] = element;
                position = (position | (position + 1)) & mask;
            }
        }

        position = mask;
        i = arrayLength;
        while (i > countVector[0])
        {
            if (auxiliaryVector[position] == 0)
            {
                position = (((position + 1) & position) - 1) & mask;
            }
            else
            {
                position = (int)auxiliaryVector[position];

                i = countVector[position] = i - countVector[position];

                position--;
            }
        }
        countVector[0] = 0;

        // Final iteration over input array
        for (i = 0; i < arrayLength; i++)
        {
            sortedArray[countVector[(vector[i] >> exp) & mask]++] = vector[i];
        }

        // Clear count vector and auxiliary vector using jumps? Might be faster
        for (i = elementsPerChunk - 1; i >= 0; --i)
        {
            countVector[i] = 0;
            auxiliaryVector[i] = 0;
        }

        return sortedArray;
    }
}
#endregion