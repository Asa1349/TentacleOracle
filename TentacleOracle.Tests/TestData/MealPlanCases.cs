using TentacleOracle.Models;

namespace TentacleOracle.Tests.TestData;

public enum MealPlanOperation
{
    Load,
    Add,
    Fill
}

public static class MealPlanCases
{
    public static IEnumerable<MealPlanOperation> Operations => Enum.GetValues<MealPlanOperation>();

    // Shared by generator tests and plan-filling tests.
    public static IEnumerable<TestCaseData> Availability
    {
        get
        {
            yield return new TestCaseData(0, 3, 0);
            yield return new TestCaseData(1, 1, 1);
            yield return new TestCaseData(2, 3, 2);
            yield return new TestCaseData(3, 3, 3);
            yield return new TestCaseData(6, 3, 3);
            yield return new TestCaseData(AppSettings.MaximumMealCount, AppSettings.MaximumMealCount,
                AppSettings.MaximumMealCount);
        }
    }
}

