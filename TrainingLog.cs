using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkoutLog
{
    public class TrainingLog
    {
        List<string> training_list;
        public List <TrainingLogObj> training_log_obj_list;
        DB dB;
        public TrainingLog(DB a_dB)
        {
            dB = a_dB;
        }

        public void updateList(string exercise_name)
        {
            training_list = new List<string>();
            training_log_obj_list = new List<TrainingLogObj>();
            try {
                string query = $"SELECT exercise_id FROM my_exercises WHERE exercise_name = '{exercise_name}' AND exercise_set = 1;";
                training_list = dB.queryLog(query);

                foreach (string item in training_list)
                {
                    training_log_obj_list.Add(new TrainingLogObj(item));
                }
                training_log_obj_list.Reverse();
            } 
            catch(Exception ex) { training_log_obj_list.Add(new TrainingLogObj("error")); }
            
        }

        public class TrainingLogObj
        {
            public string image_path { get; set; }
            public TrainingLogObj(string item)
            {
                image_path = "C:\\Users\\Maxim\\Desktop\\Projects\\C#\\WorkoutLog\\reserved\\" + item + ".png";
            }
        }



    }
}
