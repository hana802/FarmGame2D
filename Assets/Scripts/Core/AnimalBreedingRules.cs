using System;
using System.Collections.Generic;
using FarmGame2D.World;

namespace FarmGame2D.Core
{
    public static class AnimalBreedingRules
    {
        public const int JuvenileDays = 3;

        public static bool ShouldBreed(int worldSeed, int day, FarmAnimalKind kind, int adultCount, int totalCount)
        {
            var populationCap = kind == FarmAnimalKind.Chicken ? 12 : 8;
            if (adultCount < 2 || totalCount >= populationCap)
            {
                return false;
            }

            var salt = kind == FarmAnimalKind.Chicken ? 1709 : 3253;
            var random = new Random(unchecked((worldSeed * 397) ^ (day * 7919) ^ salt));
            var chance = kind == FarmAnimalKind.Chicken ? 0.22 : 0.12;
            return random.NextDouble() < chance;
        }

        public static void AdvanceJuveniles(List<int> juvenileDays)
        {
            for (var index = juvenileDays.Count - 1; index >= 0; index--)
            {
                juvenileDays[index]--;
                if (juvenileDays[index] <= 0)
                {
                    juvenileDays.RemoveAt(index);
                }
            }
        }
    }
}
