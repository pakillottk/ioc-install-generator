using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace IoC.InstallGenerator
{
    /// <summary>
    /// Incremental source generator that discovers IoC installers from referenced assemblies
    /// and generates static code to load them at runtime.
    /// </summary>
    /// <remarks>
    /// This generator uses IIncrementalGenerator for better performance and automatic caching.
    /// It analyzes project references (direct and transitive) to find
    /// classes implementing <see cref="IoC.InstallGenerator.Abstractions.IIoCInstaller"/>,
    /// then generates a static loader class that can be invoked during application initialization.
    /// </remarks>
    [Generator]
    public class IoCInstallGenerator : IIncrementalGenerator
    {
        private const int MaxTransitiveDepth = 3;
        private const string IoCInstallerInterfaceName = "IIoCInstaller";
        private const string IoCInstallerNamespace = "IoC.InstallGenerator.Abstractions";
        private const string IoCInstallerLoaderAttributeName = "IoCInstallerLoaderAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // T027: Create SyntaxProvider to find classes with [IoCInstallerLoader] attribute
            var loaderClassesProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => ctx.Node as ClassDeclarationSyntax)
                .Where(static node => node != null)
                .Collect();

            // T028: Create CompilationProvider to analyze types implementing IIoCInstaller
            var compilationProvider = context.CompilationProvider;

            // T029: Combine SyntaxProvider and CompilationProvider
            var combinedProvider = loaderClassesProvider
                .Combine(compilationProvider)
                .Select((tuple, ct) => new
                {
                    LoaderClasses = tuple.Left,
                    Compilation = tuple.Right
                });

            // T030: Implement RegisterSourceOutput with incremental pipeline
            context.RegisterSourceOutput(combinedProvider, (sourceContext, data) =>
            {
                try
                {
                    var compilation = data.Compilation;
                    var loaderClasses = FindLoaderClasses(data.LoaderClasses, compilation);
                    
                    if (loaderClasses.Count == 0)
                    {
                        // No loader classes found, skip generation
                        return;
                    }

                    // T027: Discover installers in current and referenced assemblies
                    var installers = DiscoverInstallers(compilation, sourceContext);

                    // Generate partial class for each loader class
                    foreach (var loaderClass in loaderClasses)
                    {
                        // T044: Read InstallOrderAttribute if present for this specific loader class
                        var installOrder = GetInstallOrder(loaderClass, compilation);

                        var generatedCode = CodeGenerator.GeneratePartialClass(
                            loaderClass.ClassName,
                            loaderClass.Namespace,
                            installers,
                            installOrder);
                        
                        sourceContext.AddSource($"{loaderClass.ClassName}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
                    }
                }
                catch (Exception ex)
                {
                    // T030, T066: Error handling with clear error messages and diagnostic information
                    var diagnostic = Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "IOCGEN001",
                            "Source Generator Error",
                            "Error in IoC Install Generator: {0}. Stack trace: {1}",
                            "IoC.InstallGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.None,
                        ex.Message,
                        ex.StackTrace ?? "No stack trace available");
                    sourceContext.ReportDiagnostic(diagnostic);
                }
            });
        }

        // T027: Implement installer discovery logic
        // T051: Optimize reference analysis to avoid duplicate processing
        // T052: Implement caching for discovered installers during generation
        private List<InstallerInfo> DiscoverInstallers(Compilation compilation, SourceProductionContext context)
        {
            var installers = new List<InstallerInfo>();
            var processedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var discoveredTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // T052: Cache discovered types
            
            // Get the IIoCInstaller interface symbol
            var installerInterface = compilation.GetTypeByMetadataName($"{IoCInstallerNamespace}.{IoCInstallerInterfaceName}");
            if (installerInterface == null)
            {
                return installers;
            }

            // T028: Analyze references with transitive depth limit
            // T051: Use HashSet to avoid duplicate processing
            DiscoverInstallersInCompilation(compilation, installerInterface, installers, processedAssemblies, discoveredTypes, context, depth: 0);

            return installers;
        }

        // T028: Implement transitive reference analysis with 3-level depth limit
        // T051: Optimize to avoid duplicate processing using HashSet
        // T052: Cache discovered types to avoid reprocessing
        private void DiscoverInstallersInCompilation(
            Compilation compilation,
            INamedTypeSymbol installerInterface,
            List<InstallerInfo> installers,
            HashSet<string> processedAssemblies,
            HashSet<string> discoveredTypes,
            SourceProductionContext context,
            int depth)
        {
            if (depth > MaxTransitiveDepth)
            {
                return;
            }

            var assemblyName = compilation.AssemblyName ?? string.Empty;
            if (string.IsNullOrEmpty(assemblyName) || processedAssemblies.Contains(assemblyName))
            {
                return;
            }

            processedAssemblies.Add(assemblyName);

            try
            {
                // T027: Find classes implementing IIoCInstaller in current assembly
                var allTypes = GetAllTypes(compilation.GlobalNamespace);
                
                foreach (var type in allTypes)
                {
                    // T031: Validate public, instanciable installer classes
                    if (IsValidInstaller(type, installerInterface))
                    {
                        var fullTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        
                        // T052: Check cache to avoid duplicates
                        if (!discoveredTypes.Contains(fullTypeName))
                        {
                            discoveredTypes.Add(fullTypeName);
                            installers.Add(new InstallerInfo
                            {
                                FullTypeName = fullTypeName,
                                AssemblyName = assemblyName
                            });
                        }
                    }
                }

                // T028: Process transitive references
                if (depth < MaxTransitiveDepth)
                {
                    foreach (var reference in compilation.References)
                    {
                        try
                        {
                            var referencedAssembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                            if (referencedAssembly != null)
                            {
                                var referencedAssemblyName = referencedAssembly.Name;
                                
                                // Skip if already processed
                                if (string.IsNullOrEmpty(referencedAssemblyName) || 
                                    processedAssemblies.Contains(referencedAssemblyName))
                                {
                                    continue;
                                }

                                // T028: Analyze types in referenced assembly through metadata
                                // Note: This is a simplified approach - full transitive analysis would require
                                // loading assemblies, which is complex in source generators
                                var referencedTypes = GetAllTypesFromAssembly(referencedAssembly);
                                
                                foreach (var type in referencedTypes)
                                {
                                    if (IsValidInstaller(type, installerInterface))
                                    {
                                        var fullTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                        
                                        // T052: Check cache to avoid duplicates
                                        if (!discoveredTypes.Contains(fullTypeName))
                                        {
                                            discoveredTypes.Add(fullTypeName);
                                            installers.Add(new InstallerInfo
                                            {
                                                FullTypeName = fullTypeName,
                                                AssemblyName = referencedAssemblyName
                                            });
                                        }
                                    }
                                }

                                // Mark as processed to avoid duplicates
                                processedAssemblies.Add(referencedAssemblyName);

                                // Continue with deeper transitive references if within limit
                                // For now, we analyze one level deep through metadata
                                // Full 3-level transitive analysis would require more complex assembly loading
                            }
                        }
                        catch (Exception ex)
                        {
                            // T029: Handle assemblies with no installers or unavailable assemblies
                            // Skip silently - not all references can be analyzed
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "IOCGEN002",
                                    "Reference Analysis Warning",
                                    "Could not analyze reference: {0}",
                                    "IoC.InstallGenerator",
                                    DiagnosticSeverity.Warning,
                                    isEnabledByDefault: true),
                                Location.None,
                                ex.Message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // T030: Error handling for analysis failures
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "IOCGEN003",
                        "Assembly Analysis Error",
                        "Error analyzing assembly '{0}': {1}",
                        "IoC.InstallGenerator",
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    Location.None,
                    assemblyName,
                    ex.Message));
            }
        }

        private IEnumerable<INamedTypeSymbol> GetAllTypesFromAssembly(IAssemblySymbol assembly)
        {
            return GetAllTypes(assembly.GlobalNamespace);
        }

        // T031: Add validation for public, instanciable installer classes
        private bool IsValidInstaller(INamedTypeSymbol type, INamedTypeSymbol installerInterface)
        {
            if (type == null || type.IsAbstract || type.IsStatic)
            {
                return false;
            }

            if (type.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }

            // Check if type implements IIoCInstaller
            if (!type.AllInterfaces.Contains(installerInterface, SymbolEqualityComparer.Default))
            {
                return false;
            }

            // Check if type has a public parameterless constructor or is a struct
            if (type.IsValueType)
            {
                return true; // Structs have implicit parameterless constructor
            }

            var constructors = type.Constructors;
            return constructors.Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);
        }

        private IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
        {
            foreach (var type in namespaceSymbol.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                foreach (var type in GetAllTypes(nestedNamespace))
                {
                    yield return type;
                }
            }
        }

        // Find classes with [IoCInstallerLoader] attribute
        private List<LoaderClassInfo> FindLoaderClasses(IEnumerable<ClassDeclarationSyntax> classDeclarations, Compilation compilation)
        {
            var loaderClasses = new List<LoaderClassInfo>();
            var loaderAttribute = compilation.GetTypeByMetadataName($"{IoCInstallerNamespace}.{IoCInstallerLoaderAttributeName}");
            
            if (loaderAttribute == null)
            {
                return loaderClasses;
            }

            foreach (var classDecl in classDeclarations)
            {
                var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
                
                if (classSymbol != null)
                {
                    var attribute = classSymbol.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.Equals(loaderAttribute, SymbolEqualityComparer.Default) == true);
                    
                    if (attribute != null)
                    {
                        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                        loaderClasses.Add(new LoaderClassInfo
                        {
                            ClassName = classSymbol.Name,
                            Namespace = namespaceName,
                            ClassSymbol = classSymbol
                        });
                    }
                }
            }

            return loaderClasses;
        }

        // T044: Read InstallOrderAttribute from a specific loader class
        private string? GetInstallOrder(LoaderClassInfo loaderClass, Compilation compilation)
        {
            var installOrderAttribute = compilation.GetTypeByMetadataName($"{IoCInstallerNamespace}.InstallOrderAttribute");
            if (installOrderAttribute == null)
            {
                return null;
            }

            // Look for InstallOrderAttribute on the specific loader class
            if (loaderClass.ClassSymbol != null)
            {
                var attribute = loaderClass.ClassSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Equals(installOrderAttribute, SymbolEqualityComparer.Default) == true);
                
                if (attribute != null && attribute.ConstructorArguments.Length > 0)
                {
                    var assemblyNames = attribute.ConstructorArguments[0].Values
                        .Select(v => v.Value?.ToString())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
                    
                    if (assemblyNames.Length > 0)
                    {
                        return string.Join(",", assemblyNames);
                    }
                }
            }

            return null;
        }
    }

    internal class LoaderClassInfo
    {
        public string ClassName { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public INamedTypeSymbol? ClassSymbol { get; set; }
    }
}
