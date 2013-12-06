using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace process_runtime_monitor
{
    public class ProcessStorage
    {
        private readonly CloudTable processesTable;
        private const string rowKeyDateFormat = "yyyyMMdd";

        public ProcessStorage()
        {
            var connString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connString);
            var client = storageAccount.CreateCloudTableClient();
            processesTable = client.GetTableReference("Processes");
            processesTable.CreateIfNotExists();
        }

        public void SaveOrUpdateProcess(RunningProcess process)
        {
            //convert to ProcessTableEntity
            //get from stoage if already exists
            var processFromStorage = (ProcessTableEntity)processesTable.Execute(TableOperation.Retrieve<ProcessTableEntity>(process.Name, process.Started.Date.ToString(rowKeyDateFormat))).Result;
            if (processFromStorage == null)
                SaveNewProcess(process);
            else
                UpdateExistingProcess(processFromStorage, process);
        }

        private void UpdateExistingProcess(ProcessTableEntity processFromStorage, RunningProcess p)
        {
            try
            {
                processFromStorage.AddRunTimes(p.Started, p.Stopped);
                processesTable.Execute(TableOperation.Replace(processFromStorage));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception encountered: {0}", e);
            }
        }

        public void SaveNewProcess(RunningProcess p)
        {
            var processToInsert = new ProcessTableEntity
            {
                PartitionKey = p.Name,
                RowKey = p.Started.Date.ToString(rowKeyDateFormat)
            };

            try
            {
                processToInsert.AddRunTimes(p.Started, p.Stopped);
                processesTable.Execute(TableOperation.Insert(processToInsert));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception encountered: {0}", e);
            }
        }
    }
}