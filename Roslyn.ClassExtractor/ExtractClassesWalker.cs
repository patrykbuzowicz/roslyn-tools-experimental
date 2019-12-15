using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Roslyn
{
    public class ExtractClassesWalker : CSharpSyntaxWalker
    {
        public List<(MemberDeclarationSyntax Type, string Name)> Types { get; } = new List<(MemberDeclarationSyntax Type, string Name)>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            Types.Add((Type: node, Name: node.Identifier.ToString()));
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            Types.Add((Type: node, Name: node.Identifier.ToString()));
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            Types.Add((Type: node, Name: node.Identifier.ToString()));
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            Types.Add((Type: node, Name: node.Identifier.ToString()));
        }
    }
}
