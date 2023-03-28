using System;
using System.Diagnostics;

namespace WorkoutLog
{ 
    public class Exercise
    {
        private readonly string name;
        private readonly int sets;
        private readonly int reps;
        private readonly string rest;
        private readonly double[] weights = new double[10];
        private readonly int[] repetitions = new int[10];
        private double totalWeight;

        public Exercise(string name, int sets, int reps, string rest)
        {
            this.name = name;
            this.sets = sets;
            this.reps = reps;
            this.rest = rest;
        }

        public void InputWeights(int set, double weight, int rep)
        {
            weights[set] = weight;
            repetitions[set] = rep;
        }

        public string GetName()
        {
            return name;
        }

        public int GetSets()
        {
            return sets;
        }

        public int GetReps()
        {
            return reps;
        }

        public string GetRest()
        {
            return rest;
        }

        public double GetTotalWeight()
        {
            if (totalWeight == 0)
            {
                for (int i = 0; i < sets; i++)
                {
                    totalWeight += weights[i] * repetitions[i];
                }
            }
            return totalWeight;
        }
    }
}
