using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace Song_Beater_Map_Mixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer timer = null;
        private FileSystemWatcher watcher=null;
        private readonly int SAMPLE=20;

        public MainWindow()
        {
            InitializeComponent();

        }

        

        private void dirChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                DisposeWatcher();
                if (Directory.GetFiles(dir.Text, "*.ogg").Length > 0)
                {
                    /*                timer = new DispatcherTimer();
                                    timer.Interval = TimeSpan.FromSeconds(5);
                                    timer.Tick += rescan;
                                    timer.Start();
                    */                    

                    watcher = new FileSystemWatcher();
                    watcher.Path = dir.Text;
                    watcher.IncludeSubdirectories = false;
                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                    for (int i = 1; i < 6; i++)
                        watcher.Filters.Add(i + ".json");

                    watcher.Changed += OnChanged;

                    watcher.EnableRaisingEvents = true;

                    same_Checked(null, null);

                    message.Content = "Save level from SongBeaterEditor with different settings";
                }
            } catch {
                messager("Invalid folder");
            }

        }

        private void same_Checked(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            for (int i = 1; i < 6; i++) enableButton(i.ToString());
        }
       
        private void enableButton(string i)
        {
            string[] list = Directory.GetFiles(dir.Text, i + "-*.json");
            if (same.IsChecked == true)
            {
                foreach (string path in list)
                {
                    string fn = Path.GetFileName(path);
                    if (Directory.GetFiles(dir.Text, i + "-" + fn.Split("-")[1] + "-*.json").Length > 1)
                    {
                        button.IsEnabled = true;
                        return;
                    }
                }
            }
            else if (list.Length > 1)
            {
                button.IsEnabled = true;
                return;
            }

        }

        private void DisposeWatcher()
        {
            if (watcher == null) return;
            watcher.Changed -= OnChanged;
            watcher.Dispose();
            watcher = null;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {           

            string i= Path.GetFileName(e.FullPath).Substring(0,1);

            if (!File.Exists(e.FullPath)) return;

            string json = File.ReadAllText(e.FullPath);
            Level level = JsonConvert.DeserializeObject<Level>(json);

            if (level.spheres.Length > SAMPLE - 1)
            {
                String newname = "-" + level.spheres.Length + "-";

                for (int s = 0; s < SAMPLE; s++)
                    if (s < SAMPLE - 1 && level.spheres[s].time == level.spheres[s + 1].time) { newname += level.spheres[s].y.ToString()+ level.spheres[s+1].y; s++; }                   
                    else newname += (level.spheres[s].type == 1 ? "L" : "R")+level.spheres[s].y;

                messager("Renamed " + i + newname + ".json");

                newname = e.FullPath.Substring(0, e.FullPath.Length - 5) + newname + ".json";
                File.Delete(newname);
                File.Move(e.FullPath, newname);
            }

            Dispatcher.Invoke(() => {
                enableButton(i);
            });            
        }

        private void messager(string v)
        {
            Dispatcher.Invoke(() => {               
                message.Content = v;
                message.Foreground = Brushes.Red;
            });

        }

        public class Level
        {
            public string version;
            public Sphere[] spheres;
        }

        public class Sphere
        {
            public float time;
            public int x, y, type;
        }

        private void merge(object sender, RoutedEventArgs e)
        {
            // check for problems

            int cmin, cmax;

            try
            {
                cmin = int.Parse(min.Text);
                cmax = int.Parse(max.Text);
                if (cmin > cmax || cmin < 1 || cmax<1) cmin = cmax / 0;
            } catch
            {
                message.Content = "Incorrect chuck size";
                return;
            }

            if(same.IsChecked==true) for (int i = 1; i < 6; i++) {
                string[] list = Directory.GetFiles(dir.Text, i + "-*.json");
                string lastCount = null;
                foreach (string path in list)
                {
                    string count = Path.GetFileName(path).Split("-")[1];
                    if (lastCount != null && !count.Equals(lastCount))
                    {
                        message.Content = "Remove unmatching orb count for level " + i;
                        return;
                    }
                    lastCount = count;
                }
            }

            string mergedir = dir.Text + (dir.Text.EndsWith("\\") ? "" : "\\") + "merged";
            Directory.CreateDirectory(mergedir);

            // add mp4

            string sb = "",fn="";            
            if (Directory.GetFiles(dir.Text, "sb.json").Length == 1)
            {
                fn = Directory.GetFiles(dir.Text, "sb.json")[0];
                sb = File.ReadAllText(fn);                
            }
            string[] sbsplit = sb.Split(",");

            // mix

            string mixed = "", notmixed = "";

            for (int i = 1; i < 6; i++)
            {
                string[] list = Directory.GetFiles(dir.Text, i + "-*.json");

                if (list.Length > 1)
                {
                    // read inputs

                    Level[] level = new Level[list.Length];

                    int part = 0;
                    float maxorb = 0;
                    foreach (string path in list)
                    {
                        string json = File.ReadAllText(path);
                        level[part++] = JsonConvert.DeserializeObject<Level>(json);
                        maxorb = Math.Max(maxorb, level[part - 1].spheres.Last<Sphere>().time);
                    }

                    // prepare output

                    Level output = new Level();
                    output.version = level[0].version;
                    List<Sphere> spheres = new List<Sphere>();                   

                    // merge

                    Random random = new Random();
                    float lastorb = 0;
                    do
                    {
                        int next = random.Next(cmax - cmin) + cmin;
                        int copy = random.Next(level.Length);

                        int sphere = 0, len = level[copy].spheres.Length;
                        while (sphere < len && level[copy].spheres[sphere].time <= lastorb) sphere++;

                        while (sphere < len && (next > 0 || sphere>0 && level[copy].spheres[sphere].time == level[copy].spheres[sphere - 1].time))
                        {
                            //Debug.WriteLine("sphere="+sphere+" len="+len+" next="+next+" copy="+copy+" time="+ level[copy].spheres[sphere].time);
                            spheres.Add(level[copy].spheres[sphere++]);
                            next--;                           
                        }
                        lastorb = spheres.Last<Sphere>().time;
                    } while (lastorb < maxorb);

                    output.spheres = spheres.ToArray();

                    if (File.Exists(mergedir + "\\" + i + ".json")) 
                        notmixed += " " + i;
                    else
                    {
                        mixed += " " + i;
                        File.WriteAllText(mergedir + "\\" + i + ".json", JsonConvert.SerializeObject(output));

                        // update note counts in sb

                        if (sb.Length>0)
                        {
                            for(int c=0;c<sbsplit.Length;c++) if(sbsplit[c].TrimStart().StartsWith("\"notes"+i+"\""))
                                {
                                    sbsplit[c] = "\"notes" + i + "\":" + spheres.Count;
                                    break;
                                }
                        }
                    }                  
                }
            }
            message.Content = (mixed.Length > 0 ? "Merged" + mixed + " into subfolder 'merged'. " : "") 
                + (notmixed.Length>0 ? "Did not overwrite existing"+notmixed+(mixed.Length>0 ? "":" in 'subfolder 'merged'"):"");

            if (sb.Length > 0) {
                sb = String.Join(",", sbsplit);
                if(Directory.GetFiles(dir.Text, "*.mp4").Length == 1) {
                    string song = Path.GetFileName(Directory.GetFiles(dir.Text, "*.mp4")[0]);
                    sb = sb.Replace("\"video\":\"\"", "\"video\":\"" + song + "\"");
                }
                File.WriteAllText(Path.GetDirectoryName(fn) + "\\merged\\sb.json", sb);
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisposeWatcher();
        }

    }
}
