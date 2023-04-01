using System.Collections.Generic;

namespace WpfApp1
{
    public class AdditionalSet
    {
        public Dictionary<string, bool> Keys { get; } = new Dictionary<string, bool>();
        public int[] InitialSets { get; } = new int[10];
        public int[] AdditionSets { get; } = new int[10];

        public AdditionalSet(Workout workout)
        {
            for (int i = 0; i < workout.ExercisesCount; i++)
            {
                InitialSets[i] = workout.ExerciseList[i].ExerciseSets;
            }
            GenerateAdditionSets();
        }

        private void GenerateAdditionSets()
        {
            for (int i = 0; i < InitialSets.Length; i++)
            {
                if (InitialSets[i] != 0)
                {
                    AdditionSets[i] = InitialSets[i] + 1;
                }
            }
        }
    }
}