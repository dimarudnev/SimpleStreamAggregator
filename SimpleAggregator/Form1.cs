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
                textBox1.Text = this.openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            if(!backgroundWorker1.IsBusy) {
                backgroundWorker1.RunWorkerAsync(new CalculatorOptions {
                    SourcePath = @"D:\Data\LANL\source",
                    DestinationPath = @"D:\Data\LANL\",
                    TimeFrame = Convert.ToInt32(this.textBox4.Text),
                    FrameCount = Convert.ToInt32(this.textBox5.Text),
                    ResultFileName = this.textBox6.Text
                });
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
            var calc = new Aggregator();
            bw.ReportProgress(0, "Reading and calculating...");
            var readers = new List<ReaderBase> {
                new ProcReader(options, calc),
                new DnsReader(options, calc),
                new FlowsReader(options, calc)
            };
            for(int i = 0; i < options.FrameCount; i++) {
                calc.Begin();
                List<Task> tasks = new List<Task>();
                foreach(ReaderBase reader in readers) {
                    tasks.Add(Task.Factory.StartNew(() => reader.ReadNextTimeStamp(i)));
                }
                Task.WaitAll(tasks.ToArray());
                bw.ReportProgress((int)(((i + 1) * 100) / options.FrameCount));
                calc.End(i);
            }
            readers.ForEach(reader => reader.Dispose());

            bw.ReportProgress(0, "Writing result...");
            var resultFile = options.DestinationPath + options.ResultFileName + ".csv";
            using(var fileStream = new FileStream(resultFile, FileMode.Create)) {
                using(var streamWriter = new StreamWriter(fileStream)) {
                    calc.WriteResult(streamWriter);
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
            if(e.UserState != null) {
                this.label4.Text = e.UserState.ToString();
            }
        }
        private void Form1_Load(object sender, EventArgs e) {

        }
    }
}
