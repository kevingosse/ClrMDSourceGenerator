using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Diagnostics.Runtime;

namespace ClrMDSourceGenerator
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
    {
        private const string AttributeSource = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MemoryDumpAttribute : Attribute
{
    public MemoryDumpAttribute(string source, params string[] filters)
    {
        Source = source;
        Filters = filters;
    }

    public string Source { get; set; }

    public string[] Filters { get; set; }
}
";

        public static StringBuilder Output = new StringBuilder();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(c => c.AddSource("MemoryDumpAttribute", AttributeSource));

            context.SyntaxProvider.
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            context.RegisterForPostInitialization(pi => pi.AddSource("MemoryDumpAttribute", AttributeSource));
            context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is AttributeSyntaxReceiver syntaxReceiver))
            {
                return;
            }

            foreach (var value in syntaxReceiver.Attributes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                Output.AppendLine($"{value.dump} - {string.Join(", ", value.namespaces)}");
                GenerateEntities(value.dump, value.namespaces, context);
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            GenerateDebug(context);
        }

        public void GenerateDebug(GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
using System;
namespace ClrMDSourceGenerator
{
    public class Test 
    {
        public static string Result = @""{0}"";
    }
}
");
            context.AddSource("Debug", SourceText.From(sourceBuilder.ToString().Replace("{0}", Output.ToString().Replace("\"", "\"\"")), Encoding.UTF8));
        }

        public void GenerateEntities(string dumpFile, IReadOnlyList<string> namespaces, GeneratorExecutionContext context)
        {
            GenerateDumpLoader(dumpFile, context);

            context.CancellationToken.ThrowIfCancellationRequested();

            var dataTarget = DataTarget.LoadDump(dumpFile);
            var runtime = dataTarget.ClrVersions[0].CreateRuntime();

            var allTypes = GetAllTypes(runtime)
                .Where(t =>
                {
                    if (namespaces.Count > 0)
                    {
                        bool match = false;

                        foreach (var filter in namespaces)
                        {
                            if (Regex.IsMatch(t.Name, filter))
                            {
                                match = true;
                                break;
                            }
                        }

                        return match;
                    }

                    return true;
                })
                .Distinct(new TypeEqualityComparer());

            var pendingTypes = new List<ClrType>(allTypes);

            var processedTypes = new HashSet<ClrType>();

            while (pendingTypes.Count > 0)
            {
                var currentTypes = pendingTypes;
                pendingTypes = new List<ClrType>();

                foreach (var type in currentTypes)
                {
                    if (!processedTypes.Add(type))
                    {
                        continue;
                    }

                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (type.Name == string.Empty) continue;
                    if (type.IsArray) continue;
                    if (type.IsFree) continue;
                    if (type.Name.Contains("<")) continue;
                    if (type.Name.Contains("+")) continue;

                    if (type.ElementType >= ClrElementType.Boolean && type.ElementType < ClrElementType.Pointer && !type.IsEnum && !type.IsString) continue;

                    File.AppendAllText(@"E:\alltypes.txt", type.Name + "\r\n");

                    pendingTypes.AddRange(GenerateType(type, context));
                }
            }
        }

        public void GenerateDumpLoader(string dumpFile, GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
using Microsoft.Diagnostics.Runtime;

namespace Generated
{
    public static class DumpLocator
    {
        private static ClrHeap _heap;

        public static ClrHeap GetHeap()
        {
            if (_heap == null)
            {
                var dataTarget = DataTarget.LoadDump(@""{dumpFile}"");
                var runtime = dataTarget.ClrVersions[0].CreateRuntime();
                _heap = runtime.Heap;
            }

            return _heap;
        }
    }
}
");

            var result = sourceBuilder.ToString().Replace("{dumpFile}", dumpFile);

            context.AddSource("Generated.DumpLocator", SourceText.From(result, Encoding.UTF8));
        }


        public IEnumerable<ClrType> GenerateType(ClrType type, GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
namespace Generated{namespace}
{
    public class {typeName}
    {
        private ulong _address;

        {fields}

        public {typeName}(ulong address)
        {
            _address = address;
        }
    }
}
");

            var fields = new StringBuilder();

            var allFields = new HashSet<string>();

            foreach (var field in type.Fields)
            {
                if (!allFields.Add(field.Name))
                {
                    continue;
                }

                var name = field.Name;

                var auto = Regex.Match(field.Name, @"<(?<Field>\w+)>k__BackingField");

                if (auto.Success)
                {
                    name = auto.Groups["Field"].Value;
                }

                if (field.Type.Name.Contains("<")) continue;
                if (field.Type.Name.Contains("+")) continue;
                if (field.Type.IsArray) continue;

                if (field.ElementType >= ClrElementType.Boolean && field.ElementType < ClrElementType.Pointer && !field.Type.IsEnum && !field.Type.IsString)
                {
                    fields.AppendLine($"public global::{field.Type.Name} {name} => Generated.DumpLocator.GetHeap().GetObject(_address).ReadField<global::{field.Type.Name}>(\"{ name}\");");
                }
                else
                {
                    fields.AppendLine($"public Generated.{field.Type.Name} {name};");
                }

                yield return field.Type;
            }

            var typeName = type.Name.Split('.').Last();

            string @namespace;

            if (typeName.Length == type.Name.Length)
            {
                // No namespace
                @namespace = string.Empty;
            }
            else
            {
                @namespace = "." + type.Name.Remove(type.Name.Length - typeName.Length - 1, typeName.Length + 1);
            }

            var result = sourceBuilder.ToString()
                .Replace("{namespace}", @namespace)
                .Replace("{typeName}", typeName)
                .Replace("{fields}", fields.ToString());

            var fileName = $"Generated.{type.Name}-{Guid.NewGuid()}.g.cs";

            context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));

            //File.AppendAllText(Path.Combine(tempFolder, "generated.cs"), result);

            // File.WriteAllText(Path.Combine(tempFolder, fileName), result);
        }

        private static IEnumerable<ClrType> GetAllTypes(ClrRuntime runtime)
        {
            foreach (var module in runtime.EnumerateModules())
            {
                foreach (var (_, token) in module.EnumerateTypeDefToMethodTableMap())
                {
                    var type = module.ResolveToken(token);

                    if (type != null)
                    {
                        yield return type;
                    }
                }
            }
        }

        private class TypeEqualityComparer : IEqualityComparer<ClrType>
        {
            public bool Equals(ClrType x, ClrType y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(ClrType obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }

    //public class ClassWithULongField
    //{
    //    private static ClrHeap heap;

    //    private ulong _address;

    //    public ClassWithULongField(ulong address)
    //    {
    //        _address = address;
    //    }

    //    public System.UInt64 Value => heap.GetObject(_address).ReadField<UInt64>("Value");

    //}

    public class AttributeSyntaxReceiver : ISyntaxReceiver
    {
        public List<(string dump, IReadOnlyList<string> namespaces)> Attributes { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                var attributes = classDeclaration.AttributeLists.SelectMany(a => a.Attributes)
                    .Where(a => a.Name.ToString().Contains("MemoryDump"))
                    .ToList();

                foreach (var attribute in attributes)
                {
                    var literal = attribute.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax;

                    if (literal == null) continue;

                    var namespaces = new List<string>();

                    foreach (var argument in attribute.ArgumentList.Arguments.Skip(1))
                    {
                        var expression = argument.Expression as LiteralExpressionSyntax;

                        if (expression != null)
                        {
                            namespaces.Add(expression.Token.ValueText);
                        }
                    }

                    Attributes.Add((literal.Token.ValueText, namespaces));
                }
            }
        }
    }
}
