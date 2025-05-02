using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using AutoMapper;
using AzBae.Core.Configuration;
using AzBae.Core.Services;
using Azure.Identity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using FluentValidation;
using Microsoft.Extensions.Options;
using Terminal.Gui;

namespace GUI.Models.FunctionApps;

public partial class ListMyFunctionAppsViewModel : BaseViewModel<ListMyFunctionAppsActionTypes>
{
    private readonly IMapper _mapper;
    private readonly IFunctionAppService _functionAppService;
    private readonly IOptions<ResourceFilterSettings>? _filterSettings;
    
    [ObservableProperty]
    private ObservableCollection<FunctionApp> _functionApps = new();
    
    [ObservableProperty]
    private ObservableCollection<FunctionApp> _allFunctionApps = new();
    
    [ObservableProperty]
    private string _filterPattern = string.Empty;
    
    [ObservableProperty]
    private bool _isFilterActive;
    
    [ObservableProperty]
    private string _filterStatusMessage = string.Empty;
    
    [ObservableProperty]
    private string _appInsightsUrl = string.Empty;
    
    public ListMyFunctionAppsViewModel(IMapper mapper, IFunctionAppService functionAppService, IOptions<ResourceFilterSettings>? filterSettings = null)
    {
        _mapper = mapper;
        _functionAppService = functionAppService;
        _filterSettings = filterSettings;
        if (_filterSettings?.Value?.FunctionAppFilterPattern != null)
        {
            FilterPattern = _filterSettings.Value.FunctionAppFilterPattern;
        }
    }
    
    public override void Initialize()
    {
        SendMessage(ListMyFunctionAppsActionTypes.Initialize);
    }
    
    public override void Validate()
    {
        // Validation for filter pattern could go here
    }
    
    [RelayCommand]
    public async Task GetInstrumentationKey(string functionAppId)
    {
        var functionApp = FunctionApps.First(x => x.Id == functionAppId);
        SendMessage(ListMyFunctionAppsActionTypes.AppInsightsRequested);
        var url = await _functionAppService.GetAppInsightsUrl(functionApp.Id, functionApp.Name);
        if (!string.IsNullOrEmpty(url))
        {
            AppInsightsUrl = url;
            SendMessage(ListMyFunctionAppsActionTypes.OpenAppInsights);
        }
        else
        {
            Errors = "Failed to retrieve instrumentation key.";
            SendMessage(ListMyFunctionAppsActionTypes.Error);
        }
    }
    
    [RelayCommand]
    public void ApplyFilter()
    {
        IsFilterActive = !string.IsNullOrWhiteSpace(FilterPattern);
        
        if (!IsFilterActive)
        {
            ClearFilter();
            return;
        }
        
        try
        {
            var regex = new Regex(FilterPattern, RegexOptions.IgnoreCase);
            var filteredList = AllFunctionApps.Where(app => regex.IsMatch(app.Name) ||  regex.IsMatch(app.ResourceGroup)).ToList();
            
            FunctionApps.Clear();
            FunctionApps.AddRange(filteredList);
            
            FilterStatusMessage = $"Showing {FunctionApps.Count} of {AllFunctionApps.Count} function apps (filtered by '{FilterPattern}')";
            SendMessage(ListMyFunctionAppsActionTypes.FilterApplied);
        }
        catch (ArgumentException ex)
        {
            Errors = $"Invalid regular expression pattern: {ex.Message}";
            SendMessage(ListMyFunctionAppsActionTypes.Error);
        }
    }
    
    [RelayCommand]
    public void ClearFilter()
    {
        FunctionApps.Clear();
        foreach (var app in AllFunctionApps)
        {
            FunctionApps.Add(app);
        }
        
        IsFilterActive = false;
        FilterStatusMessage = string.Empty;
        
        SendMessage(ListMyFunctionAppsActionTypes.FilterCleared);
    }
    
    // Mock method to load function apps - in a real implementation, this would call Azure SDK
    [RelayCommand]
    public async Task LoadFunctionApps()
    {
        SendMessage(ListMyFunctionAppsActionTypes.ListFunctionAppsProgress);
        
        var functionApps = await _functionAppService.GetFunctionAppsAsync();
        var apps = functionApps.Select(x => _mapper.Map<FunctionApp>(x));
        
        AllFunctionApps.Clear();
        foreach (var app in apps)
        {
            AllFunctionApps.Add(app);
        }
        
        // Apply initial filter if we have a pattern
        if (!string.IsNullOrWhiteSpace(FilterPattern))
        {
            ApplyFilter();
        }
        else
        {
            // Otherwise show all
            ClearFilter();
        }
        
        SendMessage(ListMyFunctionAppsActionTypes.ListFunctionAppsFinished);
    }
    
    protected override void DisposeManaged()
    {
        // Clean up managed resources
    }
    
    protected override void DisposeUnmanaged()
    {
        // Clean up unmanaged resources
    }
}

public class FunctionAppViewModelValidator : AbstractValidator<ListMyFunctionAppsViewModel>
{
    public FunctionAppViewModelValidator()
    {
        // Could add validation for filter pattern if needed
    }
}