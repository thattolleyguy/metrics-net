using System;
using System.Collections.Generic;
using Metrics.Core;

namespace Metrics.CLR
{
    /// <summary>
    /// A convenience class for installing global, machine-level metrics
    /// <seealso href="http://technet.microsoft.com/en-us/library/cc768048.aspx#XSLTsection132121120120" />
    /// <seealso href="http://msdn.microsoft.com/en-us/library/w8f5kw2e%28v=VS.71%29.aspx" />
    /// </summary>
    public class MachineMetrics : MetricSet
    {
        private readonly IDictionary<MetricName, IMetric> _metrics;

        private const string TotalInstance = "_Total";
        private const string GlobalInstance = "_Global_";

        public IDictionary<MetricName, IMetric> Metrics
        {
            get
            {
                return _metrics;
            }
        }

        public static MachineMetrics Create(MachineMetricsCategory category = MachineMetricsCategory.All)
        {
            var metrics = new MachineMetrics();
            if (category.HasFlag(MachineMetricsCategory.PhysicalDisk))
                metrics.InstallPhysicalDisk();

            if (category.HasFlag(MachineMetricsCategory.LogicalDisk))
                metrics.InstallLogicalDisk();

            if (category.HasFlag(MachineMetricsCategory.LocksAndThreads))
                metrics.InstallClrLocksAndThreads();

            if (category.HasFlag(MachineMetricsCategory.Memory))
                metrics.InstallClrMemory();

            return metrics;
        }

        private MachineMetrics()
        {
            _metrics = new Dictionary<MetricName, IMetric>();
        }
        public void InstallPhysicalDisk()
        {
            _metrics.Add(new MetricName("physical_disk.current_disk_queue_length"), PerformanceCounterGauge.create("PhysicalDisk", "Current Disk Queue Length", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.avg_disk_queue_length"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk Queue Length", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.disk_read_queue_length"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk Read Queue Length", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.disk_write_queue_length"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk Write Queue Length", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.percent_disk_time"), PerformanceCounterGauge.create("PhysicalDisk", "% Disk Time", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.percent_disk_read_time"), PerformanceCounterGauge.create("PhysicalDisk", "% Disk Read Time", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.percent_disk_write_time"), PerformanceCounterGauge.create("PhysicalDisk", "% Disk Write Time", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.avg_disk_seconds_per_transfer"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk sec/Transfer", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.avg_disk_seconds_per_read"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk sec/Read", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.avg_disk_seconds_per_write"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk sec/Write", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.disk_transfers_per_second"), PerformanceCounterGauge.create("PhysicalDisk", "Disk Transfers/sec", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.disk_reads_per_second"), PerformanceCounterGauge.create("PhysicalDisk", "Disk Reads/sec", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.disk_writes_per_second"), PerformanceCounterGauge.create("PhysicalDisk", "Disk Writes/sec", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.disk_bytes_per_second"), PerformanceCounterGauge.create("PhysicalDisk", "Disk Bytes/sec", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.disk_read_bytes_per_second"), PerformanceCounterGauge.create("PhysicalDisk", "Disk Read Bytes/sec", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.disk_write_bytes_per_second"), PerformanceCounterGauge.create("PhysicalDisk", "Disk Write Bytes/sec", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.avg_disk_bytes_per_transfer"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk Bytes/Transfer", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.avg_disk_bytes_per_read"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk Bytes/Read", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.avg_disk_bytes_per_write"), PerformanceCounterGauge.create("PhysicalDisk", "Avg. Disk Bytes/Write", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.percent_idle_time"), PerformanceCounterGauge.create("PhysicalDisk", "% Idle Time", TotalInstance));
            _metrics.Add(new MetricName("physical_disk.split_io_per_second"), PerformanceCounterGauge.create("PhysicalDisk", "Split IO/Sec", TotalInstance));
        }

        public void InstallLogicalDisk()
        {
            _metrics.Add(new MetricName("logical_disk.current_disk_queue_length"), PerformanceCounterGauge.create("LogicalDisk", "Current Disk Queue Length", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.avg_disk_queue_length"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk Queue Length", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.disk_read_queue_length"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk Read Queue Length", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.disk_write_queue_length"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk Write Queue Length", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.percent_disk_time"), PerformanceCounterGauge.create("LogicalDisk", "% Disk Time", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.percent_disk_read_time"), PerformanceCounterGauge.create("LogicalDisk", "% Disk Read Time", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.percent_disk_write_time"), PerformanceCounterGauge.create("LogicalDisk", "% Disk Write Time", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.avg_disk_seconds_per_transfer"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk sec/Transfer", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.avg_disk_seconds_per_read"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk sec/Read", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.avg_disk_seconds_per_write"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk sec/Write", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.disk_transfers_per_second"), PerformanceCounterGauge.create("LogicalDisk", "Disk Transfers/sec", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.disk_reads_per_second"), PerformanceCounterGauge.create("LogicalDisk", "Disk Reads/sec", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.disk_writes_per_second"), PerformanceCounterGauge.create("LogicalDisk", "Disk Writes/sec", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.disk_bytes_per_second"), PerformanceCounterGauge.create("LogicalDisk", "Disk Bytes/sec", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.disk_read_bytes_per_second"), PerformanceCounterGauge.create("LogicalDisk", "Disk Read Bytes/sec", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.disk_write_bytes_per_second"), PerformanceCounterGauge.create("LogicalDisk", "Disk Write Bytes/sec", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.avg_disk_bytes_per_transfer"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk Bytes/Transfer", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.avg_disk_bytes_per_read"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk Bytes/Read", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.avg_disk_bytes_per_write"), PerformanceCounterGauge.create("LogicalDisk", "Avg. Disk Bytes/Write", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.percent_idle_time"), PerformanceCounterGauge.create("LogicalDisk", "% Idle Time", TotalInstance));
            _metrics.Add(new MetricName("logical_disk.split_io_per_second"), PerformanceCounterGauge.create("LogicalDisk", "Split IO/Sec", TotalInstance));
        }

        public void InstallClrLocksAndThreads()
        {
            _metrics.Add(new MetricName("clr_locks_and_threads.total_number_of_contentions"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "Total # of Contentions", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.contention_rate_per_second"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "Contention Rate / sec", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.current_queue_length"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "Current Queue Length", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.queue_length_peak"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "Queue Length Peak", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.queue_length_per_second"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "Queue Length / sec", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.number_of_current_logical_threads"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "# of current logical Threads", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.number_of_current_physical_threads"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "# of current physical Threads", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.number_of_current_recognized_threads"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "# of current recognized threads", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.number_of_total_recognized_threads"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "# of total recognized threads", GlobalInstance));
            _metrics.Add(new MetricName("clr_locks_and_threads.rate_or_recognized_threads_per_second"), PerformanceCounterGauge.create(".NET CLR LocksAndThreads", "rate of recognized threads / sec", GlobalInstance));
        }

        public void InstallClrMemory()
        {
            _metrics.Add(new MetricName("clr_memory.number_of_gen_0_collections"), PerformanceCounterGauge.create(".NET CLR Memory", "# Gen 0 Collections", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.number_of_gen_1_collections"), PerformanceCounterGauge.create(".NET CLR Memory", "# Gen 1 Collections", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.number_of_gen_2_collections"), PerformanceCounterGauge.create(".NET CLR Memory", "# Gen 2 Collections", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.promoted_memory_from_gen_0"), PerformanceCounterGauge.create(".NET CLR Memory", "Promoted Memory from Gen 0", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.promoted_memory_from_gen_1"), PerformanceCounterGauge.create(".NET CLR Memory", "Promoted Memory from Gen 1", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.gen_0_promoted_bytes_per_second"), PerformanceCounterGauge.create(".NET CLR Memory", "Gen 0 Promoted Bytes/Sec", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.gen_1_promoted_bytes_per_second"), PerformanceCounterGauge.create(".NET CLR Memory", "Gen 1 Promoted Bytes/Sec", GlobalInstance));

            //_Global_:.NET CLR Memory:Promoted Finalization-Memory from Gen 0
            //_Global_:.NET CLR Memory:Process ID

            _metrics.Add(new MetricName("clr_memory.gen_0_heap_size"), PerformanceCounterGauge.create(".NET CLR Memory", "Gen 0 heap size", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.gen_1_heap_size"), PerformanceCounterGauge.create(".NET CLR Memory", "Gen 1 heap size", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.gen_2_heap_size"), PerformanceCounterGauge.create(".NET CLR Memory", "Gen 2 heap size", GlobalInstance));

            //_Global_:.NET CLR Memory:Large Object Heap size
            //_Global_:.NET CLR Memory:Finalization Survivors

            _metrics.Add(new MetricName("clr_memory.number_of_gc_handles"), PerformanceCounterGauge.create(".NET CLR Memory", "# GC Handles", GlobalInstance));

            //_Global_:.NET CLR Memory:Allocated Bytes/sec
            //_Global_:.NET CLR Memory:# Induced GC
            //_Global_:.NET CLR Memory:% Time in GC
            //_Global_:.NET CLR Memory:Not Displayed

            _metrics.Add(new MetricName("clr_memory.number_of_bytes_in_all_heaps"), PerformanceCounterGauge.create(".NET CLR Memory", "# Bytes in all Heaps", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.total_number_of_committed_bytes"), PerformanceCounterGauge.create(".NET CLR Memory", "# Total committed Bytes", GlobalInstance));
            _metrics.Add(new MetricName("clr_memory.total_number_of_reserved_bytes"), PerformanceCounterGauge.create(".NET CLR Memory", "# Total reserved Bytes", GlobalInstance));

            //_Global_:.NET CLR Memory:# of Pinned Objects
            //_Global_:.NET CLR Memory:# of Sink Blocks in use
        }
    }
}