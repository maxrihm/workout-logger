using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkoutLog
{
    public class Workout
    {
        List<string[]> workout_list = new List<string[]>();
        public List<Exercise> exercise_list = new List<Exercise>();
        public string workout_name;
        public DateTime workout_date = DateTime.Now;
        string rewrite_date;
        public int exercises_count = 0;

        public Workout()
        {
            workout_list = ReadFromFile.generateList();
            workout_name = workout_list[0].GetValue(0).ToString();
            generateObj();
            exercises_count = exercise_list.Count;
            rewrite_date = workout_list[1].GetValue(0).ToString();
        }


        public void generateObj()
        {
            workout_list.RemoveRange(0, 2);
            foreach (string[] line in workout_list)
            {
                string ex_name = line.GetValue(0).ToString();
                int ex_sets; int.TryParse(line.GetValue(1).ToString(), out ex_sets);
                int ex_reps; int.TryParse(line.GetValue(2).ToString(), out ex_reps);
                string ex_rest = line.GetValue(3).ToString();
                exercise_list.Add(new Exercise(ex_name, ex_sets, ex_reps, ex_rest));
            }
        }

        public void writeWorkout(DB dB)
        {
            string workout_date_string = workout_date.ToString().Split(' ')[0];
            ReadFromFile.writeToLine(rewrite_date, workout_date_string);
            foreach (Exercise exercise in exercise_list)
            {
                foreach (int index in Enumerable.Range(0, exercise.exercise_sets))
                {
                    string result;
                    if (index == 0) result = $"INSERT INTO my_exercises(exercise_name, exercise_weight, exercise_set, exercise_reps, total_weight) VALUES('{exercise.exercise_name}', {exercise.weight_array[index]}, {index+1}, {exercise.reps_array[index]}, {exercise.total_weight});";
                    else result = $"INSERT INTO my_exercises(exercise_name, exercise_weight, exercise_set, exercise_reps) VALUES('{exercise.exercise_name}', {exercise.weight_array[index]}, {index+1}, {exercise.reps_array[index]});";
                    dB.writeToDB(result);
                    int last_id = dB.getLastId();
                    try {
                    string exercise_file = "C:\\Users\\Maxim\\Desktop\\Projects\\C#\\WorkoutLog\\reserved\\" + exercise.exercise_name + ".png";
                    string exercise_file_new = "C:\\Users\\Maxim\\Desktop\\Projects\\C#\\WorkoutLog\\reserved\\" + last_id + ".png";
                    System.IO.File.Move(exercise_file, exercise_file_new); } catch (Exception ex) { }

                }
            }
            
        }

    }
}
