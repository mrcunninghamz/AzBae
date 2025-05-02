namespace GUI.Models.FunctionApps;

public enum ListMyFunctionAppsActionTypes
{
    Initialize,
    Validate,
    ListFunctionApps,
    ListFunctionAppsProgress,
    ListFunctionAppsFinished,
    FilterApplied,
    FilterCleared,
    AppInsightsRequested,
    OpenAppInsights,
    Error
}