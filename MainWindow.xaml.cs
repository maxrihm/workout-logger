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

namespace WorkoutLog
{

    public partial class MainWindow : Window
    {
        static Workout workout = new Workout();
        public int index_exercise = 0;
        static DB dB = new DB();
        Analyzer analyzer = new Analyzer(dB);
        TrainingLog training_log = new TrainingLog(dB);
        Dropsets dropset_entity;
        AdditionalSet ads = new AdditionalSet(workout);
        TextBox[] weight_textboxes, reps_textboxes, po_pr_textboxes, total_volume_textboxes, sets_labels;
        Dictionary<string, string> drop_sets_override = new Dictionary<string, string>();
        Dictionary<string, string> drop_sets_values = new Dictionary<string, string>();
        DispatcherTimer dt = new DispatcherTimer(DispatcherPriority.Send); int timer_interval = 0;


        public MainWindow()
        {
            InitializeComponent();
            imageBinding();
            initializeTextboxesLists();
            label_name.Content = workout.workout_name;
            foreach (Exercise ex in workout.exercise_list)
            {
                element_exercises_list.Items.Add(ex.exercise_name);
            }
            date_textbox.Text = DateTime.Now.ToString("ddd, dd MMM");
            last_time_icon.Visibility = Visibility.Hidden;
            pr_icon.Visibility = Visibility.Hidden;
            finish_button.Visibility = Visibility.Hidden;
            dt.Interval = new TimeSpan(0, 0, 1); dt.Tick += new EventHandler(dispatcherTimer_Tick);
            calculateVolumes(null, null);
            




        }



        private void updateExercise(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (element_exercises_list.Items.Count == workout.exercises_count)
                {
                    workoutScreenshot();
                    saveExercise();

                }

                index_exercise = int.Parse(element_exercises_list.SelectedIndex.ToString());
                textbox_exercise_name.Text = workout.exercise_list[index_exercise].exercise_name + ", " +
                ads.initial_sets[index_exercise] + " sets, " +
                workout.exercise_list[index_exercise].exercise_reps + " reps, " +
                workout.exercise_list[index_exercise].exercise_rest + " seconds";
                if (dt.IsEnabled == false)
                    timer_button.Content = workout.exercise_list[index_exercise].exercise_rest;

                updateTextBoxes();

                dropset_entity = null; clearElementsValues();

                analyzer.exercise_name = workout.exercise_list[index_exercise].exercise_name;
                analyzer.queryExerciseData();
                training_log.updateList(workout.exercise_list[index_exercise].exercise_name);
                last_time_list.ItemsSource = training_log.training_log_obj_list;

                if (analyzer.db_is_empty == false)
                {
                    double totalVolumeLastTime = analyzer.lastTimeResults();
                    last_time_textbox.Text = totalVolumeLastTime.ToString();
                    double pr_total_volume = analyzer.getPersonalRecordVolume();
                    pr_volume_textbox.Text = pr_total_volume.ToString();
                }
                else
                {
                    last_time_textbox.Text = "0";
                    pr_volume_textbox.Text = "0";
                    foreach (TextBox textbox in po_pr_textboxes) { textbox.Text = "0"; }
                }

                for (int index = 0; index != workout.exercise_list[index_exercise].exercise_sets; index++)
                {
                    reps_textboxes[index].Text = workout.exercise_list[index_exercise].reps_array[index].ToString();
                    weight_textboxes[index].Text = workout.exercise_list[index_exercise].weight_array[index].ToString();
                }

                if (element_exercises_list.SelectedIndex == element_exercises_list.Items.Count - 1) { finish_button.Visibility = Visibility.Visible; }
                else { finish_button.Visibility = Visibility.Hidden; }


                label_error.Content = "";
                element_exercises_list.IsHitTestVisible = true;
                calculateVolumes(null, null);

        }
            catch (Exception ex) { label_error.Content = "Error!"; element_exercises_list.IsHitTestVisible = false; }

}

        private void saveExercise()
        {
            double total_volume = (Convert.ToDouble(textbox_total_volume.Text));
            workout.exercise_list[index_exercise].total_weight = total_volume;
            Trace.WriteLine(workout.exercise_list[index_exercise].total_weight.ToString());
            for (int index = 0; index != workout.exercise_list[index_exercise].exercise_sets; index++)
            {
                try {
                    double set_weight_results = (Convert.ToDouble(weight_textboxes[index].Text));
                    int set_reps_results = (Convert.ToInt32(reps_textboxes[index].Text));
                    workout.exercise_list[index_exercise].inputWeights(index, set_weight_results, set_reps_results);
                } catch(Exception ex) { }
                    
            }
        }


        private void test_func_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void workoutScreenshot()
        {

            int screenLeft = 341;
            int screenTop = 175;

            int screenWidth;
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

                default:
                    screenWidth = 263;
                    break;
            }
            int screenHeight = 152;


            using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    String filename = workout.exercise_list[index_exercise].exercise_name + ".png";
                    Opacity = .0;
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                    bmp.Save("C:\\Users\\Maxim\\Desktop\\Projects\\C#\\WorkoutLog\\reserved\\" + filename);
                    Opacity = 1;
                }
            }

        }

        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            label_name.Focus();
            workoutScreenshot();
            saveExercise();
            workout.writeWorkout(dB);
            dB.closeConnection();
            Application.Current.Shutdown();
        }

        private void calculateVolumes(object sender, TextChangedEventArgs e)
        {
            double last_time_volume = 0, harder_last_time = 0, pr_total_volume_current = 0, total_volume = 0, pr_total_volume = 0;

            try {
                if (element_exercises_list.Items.Count == workout.exercises_count)
                {
                    void totalVolumeCalc() {
                        total_volume = 0;
                        for (int index = 0; index != workout.exercise_list[index_exercise].exercise_sets; index++)
                        {
                            double total_volume_per_set = Convert.ToInt32(reps_textboxes[index].Text) * Convert.ToDouble(weight_textboxes[index].Text);
                            total_volume_textboxes[index].Text = total_volume_per_set.ToString();
                            total_volume += total_volume_per_set;
                            textbox_total_volume.Text = total_volume.ToString();
                        }
                    } totalVolumeCalc();



                    if (analyzer.db_is_empty == false)
                    {
                        last_time_volume = Convert.ToDouble(last_time_textbox.Text);
                        pr_total_volume = Convert.ToDouble(pr_volume_textbox.Text);

                        harder_last_time = (total_volume - last_time_volume) / last_time_volume;
                        string result_harder_last_time = harder_last_time.ToString("#0.##%");
                        harder_last_time_textbox.Text = result_harder_last_time;


                        pr_total_volume_current = (total_volume - pr_total_volume) / pr_total_volume;
                        string result_pr_total_volume_current = pr_total_volume_current.ToString("#0.##%");
                        textbox_pr_current.Text = result_pr_total_volume_current;

                        calculatePoPr();

                        totalVolumeCalc();
                        harder_last_time = (total_volume - last_time_volume) / last_time_volume;
                        pr_total_volume_current = (total_volume - pr_total_volume) / pr_total_volume;
                    }
                    else
                    {
                        harder_last_time_textbox.Text = "0";
                        textbox_pr_current.Text = "0";
                        foreach (TextBox textbox in po_pr_textboxes) { textbox.Text = "0"; textbox.Height = 20; }
                    }
                    overrideDropsets();
                    changeStatusAdditionalSets();


                }
                label_error.Content = "";
                element_exercises_list.IsHitTestVisible = true;

            }
            catch (Exception ex) { label_error.Content = "Type Error!"; element_exercises_list.IsHitTestVisible = false; }



        }

        private void XValue_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            textbox.Focus();
            textbox.SelectAll();
            e.Handled = true;
        }

        private void element_exercises_list_MouseEnter(object sender, MouseEventArgs e) { label_name.Focus(); }

        public void calculatePoPr()
        {
            double current_pr = Convert.ToDouble(pr_volume_textbox.Text.ToString());
            double current_total_volume = Convert.ToDouble(textbox_total_volume.Text.ToString());
            double last_time_total_volume = Convert.ToDouble(last_time_textbox.Text.ToString());

            for (int index = 0; index != workout.exercise_list[index_exercise].exercise_sets; index++)
            {
                double current_set_weight = Convert.ToDouble(weight_textboxes[index].Text.ToString());
                double set_last_time = Math.Ceiling((((last_time_total_volume - current_total_volume + 1) / current_set_weight) / (workout.exercise_list[index_exercise].exercise_sets - index)));
                double pr_calculation = Math.Ceiling((((current_pr - current_total_volume + 1) / current_set_weight) / (workout.exercise_list[index_exercise].exercise_sets - index)));
                po_pr_textboxes[index].Text = set_last_time.ToString() + "/" + pr_calculation.ToString();
                po_pr_textboxes[index].Height = 20;

            }


        }


        public void initializeTextboxesLists()
        {
            weight_textboxes = new TextBox[]
            {
                set1_weight, set2_weight, set3_weight, set4_weight, set5_weight
            };

            reps_textboxes = new TextBox[]
            {
                set1_reps, set2_reps, set3_reps, set4_reps, set5_reps
            };

            po_pr_textboxes = new TextBox[]
            {
                textbox_set1_po_pr, textbox_set2_po_pr, textbox_set3_po_pr, textbox_set4_po_pr, textbox_set5_po_pr
            };

            total_volume_textboxes = new TextBox[]
            {
                textbox_set1_total_volume, textbox_set2_total_volume, textbox_set3_total_volume, textbox_set4_total_volume, textbox_set5_total_volume
            };


            sets_labels = new TextBox[]
            {
                set1_label, set2_label, set3_label, set4_label, set5_label
            };

        }

        public void updateTextBoxes()
        {
            
            for (int index = 0; index != weight_textboxes.Length; index++)
            {
                weight_textboxes[index].Visibility = Visibility.Hidden;
                reps_textboxes[index].Visibility = Visibility.Hidden;
                po_pr_textboxes[index].Visibility = Visibility.Hidden;
                total_volume_textboxes[index].Visibility = Visibility.Hidden;
                sets_labels[index].Visibility = Visibility.Hidden;
            }

            for (int index = 0; index != workout.exercise_list[index_exercise].exercise_sets; index++)
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


        private void updateIcons(object sender, TextChangedEventArgs e)
        {
            try {
                string str_harder = harder_last_time_textbox.Text.ToString(); str_harder = str_harder.Remove(str_harder.Length - 1); double harder = Convert.ToDouble(str_harder);
                string str_pr = textbox_pr_current.Text.ToString(); str_pr = str_pr.Remove(str_pr.Length - 1); double pr = Convert.ToDouble(str_pr);

                if (harder > 0) { last_time_icon.Visibility = Visibility.Visible; } else { last_time_icon.Visibility = Visibility.Hidden; }
                if (pr > 0) { pr_icon.Visibility = Visibility.Visible; } else { pr_icon.Visibility = Visibility.Hidden; }
            } catch (Exception ex) { last_time_icon.Visibility = Visibility.Hidden; pr_icon.Visibility = Visibility.Hidden; }

        }

        public void dropSets(object sender, MouseButtonEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            dropset_entity = new Dropsets(Array.IndexOf(po_pr_textboxes, textbox));
            clearElementsValues();
            textbox_dropset_current.Text = sets_labels[dropset_entity.set_index].Text;

            dropset_entity.exercise_sets = workout.exercise_list[index_exercise].exercise_sets;

            if (analyzer.db_is_empty == false) {
                dropset_entity.current_pr = Convert.ToDouble(pr_volume_textbox.Text.ToString());
                dropset_entity.current_total_volume = Convert.ToDouble(textbox_total_volume.Text.ToString());
                dropset_entity.last_time_total_volume = Convert.ToDouble(last_time_textbox.Text.ToString());
            }


        }

        public void updateDropSets(object sender, TextChangedEventArgs e)
        {
            if (dropset_entity != null) {
                try {
                    dropset_entity.weight1 = Convert.ToDouble(dropset_weight1_textbox.Text.ToString());
                    dropset_entity.weight2 = Convert.ToDouble(dropset_weight2_textbox.Text.ToString());
                    dropset_list.Items.Clear();
                    if (analyzer.db_is_empty == false && dropset_entity.set_index==workout.exercise_list[index_exercise].exercise_sets-1) { List<string> list_two = dropset_entity.calculateList();
                        foreach (string x in list_two) { dropset_list.Items.Add(x); }
                    }

                    int reps1 = Convert.ToInt32(dropset_reps1_textbox.Text.ToString());
                    int reps2 = Convert.ToInt32(dropset_reps2_textbox.Text.ToString());
                    double total_volume_dropsets = dropset_entity.weight1 * reps1 + dropset_entity.weight2 * reps2;
                    dropset_total_volume_textbox.Text = total_volume_dropsets.ToString();


                    string textbox_override = "DS:" + dropset_entity.weight1.ToString() + "|" + reps1.ToString() + ", " + dropset_entity.weight2.ToString() + "|" + reps2;
                    string unique_key = workout.exercise_list[index_exercise].exercise_name + dropset_entity.set_index;
                    drop_sets_override[unique_key] = textbox_override; drop_sets_values[unique_key] = total_volume_dropsets.ToString();
                }
                catch (Exception ex) { }

            }
        }


        public void overrideDropsets()
        {
            foreach (var element in drop_sets_override)
            {
                foreach (int x in Enumerable.Range(0, workout.exercise_list[index_exercise].exercise_sets))
                {
                    string unique_key = workout.exercise_list[index_exercise].exercise_name + x.ToString();
                    if (element.Key == unique_key) {
                        if (weight_textboxes[x].Text == "1" || weight_textboxes[x].Text == drop_sets_values[unique_key] && reps_textboxes[x].Text == "1")
                        {
                            weight_textboxes[x].Text = drop_sets_values[unique_key]; reps_textboxes[x].Text = "1";
                            po_pr_textboxes[x].Text = element.Value;
                            po_pr_textboxes[x].Height = 39;
                            dropset_entity = null; clearElementsValues();
                        }

                    }
                }
            }

        }

        private void additionalSet(object sender, MouseButtonEventArgs e)
        { 
            if(workout.exercise_list[index_exercise].exercise_sets != ads.addition_sets[index_exercise]) { 
            ads.keys[workout.exercise_list[index_exercise].exercise_name] = false;
            workout.exercise_list[index_exercise].exercise_sets = ads.addition_sets[index_exercise];
            weight_textboxes[ads.initial_sets[index_exercise]].Text = "1";
            
            reps_textboxes[ads.initial_sets[index_exercise]].Text = "1";
            updateExercise(null, null);
            }

        }

        public bool getCurrentAdditionalSetStatus() {
            if (ads.keys.ContainsKey(workout.exercise_list[index_exercise].exercise_name))
                if (ads.keys[workout.exercise_list[index_exercise].exercise_name] == true) return true;
                    else return false;
            else return false;
        }

        public void changeStatusAdditionalSets() {
            if (ads.keys.ContainsKey(workout.exercise_list[index_exercise].exercise_name))
                if (weight_textboxes[ads.initial_sets[index_exercise]].Text != "0" || reps_textboxes[ads.initial_sets[index_exercise]].Text != "0") { 
                    ads.keys[workout.exercise_list[index_exercise].exercise_name] = true;
                    sets_labels[ads.initial_sets[index_exercise]].Background = Brushes.Green;
                    updateAdditionalSet();
                }
                else {ads.keys.Remove((workout.exercise_list[index_exercise].exercise_name).ToString()) ;
                    workout.exercise_list[index_exercise].exercise_sets = ads.initial_sets[index_exercise];
                    Trace.WriteLine("Triggered");
                    updateExercise(null, null);
                }
        }

        public void updateAdditionalSet()
        {
            string str_harder = harder_last_time_textbox.Text.ToString(); str_harder = str_harder.Remove(str_harder.Length - 1); double harder = Convert.ToDouble(str_harder);
            string str_pr = textbox_pr_current.Text.ToString(); str_pr = str_pr.Remove(str_pr.Length - 1); double pr = Convert.ToDouble(str_pr);
            double lt_textbox = Convert.ToDouble(last_time_textbox.Text);
            bool isHarder = harder > 0;
            bool isPr = pr > 0;

            if (isHarder && getCurrentAdditionalSetStatus() == true) {
                textbox_total_volume.Text = (lt_textbox + 0.1).ToString();
                harder_last_time_textbox.Text = harder_last_time_textbox.Text;
                harder_last_time_textbox.Foreground = Brushes.Green;
                if(isPr)
                {
                    textbox_pr_current.Text = textbox_pr_current.Text;
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

        private void dispatcherTimer_Tick(object sender, EventArgs e)
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
                SoundPlayer player = new SoundPlayer(@"C:\Users\Maxim\Desktop\Projects\C#\WorkoutLog\15.wav");
                player.Load();
                player.Play();
            }

            if (timer_interval == 4)
            {
                SoundPlayer player = new SoundPlayer(@"C:\Users\Maxim\Desktop\Projects\C#\WorkoutLog\6.wav");
                player.Load();
                player.Play();
            }

        }

        private void startTimer(object sender, RoutedEventArgs e)
        {
            if (!dt.IsEnabled) {
            timer_interval = Convert.ToInt32(workout.exercise_list[index_exercise].exercise_rest);
            dt.Start();
            }
        
        }

        public void clearElementsValues()
        {
            dropset_list.Items.Clear();
            dropset_weight1_textbox.Text = "0";
            dropset_weight2_textbox.Text = "0";
            dropset_reps1_textbox.Text = "0";
            dropset_reps2_textbox.Text = "0";
            dropset_total_volume_textbox.Text = "0";
            textbox_dropset_current.Text = "";

        }

        public void imageBinding()
        {
            System.IO.File.Copy($"C:/Users/Maxim/Desktop/Projects/C#/WorkoutLog/{workout.workout_name}.png", "C:/Users/Maxim/Desktop/Projects/C#/WorkoutLog/Workout - Current.png", true);
            img_table.Source = null;
            var image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(@"C:\Users\Maxim\Desktop\Projects\C#\WorkoutLog\Workout - Current.png", UriKind.Relative);
            image.EndInit();
            img_table.Source = image;
        }
    }
}
