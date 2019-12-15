using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Roslyn
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var path = args.FirstOrDefault();

            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Missing argument: path");
                return;
            }

            var fileContents = await File.ReadAllTextAsync(path);
            var workspace = new AdhocWorkspace();
            var solution = workspace.CurrentSolution;
            var project = solution.AddProject("project", "assembly", LanguageNames.CSharp);
            var document = project.AddDocument("file.cs", fileContents);

            var syntaxTree = await document.GetSyntaxTreeAsync();
            var compUnit = syntaxTree.GetCompilationUnitRoot();

            var visitor = new ExtractClassesWalker();
            compUnit.Accept(visitor);

            var docs = visitor.Types
                .Where(ParentIsNamespace)
                .Select(WrapWithParentNamespace)
                .Select(x => (contents: x.Type.ToFullString(), name: x.Name))
                .ToList();

            var targetDir = Path.GetDirectoryName(path);
            docs.ForEach(x =>
            {
                var targetPath = Path.Combine(targetDir, $"{x.name}.cs");
                Console.WriteLine($"Writing to {targetPath}");
                File.WriteAllText(targetPath, x.contents);
            });
        }

        private static bool ParentIsNamespace((MemberDeclarationSyntax Type, string Name) arg) => arg.Type.Parent is NamespaceDeclarationSyntax;

        private static (MemberDeclarationSyntax Type, string Name) WrapWithParentNamespace((MemberDeclarationSyntax Type, string Name) pair)
        {
            var ns = pair.Type.Parent as NamespaceDeclarationSyntax;
            var wrapped = ns
                .RemoveNodes(ns.Members, SyntaxRemoveOptions.KeepDirectives)
                .AddMembers(pair.Type);

            return (Type: wrapped, Name: pair.Name);
        }
    }
}
