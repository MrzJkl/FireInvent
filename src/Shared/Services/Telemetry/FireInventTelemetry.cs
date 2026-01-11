using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FireInvent.Shared.Services.Telemetry;

/// <summary>
/// Provides telemetry instrumentation for FireInvent services
/// </summary>
public sealed class FireInventTelemetry : IDisposable
{
    public const string ServiceName = "FireInvent.Api";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    
    private readonly Meter _meter;
    
    // Counters
    public Counter<long> ItemsCreatedCounter { get; }
    public Counter<long> ItemsAssignedCounter { get; }
    public Counter<long> MaintenanceRecordsCounter { get; }
    public Counter<long> OrdersCreatedCounter { get; }
    public Counter<long> VisitsCreatedCounter { get; }
    
    // Histograms
    public Histogram<double> DatabaseQueryDuration { get; }
    public Histogram<long> ItemsPerOrder { get; }
    public Histogram<long> ItemsPerVisit { get; }
    
    // Gauges (using ObservableGauge)
    private long _activeItems;
    private long _pendingOrders;
    
    public FireInventTelemetry()
    {
        _meter = new Meter(ServiceName, "1.0.0");
        
        ItemsCreatedCounter = _meter.CreateCounter<long>(
            "fireinvent.items.created",
            unit: "items",
            description: "Total number of inventory items created");
            
        ItemsAssignedCounter = _meter.CreateCounter<long>(
            "fireinvent.items.assigned",
            unit: "assignments",
            description: "Total number of item assignments");
            
        MaintenanceRecordsCounter = _meter.CreateCounter<long>(
            "fireinvent.maintenance.records",
            unit: "records",
            description: "Total number of maintenance records created");
            
        OrdersCreatedCounter = _meter.CreateCounter<long>(
            "fireinvent.orders.created",
            unit: "orders",
            description: "Total number of orders created");
            
        VisitsCreatedCounter = _meter.CreateCounter<long>(
            "fireinvent.visits.created",
            unit: "visits",
            description: "Total number of visits created");
        
        // Initialize histograms
        DatabaseQueryDuration = _meter.CreateHistogram<double>(
            "fireinvent.database.query.duration",
            unit: "ms",
            description: "Duration of database queries");
            
        ItemsPerOrder = _meter.CreateHistogram<long>(
            "fireinvent.orders.items",
            unit: "items",
            description: "Number of items per order");
            
        ItemsPerVisit = _meter.CreateHistogram<long>(
            "fireinvent.visits.items",
            unit: "items",
            description: "Number of items per visit");
        
        // Initialize observable gauges
        _meter.CreateObservableGauge(
            "fireinvent.items.active",
            () => _activeItems,
            unit: "items",
            description: "Current number of active inventory items");
            
        _meter.CreateObservableGauge(
            "fireinvent.orders.pending",
            () => _pendingOrders,
            unit: "orders",
            description: "Current number of pending orders");
    }
    
    public void UpdateActiveItems(long count)
    {
        _activeItems = count;
    }
    
    public void UpdatePendingOrders(long count)
    {
        _pendingOrders = count;
    }
    
    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }
    
    public void Dispose()
    {
        _meter?.Dispose();
    }
}
