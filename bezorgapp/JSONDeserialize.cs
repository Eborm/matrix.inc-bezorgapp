public class DeliveryService
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class DeliveryState
{
    public int Id { get; set; }
    public int State { get; set; }
    public DateTime DateTime { get; set; }
    public int OrderId { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public List<DeliveryState?> DeliveryStates { get; set; }
}
