using TentacleOracle.BusinessLogic;
using TentacleOracle.Tests.TestData;

namespace TentacleOracle.Tests.BusinessLogic;

[TestFixture]
public sealed class DishRatingCalculatorTests
{
    [TestCase(0, 0, null)]
    [TestCase(2, 0, 2d)]
    [TestCase(0, 3, 3d)]
    [TestCase(2, 3, 2.5d)]
    [TestCase(3, 2, 2.5d)]
    [TestCase(1, 1, 1d)]
    [TestCase(5, 5, 5d)]
    [TestCase(1, 5, 3d)]
    public void GetAverageRating_IgnoresUnratedValues(int ratingA, int ratingR, double? expected)
    {
        var dish = DishFactory.Create(ratingA: ratingA, ratingR: ratingR);

        Assert.That(dish.GetAverageRating(), Is.EqualTo(expected));
    }

    [Test]
    public void GetAverageRating_NullDish_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => DishRatingCalculator.GetAverageRating(null!));
    }
}
