using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VehicleExpenseAPI.Filters;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToList();

        if (!fileParameters.Any())
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>(),
                        Required = new HashSet<string>()
                    }
                }
            }
        };

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

        foreach (var param in context.MethodInfo.GetParameters())
        {
            if (param.ParameterType == typeof(IFormFile))
            {
                schema.Properties[param.Name!] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
                schema.Required.Add(param.Name!);
            }
            else
            {
                schema.Properties[param.Name!] = new OpenApiSchema
                {
                    Type = GetSchemaType(param.ParameterType)
                };
            }
        }
    }

    private static string GetSchemaType(Type type)
    {
        return type.Name switch
        {
            "Int32" => "integer",
            "String" => "string",
            "Boolean" => "boolean",
            "Decimal" => "number",
            _ => "string"
        };
    }
}
