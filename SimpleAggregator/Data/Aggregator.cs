using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    class Aggregator {
        private CalculatorOptions options;
        List<EventsInfo> basis = null;
        List<string> basisKeys = new List<string>();
        List<Aggregation> aggregations = new List<Aggregation>();

        ConcurrentDictionary<string, EventsInfo> current = new ConcurrentDictionary<string, EventsInfo>();

        public List<string> Basis {
            get {
                return basisKeys;
            }
        }

        public Aggregator(CalculatorOptions option) {
            this.options = option;
        }

        public void Begin() {
            current.Clear();
        }
        void GenerateBasis() {
            basis = new List<EventsInfo>();
            for(int i = 0; i < options.BasisCount; i++) {
                EventsInfo basisElement = null;
                if(options.Basis.Length > i) {
                    var basisKey = options.Basis[i];
                    if(current.ContainsKey(basisKey)) {
                        basisElement = current[basisKey];
                        basisKeys.Add(basisKey);
                    }
                } 
                if(basisElement == null) {
                    basisElement = current.Values.ElementAt(i);
                    basisKeys.Add(current.Keys.ElementAt(i));
                }
                basis.Add(basisElement);
            }
        }
        public void End(int timeIndex) {
            if(basis == null) {
                GenerateBasis();
            }
            int[,] matrix = new int[current.Keys.Count, basis.Count];
            int i = 0, j = 0;
            foreach(EventsInfo item in current.Values) {
                item.Normalize();
            }
            foreach(EventsInfo item in current.Values) {
                foreach(EventsInfo basisItem in basis) {
                    matrix[i, j++] = EventsInfo.CalcDictance(item, basisItem);
                }
                i++;
                j = 0;
            }
            var startTime = options.StartTime + options.TimeFrame * timeIndex;
            aggregations.Add(new Aggregation() {
                Index = timeIndex,
                Matrix = matrix,
                RowNames = current.Keys.ToArray(),
                StartTime = startTime,
                EndTime = startTime + options.TimeFrame -1,
            });
            current.Clear();
        }
        public void WriteResult(StreamWriter writer, RedTeam redTeam) {
            writer.Write("Time,Comp,Anomaly");
            for(int i = 0; i < Basis.Count; i++) {
                writer.Write(",V{0}", i);
            }
            writer.WriteLine();
            foreach(Aggregation info in aggregations) {
                info.WriteToFile(writer, redTeam);
            }

        }

        public void AddValue(string rowName, string[] colNames) {
            foreach(var colName in colNames) {
                current.GetOrAdd(rowName, (key) => new EventsInfo()).Increment(colName);
            }
        }
    }
    class Aggregation {
        public int StartTime { get; set; }
        public int EndTime { get; set; }

        public int Index { get; set; }
        public string[] RowNames { get; set; }
        public int[,] Matrix { get; set; }

        public void WriteToFile(StreamWriter writer, RedTeam redTeam) {
            var dim1 = Matrix.GetUpperBound(0) + 1;
            var dim2 = Matrix.GetUpperBound(1) + 1;
            for(int i = 0; i < dim1; i++) {
                var row = RowNames[i];
                writer.Write(string.Format("{0},", Index));
                writer.Write(string.Format("{0},", row));
                writer.Write(string.Format("{0},", redTeam.HasAnomaly(EndTime, row)? "1" : "0"));
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
            this.AddOrUpdate(value, 1, (key, oldValue) => {
                return oldValue + 1;
            });
        }

        public void Normalize() {
            int sum = Values.Sum();
            foreach(var key in Keys.ToList()) { 
                this[key] = (int)(((double)this[key]*100) / sum);
            }
        }

        public static int CalcDictance(EventsInfo value1, EventsInfo value2) {
            const double pow = 2;
            double distance = 0;
            var intersectKeys = value1.Keys.Intersect(value2.Keys);
            var uniqueKeysForValue1 = value1.Keys.Except(intersectKeys);
            var uniqueKeysForValue2 = value2.Keys.Except(intersectKeys);
            foreach(string key in intersectKeys) {
                distance += Math.Pow(Math.Abs(value1[key] - value2[key]), pow);
            }
            foreach(string key in uniqueKeysForValue1) {
                distance += Math.Pow(Math.Abs(value1[key]), pow);
            }
            foreach(string key in uniqueKeysForValue2) {
                distance += Math.Pow(Math.Abs(value2[key]), pow);
            }
            return (int)Math.Pow(distance, 1D/ pow);
        }
    }
}
