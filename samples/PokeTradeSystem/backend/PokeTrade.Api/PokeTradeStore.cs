namespace PokeTrade.Api;

public sealed class PokeTradeStore
{
    private readonly object _gate = new();
    private readonly List<PokemonCard> _cards =
    [
        new() { Id = 1, Name = "Charizard ex", SetName = "Obsidian Flames", Rarity = "Special Illustration Rare", Price = 149.90m, Stock = 2, ReorderLevel = 3, IsPublished = true, WebEnabled = true, SaleStartsAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), SaleEndsAt = null },
        new() { Id = 2, Name = "Pikachu ex", SetName = "Surging Sparks", Rarity = "Ultra Rare", Price = 42.50m, Stock = 8, ReorderLevel = 4, IsPublished = true, WebEnabled = true, SaleStartsAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), SaleEndsAt = null },
        new() { Id = 3, Name = "Umbreon ex", SetName = "Prismatic Evolutions", Rarity = "Special Illustration Rare", Price = 219.00m, Stock = 1, ReorderLevel = 2, IsPublished = true, WebEnabled = true, SaleStartsAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), SaleEndsAt = null },
        new() { Id = 4, Name = "Mewtwo VSTAR", SetName = "Crown Zenith", Rarity = "Galarian Gallery", Price = 85.00m, Stock = 5, ReorderLevel = 3, IsPublished = true, WebEnabled = true, SaleStartsAt = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero), SaleEndsAt = null }
    ];

    private readonly List<Order> _orders = [];
    private readonly List<WorkPlay> _workPlays = [];
    private readonly List<Delivery> _deliveries = [];
    private int _nextOrderId = 1001;
    private int _nextWorkPlayId = 5001;
    private int _nextDeliveryId = 9001;

    public IReadOnlyList<PokemonCard> GetCards()
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            return _cards
                .Where(card =>
                    card.IsPublished &&
                    card.WebEnabled &&
                    card.SaleStartsAt <= now &&
                    (card.SaleEndsAt == null || now < card.SaleEndsAt) &&
                    card.Stock > 0)
                .Select(CloneCard)
                .ToArray();
        }
    }

    public IReadOnlyList<Order> GetOrders()
    {
        lock (_gate) return _orders.OrderByDescending(order => order.Id).ToArray();
    }

    public IReadOnlyList<WorkPlay> GetWorkPlays()
    {
        lock (_gate) return _workPlays.OrderBy(workPlay => workPlay.Status).ThenBy(workPlay => workPlay.Id).ToArray();
    }

    public IReadOnlyList<Delivery> GetDeliveries()
    {
        lock (_gate) return _deliveries.OrderByDescending(delivery => delivery.Id).ToArray();
    }

    public Order CreateOrder(CreateOrderRequest request)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
                throw new InvalidOperationException("Customer name is required.");
            if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
                throw new InvalidOperationException("Delivery address is required.");
            if (request.Lines.Count == 0)
                throw new InvalidOperationException("An order must contain at least one card.");
            if (request.Lines.Any(line => line.Quantity <= 0))
                throw new InvalidOperationException("Order quantity must be greater than zero.");

            var lines = request.Lines.Select(line =>
            {
                var card = FindCard(line.CardId);
                return new OrderLine { CardId = card.Id, CardName = card.Name, Quantity = line.Quantity, UnitPrice = card.Price };
            }).ToList();

            var order = new Order
            {
                Id = _nextOrderId++,
                CustomerName = request.CustomerName.Trim(),
                DeliveryAddress = request.DeliveryAddress.Trim(),
                Lines = lines
            };
            _orders.Add(order);

            if (CanReserve(order))
            {
                Reserve(order);
                order.Status = OrderStatus.ReadyForDelivery;
                CreateDelivery(order);
            }
            else
            {
                order.Status = OrderStatus.AwaitingStock;
                CreatePurchaseWorkPlays(order);
            }

            return order;
        }
    }

    public WorkPlay StartWorkPlay(int id)
    {
        lock (_gate)
        {
            var workPlay = FindWorkPlay(id);
            if (workPlay.Status != WorkPlayStatus.Open)
                throw new InvalidOperationException("Only an Open WorkPlay can be started.");

            workPlay.Status = WorkPlayStatus.InProgress;
            return workPlay;
        }
    }

    public WorkPlay CompleteWorkPlay(int id, CompleteWorkPlayRequest request)
    {
        lock (_gate)
        {
            var workPlay = FindWorkPlay(id);
            if (workPlay.Status != WorkPlayStatus.InProgress)
                throw new InvalidOperationException("Only an InProgress WorkPlay can be completed.");
            if (request.PurchasedQuantity <= 0)
                throw new InvalidOperationException("Purchased quantity must be greater than zero.");

            var card = FindCard(workPlay.CardId);
            card.Stock += request.PurchasedQuantity;
            workPlay.PurchasedQuantity = request.PurchasedQuantity;
            workPlay.Status = WorkPlayStatus.Completed;
            FulfillWaitingOrders();
            return workPlay;
        }
    }

    public Delivery DispatchDelivery(int id)
    {
        lock (_gate)
        {
            var delivery = FindDelivery(id);
            if (delivery.Status != DeliveryStatus.Pending)
                throw new InvalidOperationException("Only a Pending delivery can be dispatched.");

            var order = FindOrder(delivery.OrderId);
            if (order.Status != OrderStatus.ReadyForDelivery)
                throw new InvalidOperationException("Order must be ReadyForDelivery before dispatch.");

            delivery.Status = DeliveryStatus.Dispatched;
            delivery.DispatchedAt = DateTimeOffset.UtcNow;
            order.Status = OrderStatus.Shipped;
            return delivery;
        }
    }

    public Delivery MarkDeliveryDelivered(int id)
    {
        lock (_gate)
        {
            var delivery = FindDelivery(id);
            if (delivery.Status != DeliveryStatus.Dispatched)
                throw new InvalidOperationException("Only a Dispatched delivery can be marked delivered.");

            var order = FindOrder(delivery.OrderId);
            delivery.Status = DeliveryStatus.Delivered;
            delivery.DeliveredAt = DateTimeOffset.UtcNow;
            order.Status = OrderStatus.Delivered;
            return delivery;
        }
    }

    private void FulfillWaitingOrders()
    {
        foreach (var order in _orders.Where(order => order.Status == OrderStatus.AwaitingStock).OrderBy(order => order.Id))
        {
            if (!CanReserve(order)) continue;
            Reserve(order);
            order.Status = OrderStatus.ReadyForDelivery;
            CreateDelivery(order);
        }
    }

    private void CreatePurchaseWorkPlays(Order order)
    {
        foreach (var line in order.Lines)
        {
            var card = FindCard(line.CardId);
            var shortage = Math.Max(0, line.Quantity - card.Stock);
            if (shortage == 0) continue;

            _workPlays.Add(new WorkPlay
            {
                Id = _nextWorkPlayId++,
                OrderId = order.Id,
                CardId = card.Id,
                CardName = card.Name,
                QuantityToBuy = shortage + card.ReorderLevel,
                Type = "PurchaseStock",
                Reason = $"Order #{order.Id} needs {line.Quantity} but only {card.Stock} are in stock."
            });
        }
    }

    private bool CanReserve(Order order) => order.Lines.All(line => FindCard(line.CardId).Stock >= line.Quantity);

    private void Reserve(Order order)
    {
        foreach (var line in order.Lines)
            FindCard(line.CardId).Stock -= line.Quantity;
    }

    private void CreateDelivery(Order order)
    {
        if (_deliveries.Any(delivery => delivery.OrderId == order.Id)) return;
        _deliveries.Add(new Delivery { Id = _nextDeliveryId++, OrderId = order.Id, Address = order.DeliveryAddress });
    }

    private PokemonCard FindCard(int id) => _cards.SingleOrDefault(card => card.Id == id)
        ?? throw new KeyNotFoundException($"Card {id} was not found.");
    private WorkPlay FindWorkPlay(int id) => _workPlays.SingleOrDefault(workPlay => workPlay.Id == id)
        ?? throw new KeyNotFoundException($"WorkPlay {id} was not found.");
    private Delivery FindDelivery(int id) => _deliveries.SingleOrDefault(delivery => delivery.Id == id)
        ?? throw new KeyNotFoundException($"Delivery {id} was not found.");
    private Order FindOrder(int id) => _orders.SingleOrDefault(order => order.Id == id)
        ?? throw new KeyNotFoundException($"Order {id} was not found.");

    private static PokemonCard CloneCard(PokemonCard card) => new()
    {
        Id = card.Id,
        Name = card.Name,
        SetName = card.SetName,
        Rarity = card.Rarity,
        Price = card.Price,
        Stock = card.Stock,
        ReorderLevel = card.ReorderLevel,
        IsPublished = card.IsPublished,
        WebEnabled = card.WebEnabled,
        SaleStartsAt = card.SaleStartsAt,
        SaleEndsAt = card.SaleEndsAt
    };
}
