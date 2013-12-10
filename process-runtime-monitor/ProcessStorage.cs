using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Topshelf.Logging;

namespace process_runtime_monitor
{
    public class ProcessStorage
    {
        private readonly CloudTable processesTable;
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
            var processFromStorage = (ProcessTableEntity)processesTable.Execute(TableOperation.Retrieve<ProcessTableEntity>(process.Name, process.Started.Date.ToString(ProcessTableEntity.RowKeyDateFormat))).Result;
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
                RowKey = p.Started.Date.ToString(ProcessTableEntity.RowKeyDateFormat)
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
            var rowKeyRange = Enumerable.Range(0, end.Subtract(start).Days +1).Select(d => start.AddDays(d)).Select(d => d.ToString(ProcessTableEntity.RowKeyDateFormat)).ToArray();
            var records = processesTable.ExecuteQuery(GetTableQueryFor(processName, start, end));
            return from rk in rowKeyRange
                   join r in records
                   on rk equals r.RowKey into joinedData
                   from r in joinedData.DefaultIfEmpty()
                   select r ?? new ProcessTableEntity {RowKey = rk};
        }

        private TableQuery<ProcessTableEntity> GetTableQueryFor(string processName, DateTime start, DateTime end)
        {
            return
                new TableQuery<ProcessTableEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, processName),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, start.ToString(ProcessTableEntity.RowKeyDateFormat)),
                            TableOperators.And, 
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, end.ToString(ProcessTableEntity.RowKeyDateFormat))
                            )
                        )
                    );
        }
    }
}