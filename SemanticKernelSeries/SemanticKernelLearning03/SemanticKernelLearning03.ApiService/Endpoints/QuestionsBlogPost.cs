using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Qdrant.Client.Grpc;
using SemanticKernelLearning03.ApiService.Models;

namespace SemanticKernelLearning03.ApiService.Endpoints;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001

public static class QuestionsBlogPost
{
    public record Request(string Query);

    public record Response(string Content);

    public class Handler(Kernel kernel, [FromKeyedServices(BlogPost.VectorName)] ITextSearch textSearch)
    {
        public async Task<Response> Handle(Request request, CancellationToken ct)
        {
            var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");
            kernel.Plugins.Add(searchPlugin);

            string promptTemplate = """
                                    Utiliza solamente los siguientes contenidos para contestar las preguntas realizadas. 
                                    Si no sabes la respuesta, hazle saber al usuario que no se encontró información relevante.

                                    Al final de tu respuesta, incluye el nombre del contenido utilizado como referencia.

                                    {{#with (SearchPlugin-GetTextSearchResults query)}}  
                                        {{#each this}}  
                                        Name: {{Name}}
                                        Value: {{Value}}
                                        -----------------
                                        {{/each}}  
                                    {{/with}}  

                                    Pregunta:

                                    {{query}}
                                    """;

            KernelArguments arguments = new() { { "query", request.Query } };

            HandlebarsPromptTemplateFactory promptTemplateFactory = new();
            var response = await kernel.InvokePromptAsync(
                promptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory,
                cancellationToken: ct);

            return new Response(response.ToString());
        }
    }
}


#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0070