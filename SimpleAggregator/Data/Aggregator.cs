using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
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
    class Aggregation {
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
}
