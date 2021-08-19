using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumStringSourceGenerator
{
    public static class Template
    {
        public static string GenerateEnumToString(EnumDeclarationSyntax enumDeclaration)
        {
            string @namespace = enumDeclaration
                .Parent.DescendantNodes(x => x.IsKind(SyntaxKind.NamespaceDeclaration))
                .First().GetText().ToString().Trim('\n', '\r');
            string @enum = enumDeclaration.Identifier.Text;

            return $@"
using System;
using {@namespace};

namespace {@namespace}
{{
    public static class {@enum}_ToStringExtensions
    {{
        public static string ToStringFromSgen(this {@enum} self)
        {{
            {GetSwitchForEnum(enumDeclaration)}
        }}
    }}
}}";
        }

        private static string GetSwitchForEnum(EnumDeclarationSyntax enumDeclaration)
        {
            string enumName = enumDeclaration.Identifier.Text;

            var builder = new StringBuilder();
            builder.Append("switch (self)\n{");
            foreach (var value in enumDeclaration.Members)
            {
                string fullName = $"{enumName}.{value.Identifier.Text}";
                builder.AppendFormat($"case {fullName}: return nameof({fullName}) + \"SGEN\";\n");
            }

            builder.Append("default: throw new ArgumentOutOfRangeException(); }");
            return builder.ToString();
        }
    }

    [Generator]
    public class EnumStringSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            //Debugger.Launch();
            
            foreach (EnumDeclarationSyntax @enum in GetEnums(context.Compilation))
            {
                context.AddSource("EnumToString_" + @enum.Identifier.ValueText, Template.GenerateEnumToString(@enum));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        //private class MySyntaxReceiver : ISyntaxReceiver
        //{
        //    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        //    {
        //        Debugger.Launch();
        //        if (syntaxNode is InvocationExpressionSyntax invocation
        //            && invocation.Expression is MemberAccessExpressionSyntax access 
        //            && access.Name.Identifier.ValueText == "ToString")
        //        {
                    
        //        }
        //    }
        //}

        private static IEnumerable<EnumDeclarationSyntax> GetEnums(Compilation compilation)
        {
            IEnumerable<SyntaxNode> allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            IEnumerable<EnumDeclarationSyntax> allEnums = allNodes
                .Where(d => d.IsKind(SyntaxKind.EnumDeclaration))
                .OfType<EnumDeclarationSyntax>();

            return allEnums;
        }
    }
}
