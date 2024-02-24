using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TaskManagerApp;


namespace TaskManagerApp
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ProcessItem> processItems;
        private PerformanceCounter cpuCounter;

        public MainWindow()
        {
            InitializeComponent();
            RefreshProcessList();
            InitializeCPUCounter();
            InitializeTimer();
        }

        private async void createBtn_Click(object sender, RoutedEventArgs e)
        {
            string processName = createTxt.Text.Trim();
            if (!string.IsNullOrEmpty(processName))
            {
                blackBox2.Items.Add(processName);
                StartProcess(processName); 

            
                await Task.Delay(3000);

         
                var process = Process.GetProcessesByName(processName).FirstOrDefault();
                if (process != null && !process.HasExited)
                {
                    try
                    {
                        process.Kill();
                        MessageBox.Show($"Process '{processName}' ended successfully after 3 seconds.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshProcessList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to end process '{processName}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid process name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void InitializeCPUCounter()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        private void InitializeTimer()
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            double cpuUsage = cpuCounter.NextValue();
            CPUProgressBar.Value = cpuUsage;
        }

        private void blackBox3_Click(object sender, RoutedEventArgs e)
        {
            string processName = blackBox1.Text.Trim();
            if (!string.IsNullOrEmpty(processName))
            {
                blackBox2.Items.Add(processName);
            }
            else
            {
                MessageBox.Show("Please enter a valid process name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartProcess(string processName)
        {
            try
            {
                Process.Start(processName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start process '{processName}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EndProcessButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem != null)
            {
                var processItem = (ProcessItem)ProcessList.SelectedItem;
                var process = processItem.Process;
                try
                {
                    process.Kill();
                    MessageBox.Show("Process terminated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    processItem.Process.Refresh();
                    RefreshProcessList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to terminate process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a process to terminate.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshProcessList()
        {
            ProcessList.Items.Clear();
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle))
                .Take(20)
                .OrderBy(p => p.ProcessName);

            foreach (var process in processes)
            {
                ProcessList.Items.Add(new ProcessItem(process));
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcessList();
        }

        public class ProcessItem
        {
            public Process Process { get; }

            public string Name => Process.ProcessName;
            public int Id => Process.Id;
            public string MainWindowTitle => Process.MainWindowTitle;
            public string StartTime => Process.StartTime.ToString();
            public TimeSpan TotalProcessorTime => Process.TotalProcessorTime;
            public string MemoryUsage => $"{Process.WorkingSet64 / (1024 * 1024)} MB";

            public ProcessItem(Process process)
            {
                Process = process;
            }

            public override string ToString()
            {
                return $"{Name} (ID: {Id}) - {MainWindowTitle}";
            }
        }
    }
}
