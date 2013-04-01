using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace Data
{
    public class CallbackData
    {
        public TableServiceContext Context { get; set; }
        public TableStorageOperationState State { get; set; }
    }
}
