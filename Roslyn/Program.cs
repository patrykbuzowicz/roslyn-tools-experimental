using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Roslyn
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var workspace = new AdhocWorkspace();
            var solution = workspace.CurrentSolution;
            var project = solution.AddProject("project", "assembly", LanguageNames.CSharp);
            var document = project.AddDocument("file.cs", @"
namespace Main.Proj 
{
    class A {}

    [Test]
    class B {
        public int Number {get;set;}
    }
}
");

            var syntaxTree = await document.GetSyntaxTreeAsync();
            var compUnit = syntaxTree.GetCompilationUnitRoot();

            var visitor = new ExtractClassesWalker();
            compUnit.Accept(visitor);

            var docs = visitor.Classes
                .Where(ParentIsNamespace)
                .Select(WrapWithParentNamespace)
                .Select(x => x.ToFullString())
                .ToList();

            docs.ForEach(x => Console.WriteLine(x));
        }

        private static bool ParentIsNamespace(ClassDeclarationSyntax arg) => arg.Parent is NamespaceDeclarationSyntax;

        private static NamespaceDeclarationSyntax WrapWithParentNamespace(ClassDeclarationSyntax classDeclaration)
        {
            var ns = classDeclaration.Parent as NamespaceDeclarationSyntax;
            return ns
                .RemoveNodes(ns.Members, SyntaxRemoveOptions.KeepDirectives)
                .AddMembers(classDeclaration);
        }
    }
}
