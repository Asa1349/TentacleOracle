using TentacleOracle.BusinessLogic;
using TentacleOracle.Models;
using TentacleOracle.Tests.TestData;

namespace TentacleOracle.Tests.BusinessLogic;

[TestFixture]
public sealed class MealPlanGeneratorTests
{
    [TestCaseSource(typeof(MealPlanCases), nameof(MealPlanCases.Availability))]
    public void Generate_RespectsAvailabilityWithoutChangingInput(int available, int requested, int expected)
    {
        var dishes = DishFactory.CreateMany(available);
        var originalOrder = dishes.ToArray();

        var result = new MealPlanGenerator().Generate(dishes, requested);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(expected));
            Assert.That(result, Is.SubsetOf(dishes));
            Assert.That(result, Is.Unique);
            Assert.That(dishes, Is.EqualTo(originalOrder));
        }
    }

    [TestCase(0)]
    [TestCase(4)]
    public void Generate_ZeroRequested_ReturnsEmpty(int available)
    {
        var result = new MealPlanGenerator().Generate(DishFactory.CreateMany(available), 0);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Generate_UsesConfiguredDefault()
    {
        var dishes = DishFactory.CreateMany(AppSettings.DefaultMealCount + 2);
        IMealPlanGenerator generator = new MealPlanGenerator();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(generator.Generate(dishes), Has.Count.EqualTo(AppSettings.DefaultMealCount));
            Assert.That(new MealPlanGenerator().Generate(dishes), Has.Count.EqualTo(AppSettings.DefaultMealCount));
        }
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Generate_NegativeCount_Throws(int requested)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MealPlanGenerator().Generate([], requested));
    }

    [Test]
    public void Generate_NullDishes_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MealPlanGenerator().Generate(null!));
    }
}
