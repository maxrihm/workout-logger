using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WorkoutLog
{
    public class DB
    {
        MySqlConnection connection = new MySqlConnection("server=localhost;port=3306;username=root;password=1234;database=exercises");

        public DB()
        {
            connection.Open();
        }

        public void closeConnection()
        {
            connection.Close();
        }

        public void writeToDB(string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        public void testQuery()
        {

        }

        public List<Tuple<int, string, double, int, int, DateTime, double>> queryExerciseReader(string sql)
        {
            List<Tuple<int, string, double, int, int, DateTime, double>> temp_lst = new List<Tuple<int, string, double, int, int, DateTime, double>>();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                int ex_id = Int32.Parse(rdr[0].ToString());
                string ex_name = rdr[1].ToString();
                double ex_weight = Convert.ToDouble(rdr[2].ToString());
                int ex_set = Int32.Parse(rdr[3].ToString());
                int ex_reps = Int32.Parse(rdr[4].ToString());
                DateTime ex_date = DateTime.Parse(rdr[5].ToString());
                double ex_total_weight = Convert.ToDouble(rdr[6].ToString());


                temp_lst.Add(Tuple.Create(ex_id, ex_name, ex_weight, ex_set, ex_reps, ex_date, ex_total_weight));
            }
            rdr.Close();

            return temp_lst;
        }

        public int getLastId()
        {
            string sql = "SELECT exercise_id FROM my_exercises WHERE exercise_id = ( SELECT MAX(exercise_id) FROM my_exercises ) ; ";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            object result = cmd.ExecuteScalar();
            int r = Convert.ToInt32(result);
            return r;
        }

        public List<string> queryLog(string sql)
        {
            List<string> temp_lst = new List<string>();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                string ex_id = rdr[0].ToString();
                temp_lst.Add(ex_id);
            }
            rdr.Close();

            return temp_lst;
        }

    }
}
