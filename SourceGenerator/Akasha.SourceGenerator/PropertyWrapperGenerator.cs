using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akasha.SourceGenerator
{
    [Generator]
    public class PropertyWrapperGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            foreach (var candidateClass in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(candidateClass.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(candidateClass);

                if (classSymbol == null)
                    continue;

                // Check if class has [GenerateWrappers] attribute
                var hasAttribute = classSymbol.GetAttributes()
                    .Any(ad => ad.AttributeClass?.Name == "GenerateWrappersAttribute");

                if (!hasAttribute)
                    continue;

                // Find BaseEntity<M> base type to get Model type
                var modelType = GetModelTypeFromBaseEntity(classSymbol);
                if (modelType == null)
                    continue;

                // Generate wrapper properties
                var source = GenerateWrapperClass(classSymbol, modelType, context);
                if (!string.IsNullOrEmpty(source))
                {
                    context.AddSource($"{classSymbol.Name}_Generated.g.cs", SourceText.From(source, Encoding.UTF8));
                }
            }
        }

        private INamedTypeSymbol? GetModelTypeFromBaseEntity(INamedTypeSymbol classSymbol)
        {
            var baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                // Check if this is BaseEntity<M>
                if (baseType.Name == "BaseEntity" && baseType.IsGenericType)
                {
                    return baseType.TypeArguments[0] as INamedTypeSymbol;
                }
                baseType = baseType.BaseType;
            }
            return null;
        }

        private string GenerateWrapperClass(INamedTypeSymbol classSymbol, INamedTypeSymbol modelType, GeneratorExecutionContext context)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();

            var properties = new List<PropertyInfo>();

            // Find all RxVar<T>, RxMod<T>, RxComputed<T> fields in Model
            foreach (var member in modelType.GetMembers())
            {
                if (member is not IFieldSymbol field)
                    continue;

                var fieldType = field.Type as INamedTypeSymbol;
                if (fieldType == null || !fieldType.IsGenericType)
                    continue;

                var typeName = fieldType.Name;
                var isRxVar = typeName == "RxVar";
                var isRxMod = typeName == "RxMod";
                var isRxComputed = typeName == "RxComputed";

                if (!isRxVar && !isRxMod && !isRxComputed)
                    continue;

                var valueType = fieldType.TypeArguments[0];
                var propertyName = field.Name;
                var isReadOnly = isRxComputed;

                properties.Add(new PropertyInfo
                {
                    Name = propertyName,
                    Type = valueType.ToDisplayString(),
                    FieldName = field.Name,
                    IsReadOnly = isReadOnly
                });
            }

            if (properties.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    public partial class {classSymbol.Name}");
            sb.AppendLine("    {");

            foreach (var prop in properties)
            {
                sb.AppendLine($"        public {prop.Type} {prop.Name}");
                sb.AppendLine("        {");
                sb.AppendLine($"            get => Model.{prop.FieldName}.Value;");

                if (!prop.IsReadOnly)
                {
                    sb.AppendLine($"            set => Model.{prop.FieldName}.SetValue(value, this);");
                }

                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private class PropertyInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string FieldName { get; set; } = string.Empty;
            public bool IsReadOnly { get; set; }
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclaration
                    && classDeclaration.AttributeLists.Count > 0
                    && classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }
}
