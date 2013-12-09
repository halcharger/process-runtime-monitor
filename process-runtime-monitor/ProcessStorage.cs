using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Topshelf.Logging;

namespace process_runtime_monitor
{
    public class ProcessStorage
    {
        private readonly CloudTable processesTable;
        private const string rowKeyDateFormat = "yyyyMMdd";
        static readonly LogWriter logger = HostLogger.Get<ProcessStorage>();

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
                logger.Error(e);
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
                logger.Error(e);
            }
        }

        public IEnumerable<ProcessTableEntity> GetProcessesFor(string processName, DateTime start, DateTime end)
        {
            var dateRange = Enumerable.Range(0, end.Subtract(start).Days +1).Select(d => start.AddDays(d)).ToArray();
            var retrieveOperations = dateRange.Select(d => TableOperation.Retrieve<ProcessTableEntity>(processName, d.ToString(rowKeyDateFormat)));
            return retrieveOperations.Select(op => (ProcessTableEntity)processesTable.Execute(op).Result).Where(r => r != null);
        }
    }
}