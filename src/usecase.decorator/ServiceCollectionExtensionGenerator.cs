﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace usecase.decorator;

[Generator(LanguageNames.CSharp)]
public class ServiceCollectionExtensionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) => context.RegisterSourceOutput(context.CompilationProvider, action);

    private void action(SourceProductionContext context, Compilation compilation)
    {
        var classDeclarationSyntaxList = compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodesAndSelf()
                                    .OfType<RecordDeclarationSyntax>()
                                    .Where(syntax => syntax.AttributeLists.SelectMany(attributeList => attributeList.Attributes).Any(attribute => attribute.Name.ToString() == "GenerateDecorator"))
                                    .Select(syntax => new { tree, syntax })
        );

        var diList = classDeclarationSyntaxList.Select(o =>
        {
            var model = compilation.GetSemanticModel(o.tree);
            var typeSymbol = model.GetDeclaredSymbol(o.syntax);
            var isymbol = typeSymbol?.Interfaces.First();

            return $$"""
                    services.AddKeyedTransient<{{isymbol?.ContainingNamespace.Name}}.{{isymbol?.Name}}, {{typeSymbol?.ContainingNamespace.Name}}.{{typeSymbol?.Name}}>("{{typeSymbol?.Name}}Base");
                    services.AddTransient<{{isymbol?.ContainingNamespace.Name}}.{{isymbol?.Name}}, {{typeSymbol?.ContainingNamespace.Name}}.{{typeSymbol?.Name}}Decorator>();
                    """;
        });

        var code = $$"""
            // <auto-generated/>
            #nullable enable

            using Microsoft.Extensions.DependencyInjection;

            namespace usecase;

            public static class ServiceCollectionExtension
            {
                public static IServiceCollection AddUsecaseDecorator(this IServiceCollection services)
                {
            {{String.Join("\r\n", diList)}}
                      
                    return services;
                }
            }

           """;
        context.AddSource($"ServiceCollectionExtension.cs", code);
    }
}