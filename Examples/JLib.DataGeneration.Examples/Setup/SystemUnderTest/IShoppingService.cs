using JLib.DataGeneration.Examples.Setup.Models;

namespace JLib.DataGeneration.Examples.Setup.SystemUnderTest;

public interface IShoppingService
{
    public void AddArticleToCart(CustomerId customerId, ArticleId articleId, int quantity);
}