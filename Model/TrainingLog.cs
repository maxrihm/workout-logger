using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class TrainingLog
    {
        private DB _db;
        public List<TrainingLogObj> TrainingLogObjList { get; private set; }

        public TrainingLog(DB db)
        {
            _db = db;
            TrainingLogObjList = new List<TrainingLogObj>();
        }

        public void UpdateList(string exerciseName)
        {
            try
            {
                string query = $"SELECT exercise_id FROM my_exercises WHERE exercise_name = '{exerciseName}' AND exercise_set = 1;";
                var trainingList = _db.QueryLog(query);

                foreach (string item in trainingList)
                {
                    TrainingLogObjList.Add(new TrainingLogObj(item));
                }

                TrainingLogObjList.Reverse();
            }
            catch (Exception ex)
            {
                TrainingLogObjList.Add(new TrainingLogObj("error"));
            }
        }

        public class TrainingLogObj
        {
            public string ImagePath { get; set; }

            public TrainingLogObj(string item)
            {
                ImagePath = $"path";
            }
        }
    }
}