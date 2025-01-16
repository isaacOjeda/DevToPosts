using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernelLearning02.ApiService.Plugins;

/// <summary>
/// A plugin that returns the current time.
/// </summary>
public class TimeInformationService
{
    [KernelFunction]
    [Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
}