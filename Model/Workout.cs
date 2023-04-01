using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfApp1
{
    public class Workout
    {
        private readonly List<string[]> _workoutList;
        public List<Exercise> ExerciseList { get; } = new List<Exercise>();
        public string WorkoutName { get; }
        public DateTime WorkoutDate { get; set; } = DateTime.Now;
        public int ExercisesCount => ExerciseList.Count;
        private string _rewriteDate;

        public Workout()
        {
            _workoutList = ReadFromFile.GenerateList();
            WorkoutName = _workoutList[0][0];
            GenerateObj();
            _rewriteDate = _workoutList[1][0];
        }

        private void GenerateObj()
        {
            _workoutList.RemoveRange(0, 2);
            ExerciseList.AddRange(_workoutList.Select(line =>
            {
                string ex_name = line[0];
                int ex_sets = int.TryParse(line[1], out var sets) ? sets : 0;
                int ex_reps = int.TryParse(line[2], out var reps) ? reps : 0;
                string ex_rest = line[3];
                return new Exercise(ex_name, ex_sets, ex_reps, ex_rest);
            }));
        }

        public void WriteWorkout(DB dB)
        {
            string workoutDateString = WorkoutDate.ToString().Split(' ')[0];
            ReadFromFile.WriteToLine(_rewriteDate, workoutDateString);
            foreach (var exercise in ExerciseList)
            {
                foreach (int index in Enumerable.Range(0, exercise.ExerciseSets))
                {
                    string result;
                    if (index == 0)
                    {
                        result =
                            $"INSERT INTO my_exercises(exercise_name, exercise_weight, exercise_set, exercise_reps, total_weight) VALUES('{exercise.ExerciseName}', {exercise.WeightArray[index]}, {index + 1}, {exercise.RepsArray[index]}, {exercise.TotalWeight});";
                    }
                    else
                    {
                        result =
                            $"INSERT INTO my_exercises(exercise_name, exercise_weight, exercise_set, exercise_reps) VALUES('{exercise.ExerciseName}', {exercise.WeightArray[index]}, {index + 1}, {exercise.RepsArray[index]});";
                    }

                    dB.WriteToDB(result);
                    int lastId = dB.GetLastId();
                    try
                    {
                        string exerciseFile = $"C:\\Users\\Drogen\\Desktop\\Projects\\C#\\WpfApp1\\reserved\\{exercise.ExerciseName}.png";
                        string exerciseFileNew = $"C:\\Users\\Drogen\\Desktop\\Projects\\C#\\WpfApp1\\reserved\\{lastId}.png";
                        System.IO.File.Move(exerciseFile, exerciseFileNew);
                    }
                    catch (Exception)
                    {
                        // ignore exception
                    }
                }
            }
        }
    }
}
