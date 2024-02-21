using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
