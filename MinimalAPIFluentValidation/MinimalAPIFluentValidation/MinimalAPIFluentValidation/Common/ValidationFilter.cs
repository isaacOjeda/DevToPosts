using FluentValidation;
using MinimalAPIFluentValidation.Common.Attributes;
using System.Net;
using System.Reflection;

namespace MinimalAPIFluentValidation.Common;

public static class ValidationFilter
{
    /// <summary>
    /// Filter Factory
    /// 
    /// Si en el Endpoint actual existe [Validator] y AbstractValidator asociados,
    /// se ejecuta el "Endpoint Filter"
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public static EndpointFilterDelegate ValidationFilterFactory(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        IEnumerable<ValidationDescriptor> validationDescriptors = GetValidators(context.MethodInfo, context.ApplicationServices);

        if (validationDescriptors.Any())
        {
            return invocationContext => Validate(validationDescriptors, invocationContext, next);
        }

        // dejar pasar
        return invocationContext => next(invocationContext);
    }

    /// <summary>
    /// Endpoint Filter que valida cualquier objeto con [Validate] y sus AbstractValidator
    /// </summary>
    /// <param name="validationDescriptors"></param>
    /// <param name="invocationContext"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    private static async ValueTask<object?> Validate(IEnumerable<ValidationDescriptor> validationDescriptors, EndpointFilterInvocationContext invocationContext, EndpointFilterDelegate next)
    {
        foreach (ValidationDescriptor descriptor in validationDescriptors)
        {
            var argument = invocationContext.Arguments[descriptor.ArgumentIndex];

            if (argument is not null)
            {
                var validationResult = await descriptor.Validator.ValidateAsync(
                    new ValidationContext<object>(argument)
                );

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary(),
                        statusCode: (int)HttpStatusCode.UnprocessableEntity);
                }
            }
        }

        return await next.Invoke(invocationContext);
    }

    /// <summary>
    /// Busca los validadores de cualquier clase en los parámetros
    /// que tenga el atributo [Validate]
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    static IEnumerable<ValidationDescriptor> GetValidators(MethodInfo methodInfo, IServiceProvider serviceProvider)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();

        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];

            if (parameter.GetCustomAttribute<ValidateAttribute>() is not null)
            {
                Type validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);

                // Note that FluentValidation validators needs to be registered as singleton
                IValidator? validator = serviceProvider.GetService(validatorType) as IValidator;

                if (validator is not null)
                {
                    yield return new ValidationDescriptor { ArgumentIndex = i, Validator = validator };
                }
            }
        }
    }


    private class ValidationDescriptor
    {
        public required int ArgumentIndex { get; init; }
        public required IValidator Validator { get; init; }
    }
}
