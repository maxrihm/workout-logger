using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;

namespace WpfApp1
{

    public partial class MainWindow : Window
    {
        // Fields

        private static readonly Workout workout = new Workout();
        private static readonly DB dB = new DB();
        private readonly Analyzer analyzer = new Analyzer(dB);
        private readonly TrainingLog trainingLog = new TrainingLog(dB);
        private readonly AdditionalSet ads = new AdditionalSet(workout);
        private readonly TextBox[] weightTextboxes;
        private readonly TextBox[] repsTextboxes;
        private readonly TextBox[] poPrTextboxes;
        private readonly TextBox[] totalVolumeTextboxes;
        private readonly Label[] setsLabels;
        private readonly Dictionary<string, string> dropSetsOverride = new Dictionary<string, string>();
        private readonly Dictionary<string, string> dropSetsValues = new Dictionary<string, string>();
        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
        private int timerInterval = 0;
        private int indexExercise = 0;

        // Constructor

        public MainWindow()
        {
            InitializeComponent();
            InitializeTextboxesLists();
            LabelName.Content = workout.WorkoutName;
            foreach (Exercise exercise in workout.ExerciseList)
            {
                ElementExercisesList.Items.Add(exercise.ExerciseName);
            }
            DateTextbox.Text = DateTime.Now.ToString("ddd, dd MMM");
            LastTimeIcon.Visibility = Visibility.Hidden;
            PrIcon.Visibility = Visibility.Hidden;
            FinishButton.Visibility = Visibility.Hidden;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += DispatcherTimerTickHandler;
            CalculateVolumes(null, null);
            BindImage();
        }

        // Event Handlers

        #region Exercise Selection

        /// <summary>
        /// Handles the selection of an exercise from the list.
        /// </summary>
        private void UpdateExercise(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (element_exercises_list.Items.Count != workout.exercises_count)
                {
                    return;
                }

                WorkoutScreenshot();
                SaveExercise();

                index_exercise = element_exercises_list.SelectedIndex;
                var exercise = workout.exercise_list[index_exercise];
                textbox_exercise_name.Text = $"{exercise.exercise_name}, {ads.initial_sets[index_exercise]} sets, {exercise.exercise_reps} reps, {exercise.exercise_rest} seconds";

                if (!dt.IsEnabled)
                {
                    timer_button.Content = exercise.exercise_rest;
                }

                UpdateTextBoxes();

                dropset_entity = null;
                ClearElementsValues();

                analyzer.exercise_name = exercise.exercise_name;
                analyzer.QueryExerciseData();
                training_log.UpdateList(exercise.exercise_name);
                last_time_list.ItemsSource = training_log.training_log_obj_list;

                if (!analyzer.db_is_empty)
                {
                    double totalVolumeLastTime = analyzer.lastTimeResults();
                    last_time_textbox.Text = totalVolumeLastTime.ToString();
                    double pr_total_volume = analyzer.GetPersonalRecordVolume();
                    pr_volume_textbox.Text = pr_total_volume.ToString();
                }
                else
                {
                    last_time_textbox.Text = "0";
                    pr_volume_textbox.Text = "0";
                    foreach (TextBox textbox in po_pr_textboxes)
                    {
                        textbox.Text = "0";
                    }
                }

                for (int i = 0; i < exercise.exercise_sets; i++)
                {
                    reps_textboxes[i].Text = exercise.reps_array[i].ToString();
                    weight_textboxes[i].Text = exercise.weight_array[i].ToString();
                }

                finish_button.Visibility = element_exercises_list.SelectedIndex == element_exercises_list.Items.Count - 1 ? Visibility.Visible : Visibility.Hidden;
                label_error.Content = "";
                element_exercises_list.IsHitTestVisible = true;
                CalculateVolumes(null, null);
            }
            catch (Exception)
            {
                label_error.Content = "Error!";
                element_exercises_list.IsHitTestVisible = false;
            }
        }
        #endregion
        #region Saving

        /// <summary>
        /// Saves the current exercise to the database.
        /// </summary>
        private void SaveExercise()
        {
            double totalVolume;
            if (double.TryParse(textbox_total_volume.Text, out totalVolume))
            {
                workout.exercise_list[index_exercise].total_weight = totalVolume;
                Trace.WriteLine(workout.exercise_list[index_exercise].total_weight.ToString());

                for (int index = 0; index < workout.exercise_list[index_exercise].exercise_sets; index++)
                {
                    double setWeightResults;
                    int setRepsResults;
                    if (double.TryParse(weight_textboxes[index].Text, out setWeightResults)
                        && int.TryParse(reps_textboxes[index].Text, out setRepsResults))
                    {
                        workout.exercise_list[index_exercise].inputWeights(index, setWeightResults, setRepsResults);
                    }
                }
            }
        }
        #endregion

        #region Screenshot

        /// <summary>
        /// Takes a screenshot of the current workout and saves it to disk.
        /// </summary>

        private void WorkoutScreenshot()
        {
            int screenLeft = 341;
            int screenTop = 175;

            int screenWidth = 263;
            switch (workout.exercise_list[index_exercise].exercise_sets)
            {
                case 3:
                    screenWidth = 263;
                    break;

                case 4:
                    screenWidth = 326;
                    break;

                case 5:
                    screenWidth = 390;
                    break;
            }

            int screenHeight = 152;

            using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    string fileName = $"{workout.exercise_list[index_exercise].exercise_name}.png";
                    Opacity = .0;
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                    bmp.Save($@"C:\Users\Drogen\Desktop\Projects\C#\WpfApp1\reserved\{fileName}");
                    Opacity = 1;
                }
            }
        }
        #endregion

        #region Finish Button

        /// <summary>
        /// Handles the click of the "Finish" button.
        /// </summary>

        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            label_name.Focus();
            WorkoutScreenshot();
            SaveExercise();
            workout.WriteWorkout(dB);
            dB.CloseConnection();
            Application.Current.Shutdown();
        }
        #endregion

        #region Volume Calculation

        /// <summary>
        /// Calculates the volume for each set and for the entire exercise.
        /// </summary>
        private void CalculateVolumes(object sender, TextChangedEventArgs e)
        {
            double lastTimeVolume = 0;
            double harderLastTime = 0;
            double prTotalVolumeCurrent = 0;
            double totalVolume = 0;
            double prTotalVolume = 0;

            try
            {
                if (element_exercises_list.Items.Count != workout.exercises_count)
                {
                    return;
                }

                totalVolume = 0;
                for (int index = 0; index < workout.exercise_list[index_exercise].exercise_sets; index++)
                {
                    if (double.TryParse(weight_textboxes[index].Text, out double weight) && int.TryParse(reps_textboxes[index].Text, out int reps))
                    {
                        double totalVolumePerSet = reps * weight;
                        total_volume_textboxes[index].Text = totalVolumePerSet.ToString();
                        totalVolume += totalVolumePerSet;
                        textbox_total_volume.Text = totalVolume.ToString();
                    }
                }

                if (!analyzer.db_is_empty)
                {
                    lastTimeVolume = Convert.ToDouble(last_time_textbox.Text);
                    prTotalVolume = Convert.ToDouble(pr_volume_textbox.Text);

                    harderLastTime = (totalVolume - lastTimeVolume) / lastTimeVolume;
                    harder_last_time_textbox.Text = harderLastTime.ToString("#0.##%");

                    prTotalVolumeCurrent = (totalVolume - prTotalVolume) / prTotalVolume;
                    textbox_pr_current.Text = prTotalVolumeCurrent.ToString("#0.##%");

                    calculatePoPr();

                    totalVolume = 0;
                    for (int index = 0; index < workout.exercise_list[index_exercise].exercise_sets; index++)
                    {
                        if (double.TryParse(weight_textboxes[index].Text, out double weight) && int.TryParse(reps_textboxes[index].Text, out int reps))
                        {
                            double totalVolumePerSet = reps * weight;
                            total_volume_textboxes[index].Text = totalVolumePerSet.ToString();
                            totalVolume += totalVolumePerSet;
                            textbox_total_volume.Text = totalVolume.ToString();
                        }
                    }
                    harderLastTime = (totalVolume - lastTimeVolume) / lastTimeVolume;
                    prTotalVolumeCurrent = (totalVolume - prTotalVolume) / prTotalVolume;
                }
                else
                {
                    harder_last_time_textbox.Text = "0";
                    textbox_pr_current.Text = "0";
                    foreach (TextBox textbox in po_pr_textboxes)
                    {
                        textbox.Text = "0";
                        textbox.Height = 20;
                    }
                }

                overrideDropsets();
                changeStatusAdditionalSets();
                label_error.Content = "";
                element_exercises_list.IsHitTestVisible = true;
            }
            catch (Exception ex)
            {
                label_error.Content = "Type Error!";
                element_exercises_list.IsHitTestVisible = false;
            }
        }

        #endregion

        #region X Value Preview

        /// <summary>
        /// Handles the preview of an X value.
        /// </summary>
        private void XValue_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                textbox.Focus();
                textbox.SelectAll();
                e.Handled = true;
            }
        }
        #endregion

        #region Exercise List Mouse Enter

        /// <summary>
        /// Handles the mouse entering the exercise list.
        /// </summary>
        private void element_exercises_list_MouseEnter(object sender, MouseEventArgs e)
        {
            label_name.Focus();
        }
        #endregion

        #region Po/Pr Calculation

        /// <summary>
        /// Calculates the Po/Pr ratio for the current set.
        /// </summary>
        public void CalculatePoPr()
        {
            if (!double.TryParse(pr_volume_textbox.Text, out double currentPr) ||
                !double.TryParse(textbox_total_volume.Text, out double currentTotalVolume) ||
                !double.TryParse(last_time_textbox.Text, out double lastTimeTotalVolume))
            {
                return;
            }

            for (int index = 0; index < workout.exercise_list[index_exercise].exercise_sets; index++)
            {
                if (!double.TryParse(weight_textboxes[index].Text, out double currentSetWeight))
                {
                    continue;
                }

                double setLastTime = Math.Ceiling((((lastTimeTotalVolume - currentTotalVolume + 1) / currentSetWeight) / (workout.exercise_list[index_exercise].exercise_sets - index)));
                double prCalculation = Math.Ceiling((((currentPr - currentTotalVolume + 1) / currentSetWeight) / (workout.exercise_list[index_exercise].exercise_sets - index)));
                po_pr_textboxes[index].Text = $"{setLastTime}/{prCalculation}";
                po_pr_textboxes[index].Height = 20;
            }
        }
        #endregion

        #region Textbox Initialization

        /// <summary>
        /// Initializes the arrays of textboxes.
        /// </summary>
        public void InitializeTextboxesLists()
        {
            weight_textboxes = new[]
            {
        set1_weight, set2_weight, set3_weight, set4_weight, set5_weight
    };

            reps_textboxes = new[]
            {
        set1_reps, set2_reps, set3_reps, set4_reps, set5_reps
    };

            po_pr_textboxes = new[]
            {
        textbox_set1_po_pr, textbox_set2_po_pr, textbox_set3_po_pr, textbox_set4_po_pr, textbox_set5_po_pr
    };

            total_volume_textboxes = new[]
            {
        textbox_set1_total_volume, textbox_set2_total_volume, textbox_set3_total_volume, textbox_set4_total_volume, textbox_set5_total_volume
    };


            sets_labels = new[]
            {
        set1_label, set2_label, set3_label, set4_label, set5_label
    };
        }
        #endregion

        #region Textbox Updating

        /// <summary>
        /// Updates the textboxes for the current set.
        /// </summary>

        public void UpdateTextBoxes()
        {
            foreach (var tb in weight_textboxes.Concat(reps_textboxes).Concat(po_pr_textboxes).Concat(total_volume_textboxes).Concat(sets_labels))
            {
                tb.Visibility = Visibility.Hidden;
            }

            for (int index = 0; index < workout.exercise_list[index_exercise].exercise_sets; index++)
            {
                weight_textboxes[index].Visibility = Visibility.Visible;
                sets_labels[index].Background = Brushes.White;
                reps_textboxes[index].Visibility = Visibility.Visible;
                po_pr_textboxes[index].Visibility = Visibility.Visible;
                total_volume_textboxes[index].Visibility = Visibility.Visible;
                sets_labels[index].Visibility = Visibility.Visible;
                textbox_pr_current.Foreground = Brushes.Black;
                harder_last_time_textbox.Foreground = Brushes.Black;
            }
        }
        #endregion

        #region Icon Updating

        /// <summary>
        /// Updates the icons for the current set.
        /// </summary>
        private void UpdateIcons(object sender, TextChangedEventArgs e)
        {
            last_time_icon.Visibility = Visibility.Hidden;
            pr_icon.Visibility = Visibility.Hidden;
            try
            {
                if (double.TryParse(harder_last_time_textbox.Text.ToString().TrimEnd('%'), out double harder))
                {
                    if (harder > 0)
                    {
                        last_time_icon.Visibility = Visibility.Visible;
                    }
                }

                if (double.TryParse(textbox_pr_current.Text.ToString().TrimEnd('%'), out double pr))
                {
                    if (pr > 0)
                    {
                        pr_icon.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception or do nothing
            }
        }
        #endregion

        #region Dropsets

        /// <summary>
        /// Handles the selection of a dropset.
        /// </summary>
        public void DropSets(object sender, MouseButtonEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            int index = Array.IndexOf(po_pr_textboxes, textbox);
            dropset_entity = new Dropsets(index);
            ClearElementsValues();
            textbox_dropset_current.Text = sets_labels[dropset_entity.set_index].Text;
            dropset_entity.exercise_sets = workout.exercise_list[index_exercise].exercise_sets;

            if (!analyzer.db_is_empty)
            {
                if (double.TryParse(pr_volume_textbox.Text.ToString(), out double current_pr))
                {
                    dropset_entity.current_pr = current_pr;
                }

                if (double.TryParse(textbox_total_volume.Text.ToString(), out double current_total_volume))
                {
                    dropset_entity.current_total_volume = current_total_volume;
                }

                if (double.TryParse(last_time_textbox.Text.ToString(), out double last_time_total_volume))
                {
                    dropset_entity.last_time_total_volume = last_time_total_volume;
                }
            }
        }
        /// <summary>
        /// Updates the dropsets for the current exercise.
        /// </summary>

        public void UpdateDropSets(object sender, TextChangedEventArgs e)
        {
            if (dropset_entity == null) return;

            try
            {
                dropset_entity.Weight1 = double.Parse(dropset_weight1_textbox.Text);
                dropset_entity.Weight2 = double.Parse(dropset_weight2_textbox.Text);
                dropset_list.Items.Clear();

                if (!analyzer.DbIsEmpty && dropset_entity.SetIndex == workout.exercise_list[index_exercise].exercise_sets - 1)
                {
                    dropset_list.Items.AddRange(dropset_entity.CalculateList());
                }

                int reps1 = int.Parse(dropset_reps1_textbox.Text);
                int reps2 = int.Parse(dropset_reps2_textbox.Text);
                double total_volume_dropsets = dropset_entity.Weight1 * reps1 + dropset_entity.Weight2 * reps2;
                dropset_total_volume_textbox.Text = total_volume_dropsets.ToString();

                string textbox_override = $"DS:{dropset_entity.Weight1}|{reps1}, {dropset_entity.Weight2}|{reps2}";
                string unique_key = $"{workout.exercise_list[index_exercise].exercise_name}{dropset_entity.SetIndex}";
                drop_sets_override[unique_key] = textbox_override;
                drop_sets_values[unique_key] = total_volume_dropsets.ToString();
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// Overrides the dropsets for the current exercise.
        /// </summary>
        public void OverrideDropsets()
        {
            foreach (var element in drop_sets_override)
            {
                foreach (int x in Enumerable.Range(0, workout.exercise_list[index_exercise].exercise_sets))
                {
                    string unique_key = $"{workout.exercise_list[index_exercise].exercise_name}{x}";

                    if (element.Key == unique_key && (weight_textboxes[x].Text == "1" || weight_textboxes[x].Text == drop_sets_values[unique_key] && reps_textboxes[x].Text == "1"))
                    {
                        weight_textboxes[x].Text = drop_sets_values[unique_key];
                        reps_textboxes[x].Text = "1";
                        po_pr_textboxes[x].Text = element.Value;
                        po_pr_textboxes[x].Height = 39;
                        dropset_entity = null;
                        ClearElementsValues();
                    }
                }
            }
        }
        #endregion

        #region Additional Sets

        // Handles clicking on the additional set icon
        private void AdditionalSet(object sender, MouseButtonEventArgs e)
        {
            if (workout.exercise_list[index_exercise].exercise_sets != ads.addition_sets[index_exercise])
            {
                ads.keys[workout.exercise_list[index_exercise].exercise_name] = false;
                workout.exercise_list[index_exercise].exercise_sets = ads.addition_sets[index_exercise];
                weight_textboxes[ads.initial_sets[index_exercise]].Text = "1";
                reps_textboxes[ads.initial_sets[index_exercise]].Text = "1";
                UpdateExercise(null, null);
            }
        }
        // Determines if additional sets are currently in progress
        public bool GetCurrentAdditionalSetStatus()
        {
            return ads.keys.ContainsKey(workout.exercise_list[index_exercise].exercise_name) && ads.keys[workout.exercise_list[index_exercise].exercise_name];
        }

        // Handles changing the status of additional sets

        public void ChangeStatusAdditionalSets()
        {
            if (!ads.keys.ContainsKey(workout.exercise_list[index_exercise].exercise_name))
            {
                return;
            }

            if (weight_textboxes[ads.initial_sets[index_exercise]].Text != "0" ||
                reps_textboxes[ads.initial_sets[index_exercise]].Text != "0")
            {
                ads.keys[workout.exercise_list[index_exercise].exercise_name] = true;
                sets_labels[ads.initial_sets[index_exercise]].Background = Brushes.Green;
                UpdateAdditionalSet();
            }
            else
            {
                ads.keys.Remove(workout.exercise_list[index_exercise].exercise_name.ToString());
                workout.exercise_list[index_exercise].exercise_sets = ads.initial_sets[index_exercise];
                Trace.WriteLine("Triggered");
                UpdateExercise(null, null);
            }
        }
        // Updates the additional set display

        public void UpdateAdditionalSet()
        {
            double harder = 0;
            double pr = 0;
            double lt_textbox = 0;

            if (double.TryParse(harder_last_time_textbox.Text.TrimEnd('%'), out double result1))
            {
                harder = result1;
            }

            if (double.TryParse(textbox_pr_current.Text.TrimEnd('%'), out double result2))
            {
                pr = result2;
            }

            if (double.TryParse(last_time_textbox.Text, out double result3))
            {
                lt_textbox = result3;
            }

            bool isHarder = harder > 0;
            bool isPr = pr > 0;

            if (isHarder && GetCurrentAdditionalSetStatus() == true)
            {
                textbox_total_volume.Text = (lt_textbox + 0.1).ToString();
                harder_last_time_textbox.Foreground = Brushes.Green;

                if (isPr)
                {
                    textbox_pr_current.Foreground = Brushes.Green;
                    last_time_icon.Visibility = Visibility.Visible;
                    pr_icon.Visibility = Visibility.Visible;
                }
                else
                {
                    textbox_pr_current.Text = "";
                    last_time_icon.Visibility = Visibility.Visible;
                }
            }
            else
            {
                textbox_pr_current.Foreground = Brushes.Black;
                harder_last_time_textbox.Foreground = Brushes.Black;
            }
        }

        #endregion

        #region Timer
        // Handles the dispatcher timer tick event
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            timer_interval--;

            timer_button.Content = timer_interval.ToString();

            if (timer_interval == 0)
            {
                dt.Stop();
                timer_interval = Convert.ToInt32(workout.exercise_list[index_exercise].exercise_rest);
                timer_button.Content = timer_interval.ToString();
            }

            if (timer_interval == 15)
            {
                PlaySound(@"C:\Users\Drogen\Desktop\Projects\C#\WpfApp1\15.wav");
            }

            if (timer_interval == 4)
            {
                PlaySound(@"C:\Users\Drogen\Desktop\Projects\C#\WpfApp1\6.wav");
            }
        }
        // Starts the timer

        private void StartTimer(object sender, RoutedEventArgs e)
        {
            if (!dt.IsEnabled)
            {
                timer_interval = Convert.ToInt32(workout.exercise_list[index_exercise].exercise_rest);
                dt.Start();
            }
        }
        #endregion

        #region Sound

        // Plays a sound file
        private void PlaySound(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            SoundPlayer player = new SoundPlayer(filePath);
            player.Load();
            player.Play();
        }
        #endregion

        #region Clearing

        // Clears the values of various elements

        public void ClearElementsValues()
        {
            dropset_list.Items.Clear();
            dropset_weight1_textbox.Text = "0";
            dropset_weight2_textbox.Text = "0";
            dropset_reps1_textbox.Text = "0";
            dropset_reps2_textbox.Text = "0";
            dropset_total_volume_textbox.Text = "0";
            textbox_dropset_current.Text = "";
        }
        #endregion

        #region Image

        // Binds the workout image to the UI
        public void BindImage()
        {
            string sourceFilePath = $"C:/Users/Drogen/Desktop/Projects/C#/WpfApp1/{workout.workout_name}.png";
            string destinationFilePath = "C:/Users/Drogen/Desktop/Projects/C#/WpfApp1/Workout - Current.png";

            System.IO.File.Copy(sourceFilePath, destinationFilePath, true);

            img_table.Source = null;
            var image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(@"C:\Users\Drogen\Desktop\Projects\C#\WpfApp1\Workout - Current.png", UriKind.Relative);
            image.EndInit();
            img_table.Source = image;
        }
        #endregion

    }
}
