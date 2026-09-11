namespace PokeTrade.Api;

public enum OrderStatus
{
    Pending,
    AwaitingStock,
    ReadyForDelivery,
    Shipped,
    Delivered,
    Cancelled
}

public enum WorkPlayStatus
{
    Open,
    InProgress,
    Completed,
    Cancelled
}

public enum DeliveryStatus
{
    Pending,
    Dispatched,
    Delivered
}

public sealed class PokemonCard
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string SetName { get; init; }
    public required string Rarity { get; init; }
    public required decimal Price { get; init; }
    public required int ReorderLevel { get; init; }
    public int Stock { get; set; }
}

public sealed class OrderLine
{
    public required int CardId { get; init; }
    public required string CardName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}

public sealed class Order
{
    public required int Id { get; init; }
    public required string CustomerName { get; init; }
    public required string DeliveryAddress { get; init; }
    public required List<OrderLine> Lines { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public decimal Total => Lines.Sum(line => line.Quantity * line.UnitPrice);
}

public sealed class WorkPlay
{
    public required int Id { get; init; }
    public required int OrderId { get; init; }
    public required int CardId { get; init; }
    public required string CardName { get; init; }
    public required int QuantityToBuy { get; init; }
    public required string Type { get; init; }
    public required string Reason { get; init; }
    public WorkPlayStatus Status { get; set; } = WorkPlayStatus.Open;
    public int PurchasedQuantity { get; set; }
}

public sealed class Delivery
{
    public required int Id { get; init; }
    public required int OrderId { get; init; }
    public required string Address { get; init; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public DateTimeOffset? DispatchedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
}

public sealed record CreateOrderLineRequest(int CardId, int Quantity);
public sealed record CreateOrderRequest(string CustomerName, string DeliveryAddress, List<CreateOrderLineRequest> Lines);
public sealed record CompleteWorkPlayRequest(int PurchasedQuantity);
