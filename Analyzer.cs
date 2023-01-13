using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WorkoutLog
{
    public class Analyzer
    {
        public string exercise_name;
        DB dB;
        List<Tuple<int, string, double, int, int, DateTime, double>> exercise_list = new List<Tuple<int, string, double, int, int, DateTime, double>>();
        public bool db_is_empty = false;

        public Analyzer(DB p_dB)
        {
            dB = p_dB;
        }

        public void queryExerciseData()
        {
            string sql = $"SELECT * FROM my_exercises WHERE exercise_name = '{exercise_name}' AND exercise_set=1; ";
            exercise_list = dB.queryExerciseReader(sql);
            if (exercise_list.Count > 0) { db_is_empty = false; } else { db_is_empty = true; }
        }

        public double lastTimeResults()
        {
            double total_volume_last = 0;
            try {
                total_volume_last = exercise_list.Last().Item7;
                return total_volume_last;
            }
            catch (Exception ex)
            {
                return total_volume_last;
            }
        }

        public double getPersonalRecordVolume()
        {         
            List<double> total_values = new List<double>();
            try
            {
                foreach (var exercise in exercise_list) total_values.Add(exercise.Item7);
                return total_values.Max();
            }
            catch (Exception ex) { return 0; }    

        }
    }
}
