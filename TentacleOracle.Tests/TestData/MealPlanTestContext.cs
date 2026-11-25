using Moq;
using TentacleOracle.BusinessLogic;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Tests.TestData;

internal sealed class MealPlanTestContext
{
    public MealPlanTestContext()
    {
        DishService.Setup(service => service.GetDishesAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken token) =>
            {
                token.ThrowIfCancellationRequested();
                return Task.FromResult(DishCatalog.ToList());
            });
        SettingsService.Setup(service => service.GetMealCountAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken token) =>
            {
                token.ThrowIfCancellationRequested();
                return Task.FromResult(MealCount);
            });
        Generator.Setup(generator => generator.Generate(It.IsAny<IReadOnlyList<Dish>>(), It.IsAny<int>()))
            .Returns((IReadOnlyList<Dish> dishes, int count) => dishes.Take(count).ToArray());

        Plan = new MealPlanService(DishService.Object, SettingsService.Object, Generator.Object);
    }

    public Mock<IDishService> DishService { get; } = new(MockBehavior.Strict);
    public Mock<ISettingsService> SettingsService { get; } = new(MockBehavior.Strict);
    public Mock<IMealPlanGenerator> Generator { get; } = new(MockBehavior.Strict);
    public List<Dish> DishCatalog { get; set; } = DishFactory.CreateMany(4);
    public int MealCount { get; set; } = AppSettings.DefaultMealCount;
    public MealPlanService Plan { get; }

    public Task RunAsync(MealPlanOperation operation, CancellationToken cancellationToken = default) => operation switch
    {
        MealPlanOperation.Load => Plan.LoadAsync(cancellationToken),
        MealPlanOperation.Add => Plan.AddDishAsync(1, cancellationToken),
        MealPlanOperation.Fill => Plan.FillAsync(cancellationToken),
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };
}