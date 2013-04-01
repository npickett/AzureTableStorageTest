using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;
using Microsoft.WindowsAzure.StorageClient;

namespace Data
{
    [DataServiceKey("PartitionKey", "RowKey")]
    public class TableEntry : TableServiceEntity
    {
        public byte[] Data { get; set; }

        public TableEntry(string partitionKey, byte[] data)
        {
            PartitionKey = partitionKey;
            RowKey = Guid.NewGuid().ToString();
            Data = data;
        }

        public TableEntry()
        {
        }
    }
}
