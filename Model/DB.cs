using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace WpfApp1
{
    public class DB : IDisposable
    {
        private MySqlConnection connection;

        public DB(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

        public void Dispose()
        {
            connection.Close();
        }

        public void ExecuteNonQuery(string sql)
        {
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public List<Tuple<int, string, double, int, int, DateTime, double>> ExecuteQueryExerciseReader(string sql)
        {
            List<Tuple<int, string, double, int, int, DateTime, double>> result = new List<Tuple<int, string, double, int, int, DateTime, double>>();

            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    int ex_id = rdr.GetInt32(0);
                    string ex_name = rdr.GetString(1);
                    double ex_weight = rdr.GetDouble(2);
                    int ex_set = rdr.GetInt32(3);
                    int ex_reps = rdr.GetInt32(4);
                    DateTime ex_date = rdr.GetDateTime(5);
                    double ex_total_weight = rdr.GetDouble(6);

                    result.Add(Tuple.Create(ex_id, ex_name, ex_weight, ex_set, ex_reps, ex_date, ex_total_weight));
                }
            }

            return result;
        }

        public int GetLastId()
        {
            string sql = "SELECT exercise_id FROM my_exercises WHERE exercise_id = (SELECT MAX(exercise_id) FROM my_exercises)";

            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    return -1;
                }
            }
        }

        public List<string> ExecuteQueryLog(string sql)
        {
            List<string> result = new List<string>();

            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    string ex_id = rdr.GetString(0);
                    result.Add(ex_id);
                }
            }

            return result;
        }
    }
}
