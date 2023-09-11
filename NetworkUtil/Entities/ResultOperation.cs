using System;

namespace NetworkUtil
{
    public class ResultOperation
    {
        public bool ProcessedOK { get; set; }

        public Exception Exception { get; set; }

        public String Message { get; set; }
    }
}
