using System;
using System.Collections.Generic;

namespace WpfApp1
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }
        public int Set { get; set; }
        public int Reps { get; set; }
        public DateTime Date { get; set; }
        public double TotalWeight { get; set; }

        public Exercise(int id, string name, double weight, int set, int reps, DateTime date, double totalWeight)
        {
            Id = id;
            Name = name;
            Weight = weight;
            Set = set;
            Reps = reps;
            Date = date;
            TotalWeight = totalWeight;
        }
    }

    public static class ExerciseMapper
    {
        public static Exercise Map(List<Tuple<int, string, double, int, int, DateTime, double>> data)
        {
            if (data.Count == 0)
            {
                return null;
            }

            Tuple<int, string, double, int, int, DateTime, double> firstExercise = data.First();
            Exercise exercise = new Exercise
            {
                Id = firstExercise.Item1,
                Name = firstExercise.Item2,
                Weight = firstExercise.Item3,
                Sets = firstExercise.Item4,
                Reps = firstExercise.Item5,
                Date = firstExercise.Item6,
                TotalWeight = firstExercise.Item7
            };

            return exercise;
        }

        public static List<Exercise> MapAll(List<Tuple<int, string, double, int, int, DateTime, double>> data)
        {
            List<Exercise> exercises = new List<Exercise>();

            foreach (Tuple<int, string, double, int, int, DateTime, double> exerciseTuple in data)
            {
                Exercise exercise = new Exercise
                {
                    Id = exerciseTuple.Item1,
                    Name = exerciseTuple.Item2,
                    Weight = exerciseTuple.Item3,
                    Sets = exerciseTuple.Item4,
                    Reps = exerciseTuple.Item5,
                    Date = exerciseTuple.Item6,
                    TotalWeight = exerciseTuple.Item7
                };
                exercises.Add(exercise);
            }

            return exercises;
        }
    }
