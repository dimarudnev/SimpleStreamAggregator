using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Concurrent;


namespace SimpleAggregator {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
        }

        private void button1_Click(object sender, EventArgs e) {

            DialogResult dr = this.openFileDialog1.ShowDialog();

            if(dr == DialogResult.OK) {
                //textBox1.Text = this.openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            if(!backgroundWorker1.IsBusy) {
                backgroundWorker1.RunWorkerAsync(CreateOptions());
            }
        }

        private CalculatorOptions CreateOptions() {
            return new CalculatorOptions {
                StartTime = ToInt32Safe(this.textBox1.Text),
                TimeFrame = ToInt32Safe(this.textBox4.Text),
                FrameCount = ToInt32Safe(this.textBox5.Text),
                ResultFileName = this.textBox6.Text,
                BasisCount = ToInt32Safe(this.textBox2.Text),
                Basis = this.textBox3.Text.Split(','),
                Comment = this.textBox7.Text

            };
        }
        int ToInt32Safe(string str) {
            try {
                return Convert.ToInt32(str);
            } catch {
                return 0;
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            if(backgroundWorker1.IsBusy) {
                label4.Text = "Cancelling...";
                backgroundWorker1.CancelAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            var options = (CalculatorOptions)e.Argument;
            BackgroundWorker bw = sender as BackgroundWorker;
            IModuleBase module = new LanlModule();
            bw.ReportProgress(0, "Prepare RedTeam info...");
            IRedTeam redteam = module.CreateRedTeam(options);
            bw.ReportProgress(0, "Reading and calculating...");
            var aggregator = new Aggregator(options, redteam);

            var readers = module.CreateReaders(options, aggregator);
            for(int i = 0; i < options.FrameCount; i++) {
                aggregator.Begin();
                List<Task> tasks = new List<Task>();
                foreach(ReaderBase reader in readers) {
                    tasks.Add(Task.Factory.StartNew(() => reader.ReadNextTimeStamp(i)));
                }
                Task.WaitAll(tasks.ToArray());
                bw.ReportProgress((int)(((i + 1) * 100) / options.FrameCount));
                aggregator.End(i);
            }
            readers.ForEach(reader => reader.Dispose());

            bw.ReportProgress(0, "Writing result...");


            var resultFile = Path.Combine(module.DestinationPath, options.ResultFileName + ".csv");
            using(var fileStream = new FileStream(resultFile, FileMode.Create)) {
                using(var streamWriter = new StreamWriter(fileStream)) {
                    aggregator.WriteResult(streamWriter);
                }
            }

            WriteExpirementInfo(module.DestinationPath, options, readers.Select(reader => reader.GetType().Name), aggregator.Basis);
        }

        private void WriteExpirementInfo(string destinationPath, CalculatorOptions options, IEnumerable<string> sourceReaders, List<string> basis) {
            var infoFilePath = Path.Combine(destinationPath, options.ResultFileName + ".info.txt");
            using(FileStream fs = new FileStream(infoFilePath, FileMode.Create)) {
                using(StreamWriter sw = new StreamWriter(fs)) {
                    sw.WriteLine("Source Files: {0}", String.Join(",", sourceReaders));

                    sw.WriteLine("Start time: {0}", options.StartTime);
                    sw.WriteLine("End time: {0}", options.EndTime);
                    sw.WriteLine("Time frame lenght: {0}", options.TimeFrame);
                    sw.WriteLine("Time frame count: {0}", options.FrameCount);
                    sw.WriteLine("Basis count: {0}", options.BasisCount);
                    sw.WriteLine("Basis: {0}", String.Join(",", basis));

                    sw.WriteLine("Comment: {0}", options.Comment);
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if(e.Cancelled) {
                this.progressBar1.Value = 0;
                this.label4.Text = "Cancelled";
            } else {
                this.label4.Text = String.Format("Ready!");
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            this.progressBar1.Value = e.ProgressPercentage;
            this.label10.Text = e.ProgressPercentage + "%";
            if(e.UserState != null) {
                this.label4.Text = e.UserState.ToString();
            }
        }
        private void Form1_Load(object sender, EventArgs e) {

        }

        private void textBox3_TextChanged(object sender, EventArgs e) {

        }

        private void textBox7_TextChanged(object sender, EventArgs e) {

        }

        private void label7_Click(object sender, EventArgs e) {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            RecalculateTimeWindow();
        }
        private void RecalculateTimeWindow() {
            var options = CreateOptions();
            label8.Text = string.Format("{0}-{1}", options.StartTime, options.EndTime);
        }

        private void textBox4_TextChanged(object sender, EventArgs e) {
            RecalculateTimeWindow();
        }

        private void textBox5_TextChanged(object sender, EventArgs e) {
            RecalculateTimeWindow();
        }

        private void label10_Click(object sender, EventArgs e) {

        }
    }
}
