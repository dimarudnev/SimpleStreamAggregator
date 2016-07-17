using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    class AttackInfo {
        public int Time { get; set; }
        public string Target { get; set; }
        public int Index { get; set; }
    }
    class LanlRedTeam: IRedTeam {
        List<AttackInfo> activity = new List<AttackInfo>();

        public LanlRedTeam(string path, CalculatorOptions options) {
            using(var fileStream = new FileStream(Path.Combine(path, "redteam.txt"), FileMode.Open)) {
                using(var streamReader = new StreamReader(fileStream)) {
                    while(!streamReader.EndOfStream) {
                        string line = streamReader.ReadLine();
                        string[] lineParts = line.Split(',');
                        var time = Convert.ToInt32(lineParts[0]);
                        var target = lineParts[3];
                        if(time > options.StartTime) {
                            IEnumerable<AttackInfo> attacksOnTarget = activity.Where(info => info.Target == target);
                            if(attacksOnTarget.FirstOrDefault(info => info.Time == time) == null) {
                                activity.Add(new AttackInfo {
                                    Time = time,
                                    Index = attacksOnTarget.Count(),
                                    Target = target
                                });
                            }
                        }
                    }
                }
            }
        }

        public int AnomalyIndex(int timeStamp, string comp) {
            AttackInfo anomaly = this.activity.LastOrDefault(info => info.Time <= timeStamp && info.Target == comp);
            return anomaly == null ? -1: anomaly.Index;
        }
        public bool IsUnderAttack(string comp) {
            return this.activity.FirstOrDefault(info => info.Target == comp) != null;
        }
    }
}
