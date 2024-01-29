using JLib.DataGeneration.Examples.Models;
using JLib.DataGeneration.Examples.SystemUnderTest;

namespace JLib.DataGeneration.Examples.DataPackages;

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public sealed class ArticleDp : DataPackage
{
    public ArticleId FirstArticle { get; init; }
    public ArticleId SecondArticle { get; init; }
    public ArticleId ThirdArticle { get; init; }
    public ArticleId FourthArticle { get; init; }

    public ArticleDp(IDataPackageManager packageManager, ShoppingServiceMock shoppingService) : base(packageManager)
    {
        shoppingService.AddArticles(
            new(nameof(FirstArticle), 10)
            {
                Id = FirstArticle
            },
            new(nameof(SecondArticle), 20.20)
            {
                Id = SecondArticle
            },
            new(nameof(ThirdArticle), 30)
            {
                Id = ThirdArticle
            },
            new(nameof(FourthArticle), 40.40)
            {
                Id = FourthArticle
            }
        );
    }
}