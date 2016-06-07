using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    class RedTeam {
        Dictionary<string, int> activity = new Dictionary<string, int>();

        public RedTeam(CalculatorOptions options) {
            using(var fileStream = new FileStream( Path.Combine(options.SourcePath, "redteam.txt"), FileMode.Open)) {
                using(var streamReader = new StreamReader(fileStream)) {
                    string line = streamReader.ReadLine();
                    string[] lineParts = line.Split(',');
                    var time = Convert.ToInt32(lineParts[0]);
                    var target = lineParts[3];
                    if(!activity.ContainsKey(target)) {
                        activity.Add(target, time);
                    }
                }
            }
        }

        public bool HasAnomaly(int timeStamp, string comp) {
            int anomalyTime = -1;
            if(activity.TryGetValue(comp, out anomalyTime)) {
                return timeStamp >= anomalyTime;
            }
            return false;
        }
    }
}
