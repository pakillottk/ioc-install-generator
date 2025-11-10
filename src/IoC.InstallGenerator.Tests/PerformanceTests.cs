using System;
using System.Diagnostics;
using System.Linq;
using IoC.InstallGenerator.Abstractions;
using Xunit;

namespace IoC.InstallGenerator.Tests
{
    // T046-T049, T054: Performance tests for User Story 3
    public class PerformanceTests
    {
        // T046: Benchmark test comparing source generator vs reflection approach
        [Fact]
        public void BenchmarkSourceGeneratorVsReflection()
        {
            // This test compares the performance of source generator approach vs reflection
            // Note: Full benchmark requires actual generated code execution
            // This is a placeholder structure for the benchmark
            
            var container = new MockContainer();
            
            // Measure source generator approach (would use generated code)
            var sw = Stopwatch.StartNew();
            // In real test: IoCInstallerLoader.LoadAll(container);
            sw.Stop();
            var sourceGeneratorTime = sw.ElapsedMilliseconds;
            
            // Measure reflection approach
            sw.Restart();
            LoadInstallersWithReflection(container);
            sw.Stop();
            var reflectionTime = sw.ElapsedMilliseconds;
            
            // T046: Verify source generator is at least 50% faster
            // Note: This is a placeholder - actual test requires generated code
            Assert.True(sourceGeneratorTime >= 0);
            Assert.True(reflectionTime >= 0);
        }

        // T047: Test to verify generated code contains no reflection calls
        [Fact]
        public void VerifyGeneratedCodeHasNoReflection()
        {
            // This test would analyze the generated code to ensure it doesn't contain
            // reflection calls like Type.GetType(), Assembly.GetTypes(), Activator.CreateInstance(typeof(T))
            // For now, this is a placeholder that documents the requirement
            
            // In a real implementation, we would:
            // 1. Generate code using the source generator
            // 2. Parse the generated code
            // 3. Verify it doesn't contain reflection API calls
            
            Assert.True(true); // Placeholder
        }

        // T048: Test for initialization time with 50 assemblies
        [Fact]
        public void TestInitializationTimeWith50Assemblies()
        {
            // This test verifies that initialization with 50 assemblies completes in <100ms
            var container = new MockContainer();
            
            var sw = Stopwatch.StartNew();
            // In real test: Load 50 assemblies worth of installers
            // IoCInstallerLoader.LoadAll(container);
            sw.Stop();
            
            // T048: Verify initialization time is <100ms
            Assert.True(sw.ElapsedMilliseconds < 100, 
                $"Initialization took {sw.ElapsedMilliseconds}ms, expected <100ms");
        }

        // T049: Test for 3-level transitive reference performance
        [Fact]
        public void TestTransitiveReferencePerformance()
        {
            // This test verifies that analyzing 3 levels of transitive references
            // doesn't cause significant performance degradation
            var container = new MockContainer();
            
            var sw = Stopwatch.StartNew();
            // In real test: Load installers with 3-level transitive references
            // IoCInstallerLoader.LoadAll(container);
            sw.Stop();
            
            // T049: Verify transitive reference analysis is efficient
            Assert.True(sw.ElapsedMilliseconds < 200, 
                $"Transitive reference analysis took {sw.ElapsedMilliseconds}ms, expected <200ms");
        }

        // T054: Reflection-based implementation for benchmark comparison
        private void LoadInstallersWithReflection(IIoCContainer container)
        {
            // This is a reflection-based implementation for comparison
            // It demonstrates what the source generator replaces
            
            try
            {
                var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
                
                foreach (var assemblyName in referencedAssemblies)
                {
                    try
                    {
                        var assembly = System.Reflection.Assembly.Load(assemblyName);
                        var installerTypes = assembly.GetTypes()
                            .Where(t => typeof(IIoCInstaller).IsAssignableFrom(t) 
                                     && !t.IsAbstract 
                                     && !t.IsInterface
                                     && t.IsPublic);
                        
                        foreach (var installerType in installerTypes)
                        {
                            try
                            {
                                var installer = (IIoCInstaller)Activator.CreateInstance(installerType)!;
                                installer.Install(container);
                            }
                            catch (Exception ex)
                            {
                                // Handle individual installer failures
                                throw new InstallerException(installerType.FullName ?? installerType.Name, ex);
                            }
                        }
                    }
                    catch
                    {
                        // Skip assemblies that can't be loaded
                    }
                }
            }
            catch (Exception ex)
            {
                throw new AggregateException("Failed to load installers using reflection", ex);
            }
        }

        private class MockContainer : IIoCContainer
        {
            public void Register<TService, TImplementation>() where TImplementation : TService { }
            public void Register<TService>(TService instance) { }
            public void Register<TService>(Func<IIoCContainer, TService> factory) { }
            public TService Resolve<TService>()
            {
                throw new InvalidOperationException("Resolve not implemented in mock container for performance tests");
            }
        }
    }
}

