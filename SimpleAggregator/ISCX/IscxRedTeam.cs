using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator.ISCX {
    class IscxRedTeam : IRedTeam {

        
        public int AnomalyIndex(int timeStamp, string comp) {
            return 0;
        }

        public bool IsUnderAttack(string comp) {
            return true;
        }
    }
}
