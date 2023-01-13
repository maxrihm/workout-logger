using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorkoutLog
{ 
class ReadFromFile
{
        public static string file_path = @"C:\Users\Maxim\Desktop\Projects\C#\WorkoutLog\Workout - 3.txt";
    public static List<string[]> generateList() {
            DateTime local_date = DateTime.Now;
            int line_index = 0;
            int counter = 0;
            bool start_writing = false;
            List<string> list1 = new List<string>();
            List<string[]> arrayList = new List<string[]>();




            // Read the file and display it line by line.  
            foreach (string line in System.IO.File.ReadLines(file_path))
            {
                counter++;
                if (line.Contains("/")) {
                    if (DateTime.Parse(line) < local_date)
                    {
                        local_date = DateTime.Parse(line);
                        line_index = counter;
                    }
                }
            }
            string last_workout = File.ReadLines(file_path).Skip(line_index-2).Take(1).First();
            foreach (string line in System.IO.File.ReadLines(file_path))
            {

                if ((line.Contains("Workout"))==true && (line.Contains(last_workout) == false) && (start_writing==true))
                {
                    break;
                }

                if (line.Contains(last_workout))
                {
                    start_writing = true;
                }
                
                if (start_writing == true)
                {
                    list1.Add(line);
                }


            }

            foreach (string item in list1)
            {
                arrayList.Add(item.Split(new[] { ", " }, StringSplitOptions.None));
            }

            return arrayList;
    }

        public static void writeToLine(string searchable_string, string rewrite_string)
        {
            int counter = 0;

            foreach (string line in System.IO.File.ReadLines(file_path))
            {
                counter++;
                if (line.Contains(searchable_string))
                {
                    break;
                    
                    
                }
            }
            lineChanger(rewrite_string, counter);
        }

        private static void lineChanger(string newText, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(file_path);
            arrLine[line_to_edit - 3] = newText;
            File.WriteAllLines(file_path, arrLine);
        }
    }
}