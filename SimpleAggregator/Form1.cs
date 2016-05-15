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
            var calc = new Aggregator(bw, options);


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
            var resultFile = options.DestinationPath + options.ResultFileName + ".csv";
            using(var fileStream = new FileStream(resultFile, FileMode.Create)) {
                using(var streamWriter = new StreamWriter(fileStream)) {
                    calc.WriteResult(streamWriter);
                }
            }
            e.Cancel = calc.Cancelled;
            e.Result = calc.Result;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if(e.Cancelled) {
                this.progressBar1.Value = 0;
                this.label4.Text = "Cancelled";
            } else {
                this.label4.Text = String.Format("Ready! Line count: {0}.", e.Result.ToString());
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
    class CalculatorOptions {
        public string ResultFileName { get; set; }
        public string SourcePath { get; set; }
        public int TimeFrame { get; set; }
        public int FrameCount { get; set; }
        public string DestinationPath { get; internal set; }
    }
    class Aggregator : CalculatorBase {
        List<EventsInfo> basis = null;
        List<Aggregation> aggregations = new List<Aggregation>();

        ConcurrentDictionary<string, EventsInfo> current = new ConcurrentDictionary<string, EventsInfo>();

        public Aggregator(BackgroundWorker worker, CalculatorOptions options) : base(worker, options) {
            
        }

        public override void Begin() {
            current.Clear();
        }
        public override void End(int timeIndex) {
            if(basis == null) {
                basis = new List<EventsInfo>() {
                    current.Values.ElementAt(0),
                    current.Values.ElementAt(100)
                };
            }
            int[,] matrix = new int[current.Keys.Count, basis.Count];
            int i = 0, j = 0;
            foreach(KeyValuePair<string, EventsInfo> item in current) {
                foreach(EventsInfo basisItem in basis) {
                    matrix[i, j++] = EventsInfo.CalcDictance(item.Value, basisItem);
                }
                i++;
                j = 0;
            }
            aggregations.Add(new Aggregation() {
                Index = timeIndex,
                Matrix = matrix,
                RowNames = current.Keys.ToArray()
            });
            current.Clear();
        }
        public override void WriteResult(StreamWriter writer) {
            foreach(Aggregation info in aggregations) {
                //writer.WriteLine("========= Time: {0} ({1} - {2})==========", info.Key, info.Value.StartTime, info.Value.EndTime);
                info.WriteToFile(writer, worker);
            }
            
        }

        static object locker = new { };
        public override void AddValue(string rowName, string[] colNames) {
            foreach(var colName in colNames) {
                current.GetOrAdd(rowName, (key) => new EventsInfo()).Increment(colName);
            }
        }
    }
    class Aggregation  {
        public int Index { get; set; }
        public string[] RowNames { get; set; }
        public int[,] Matrix { get; set; }

        public void WriteToFile(StreamWriter writer, BackgroundWorker worker) {
            var dim1 = Matrix.GetUpperBound(0) + 1;
            var dim2 = Matrix.GetUpperBound(1) + 1;
            for(int i = 0; i < dim1; i++) {
                writer.Write(string.Format("{0},", Index));
                writer.Write(string.Format("{0},", RowNames[i]));
                for(int j = 0; j < dim2; j++) {
                    if(j != 0)
                        writer.Write(",");
                    writer.Write(Matrix[i, j]);
                }
                writer.WriteLine();
            }
        }
    }
    class EventsInfo : ConcurrentDictionary<string, int> {

        public void Increment(string value) {
            this.AddOrUpdate(value, 1, (key, oldValue) => { return oldValue + 1; });
        }
        public static int CalcDictance(EventsInfo value1, EventsInfo value2) {
            var distance = 0;
            var intersectKeys = value1.Keys.Intersect(value2.Keys);
            var uniqueKeysForValue1 = value1.Keys.Except(intersectKeys);
            var uniqueKeysForValue2 = value2.Keys.Except(intersectKeys);
            foreach(string key in intersectKeys) {
                distance += (int)Math.Pow(value1[key] - value2[key], 2);
            }
            foreach(string key in uniqueKeysForValue1) {
                distance += (int)Math.Pow(value1[key], 2);
            }
            foreach(string key in uniqueKeysForValue2) {
                distance += (int)Math.Pow(value2[key], 2);
            }
            return (int)Math.Sqrt(distance);
        }
    }
    
    class CalculatorBase {
        protected CalculatorOptions options;
        protected BackgroundWorker worker;
        int lineCount = 0;
        

        public bool Cancelled { get { return worker.CancellationPending; } }
        public object Result { get { return lineCount; } }

        public CalculatorBase(BackgroundWorker worker, CalculatorOptions options) {
            this.worker = worker;
            this.options = options;
        }
        void ReportProgress(Stream stream) {
            worker.ReportProgress((int)((100 * stream.Position) / stream.Length));
        }
        
        public virtual void WriteResult(StreamWriter writer) {
        }

        public virtual void Begin() {

        }
        public virtual void End(int timeIndex) {

        }


        public virtual void AddValue(string rowName, string[] colNames) { 
        }
    }

    abstract class ReaderBase {
        CalculatorBase calculator;
        FileStream fileStream;
        StreamReader streamReader;
        CalculatorOptions options;

        protected abstract string FileName { get; }

        public ReaderBase(CalculatorOptions options, CalculatorBase calculator) {
            this.options = options;
            this.calculator = calculator;
            fileStream = new FileStream(Path.Combine(options.SourcePath, FileName), FileMode.Open);
            streamReader = new StreamReader(fileStream);

        }

        protected virtual int GetTimeStamp(string[] lineParts) {
            return Convert.ToInt32(lineParts[0]);
        }
        protected abstract string GetRowValue(string[] lineParts);
        protected abstract string[] GetColumnValues(string[] lineParts);

        public void ReadNextTimeStamp(int timeIndex) {
            while(!streamReader.EndOfStream) {
                string line = streamReader.ReadLine();
                string[] lineParts = line.Split(',');
                var timeStamp = GetTimeStamp(lineParts);
                int currentTimeIndex = (int)(timeStamp / this.options.TimeFrame);
                if(timeIndex == currentTimeIndex) {
                    calculator.AddValue(GetRowValue(lineParts), GetColumnValues(lineParts));
                } else {
                    break;
                }
            }
        }
    }
    class ProcReader : ReaderBase {
        protected override string FileName { get { return "proc.txt"; } }

        public ProcReader(CalculatorOptions options, CalculatorBase calculator): base(options, calculator) {

        }

        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[1] { lineParts[3] };
        }

        protected override string GetRowValue(string[] lineParts) {
            return lineParts[2];
        }
    }
    class DnsReader : ReaderBase {
        protected override string FileName { get { return "dns.txt"; } }

        public DnsReader(CalculatorOptions options, CalculatorBase calculator) : base(options, calculator) {

        }

        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[1] { "dns" + lineParts[2] };
        }

        protected override string GetRowValue(string[] lineParts) {
            return lineParts[1];
        }
    }
    class FlowsReader : ReaderBase {
        protected override string FileName { get { return "flows.txt"; } }

        public FlowsReader(CalculatorOptions options, CalculatorBase calculator) : base(options, calculator) {

        }

        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[1] { "flows" + lineParts[4] + lineParts[5] };
        }

        protected override string GetRowValue(string[] lineParts) {
            return lineParts[2];
        }
    }

}
