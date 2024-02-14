using JLib.DataGeneration.Examples.Setup.Models;
using JLib.Helper;

namespace JLib.DataGeneration.Examples.Setup.SystemUnderTest;

public class ShoppingServiceMock : IShoppingService
{
    private readonly IIdRegistry _idRegistry;
    private readonly Dictionary<CustomerId, CustomerEntity> _customers = new();
    private readonly Dictionary<OrderId, OrderEntity> _orders = new();
    private readonly Dictionary<OrderItemId, OrderItemEntity> _orderItems = new();
    private readonly Dictionary<ArticleId, ArticleEntity> _articles = new();
    private readonly Dictionary<CustomerId, OrderId> _carts = new();

    public IReadOnlyDictionary<CustomerId, CustomerEntity> Customers => _customers;
    public IReadOnlyDictionary<OrderId, OrderEntity> Orders => _orders;
    public IReadOnlyDictionary<OrderItemId, OrderItemEntity> OrderItems => _orderItems;
    public IReadOnlyDictionary<ArticleId, ArticleEntity> Articles => _articles;
    public IReadOnlyDictionary<CustomerId, OrderId> Carts => _carts;

    public ShoppingServiceMock(IIdRegistry idRegistry)
    {
        _idRegistry = idRegistry;
    }

    /*
     *                      Mock Utility
     */

    public void AddCustomer(CustomerEntity customer)
        => _customers.Add(customer.Id, customer);

    public void AddArticles(params ArticleEntity[] articles)
    {
        foreach (var article in articles)
            _articles.Add(article.Id, article);
    }
    public void AddOrderItems(params OrderItemEntity[] orderItems)
    {
        foreach (var orderItem in orderItems)
            _orderItems.Add(orderItem.Id, orderItem);
    }
    public void AddOrders(params OrderEntity[] orders)
    {
        foreach (var order in orders)
            _orders.Add(order.Id, order);
    }
    public void SetCart(CustomerId customerId, OrderId orderId)
        => _carts[customerId] = orderId;


    /*
     *                      Interface Methods
     */

    public void AddArticleToCart(CustomerId customerId, ArticleId articleId, int quantity)
    {
        if (_customers.ContainsKey(customerId) == false)
            throw new UnauthorizedAccessException("customer not found");
        if (_articles.ContainsKey(articleId) == false)
            throw new KeyNotFoundException("article not found");

        var cartId = _carts.TryGetValue(customerId);

        if (cartId is null)
        {
            var orderIdIdentifier = new DataPackageValues.IdIdentifier(
                new(nameof(ShoppingServiceMock)),
                new(customerId.Value.IdInfo(_idRegistry))
            );

            var newCart = new OrderEntity(customerId, OrderStatus.Cart)
            {
                Id = new OrderId(_idRegistry.GetGuidId(orderIdIdentifier))
            };
            _orders.Add(newCart.Id, newCart);
            _carts.Add(customerId, newCart.Id);
            cartId = newCart.Id;
        }

        var orderItemIdIdentifier = new DataPackageValues.IdIdentifier(
            new(nameof(ShoppingServiceMock)),
            new(customerId.Value.IdInfo(_idRegistry))
        );

        var article = _articles[articleId];
        var orderItem = new OrderItemEntity(cartId, articleId, quantity, article.Price)
        {
            Id = new OrderItemId(_idRegistry.GetGuidId(orderItemIdIdentifier))
        };
        _orderItems.Add(orderItem.Id, orderItem);
    }
}