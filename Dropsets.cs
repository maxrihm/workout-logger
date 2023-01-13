using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace WorkoutLog
{
    public class Dropsets
    {
        public double pr_dropset;
        public double last_time_dropset;
        public int set_index;
        public int exercise_sets;

        public double weight1;
        public double weight2;

        public double current_pr;
        public double current_total_volume;
        public double last_time_total_volume;

        public Dropsets(int index)
        {
            set_index = index;
        }
        
        
       

        public List<string> calculateList()
        {
            List<int> full_range_list_one = new List<int>();
            double pr_calculation_reps_list_one = Math.Ceiling((((current_pr + 1) / weight1 - (current_total_volume / (weight1))) / (exercise_sets - set_index)));
            int return_range = (int)pr_calculation_reps_list_one + 1;

            try { 
            foreach (int x in Enumerable.Range(0, return_range))
            {
                full_range_list_one.Add(x);
            }
            List<string> list_second = new List<string>();

            foreach (int x in full_range_list_one)
            {
                double updated_total_volume = (x * weight1) + current_total_volume ;
                double pr_calculation_reps = Math.Ceiling(((current_pr + 1 - updated_total_volume) / weight2) / (exercise_sets - set_index));
                list_second.Add(pr_calculation_reps.ToString());
            }

            foreach (int x in full_range_list_one)
            {
                double updated_total_volume = (x * weight1) + current_total_volume;
                double last_time_calculation_reps = Math.Ceiling(((last_time_total_volume + 1 - updated_total_volume) / weight2) / (exercise_sets - set_index));
                    string add_last_time = x.ToString() + "     " + last_time_calculation_reps.ToString() + "/" + list_second[x];
                list_second[x] = add_last_time;
            }

            return list_second; } catch (Exception ex) { List<string> list = new List<string>(); list.Add(" "); return list; }

        }


    }
}
