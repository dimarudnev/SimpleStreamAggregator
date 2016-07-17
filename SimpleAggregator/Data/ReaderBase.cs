using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    abstract class ReaderBase {

        public abstract void ReadNextTimeStamp(int timeIndex);
        public abstract void Dispose();
    }
}
