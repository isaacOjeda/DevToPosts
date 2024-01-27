using DurableTask.Core;

namespace DurableTask.Api.Workflows;

public class ServiceProviderObjectCreator<T> : ObjectCreator<T>
{
    private readonly Type _prototype;
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderObjectCreator(Type type, IServiceProvider serviceProvider)
    {
        _prototype = type;
        _serviceProvider = serviceProvider;

        Initialize(type);
    }

    public override T Create()
    {
        return (T)_serviceProvider.GetService(_prototype)!;
    }

    private void Initialize(object obj)
    {
        Name = NameVersionHelper.GetDefaultName(obj);
        Version = NameVersionHelper.GetDefaultVersion(obj);
    }
}