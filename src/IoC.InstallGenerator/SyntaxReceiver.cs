using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IoC.InstallGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Classes { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Collect class declarations for analysis
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                Classes.Add(classDeclaration);
            }
        }
    }
}

