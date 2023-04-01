using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfApp1
{
    class ReadFromFile
    {
        private const string FilePath = @"C:\Users\Drogen\Desktop\Projects\C#\WpfApp1\Workout - 3.txt";

        public static List<string[]> GenerateList()
        {
            var localDate = DateTime.Now;
            var lineIndex = 0;
            var counter = 0;
            var startWriting = false;
            var list1 = new List<string>();
            var arrayList = new List<string[]>();

            foreach (var line in File.ReadLines(FilePath))
            {
                counter++;

                if (line.Contains("/") && DateTime.TryParse(line, out var dateTime) && dateTime < localDate)
                {
                    localDate = dateTime;
                    lineIndex = counter;
                }
            }

            var lastWorkout = File.ReadLines(FilePath).Skip(lineIndex - 2).Take(1).First();

            foreach (var line in File.ReadLines(FilePath))
            {
                if (line.Contains("Workout") && !line.Contains(lastWorkout) && startWriting)
                {
                    break;
                }

                if (line.Contains(lastWorkout))
                {
                    startWriting = true;
                }

                if (startWriting)
                {
                    list1.Add(line);
                }
            }

            foreach (var item in list1)
            {
                arrayList.Add(item.Split(new[] { ", " }, StringSplitOptions.None));
            }

            return arrayList;
        }

        public static void WriteToLine(string searchableString, string rewriteString)
        {
            var counter = 0;

            foreach (var line in File.ReadLines(FilePath))
            {
                counter++;

                if (line.Contains(searchableString))
                {
                    break;
                }
            }

            LineChanger(rewriteString, counter);
        }

        private static void LineChanger(string newText, int lineToEdit)
        {
            var arrLine = File.ReadAllLines(FilePath);
            arrLine[lineToEdit - 3] = newText;
            File.WriteAllLines(FilePath, arrLine);
        }
    }
}
