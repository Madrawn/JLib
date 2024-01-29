using JLib.DataGeneration.Examples.Models;

namespace JLib.DataGeneration.Examples.SystemUnderTest;

public interface IShoppingService
{
    public void AddArticleToCart(CustomerId customerId, ArticleId articleId, int quantity);
}