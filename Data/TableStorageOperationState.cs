using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data
{
    public class TableStorageOperationState
    {
        public string OperationType { get; set; }
        public string ErrorCode { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public bool IsBatch { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsComplete { get; set; }
        public List<object> Entries { get; set; }
        public int StatusCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public String ElapsedTime { get; set; }
        public int BatchCount { get; set; }
    }
}
