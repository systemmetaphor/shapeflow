using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TextTemplating;

namespace ShapeFlow.RuleEngines.T4
{
    internal class TemplateArgumentDirectiveProcessor : DirectiveProcessor
    {
        private const string ArgumentNameType = "type";
        private const string ArgumentNameName = "name";
        private const string ArgumentNameConverter = "converter";
        private const string MethodGetArgument = nameof(CallContext.GetData);

        private static readonly CodeGeneratorOptions Options = new CodeGeneratorOptions
        {
            BlankLinesBetweenMembers = true,
            IndentString = "        ",
            VerbatimOrder = true,
            BracingStyle = "C"
        };

        private readonly Dictionary<string, ArgumentInfo> _argumentInfos = new Dictionary<string, ArgumentInfo>();

        private CodeDomProvider _provider;
        
        public CodeDomProvider CodeProvider => _provider;

        public override void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
        {
            base.StartProcessingRun(languageProvider, templateContents, errors);
            _provider = languageProvider;
        }

        public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
        {
            var info = new ArgumentInfo
            {
                Name = GetArgument(arguments, ArgumentNameName),
                Type = GetTypeArgument(arguments, ArgumentNameType),
                ConverterType = GetTypeArgument(arguments, ArgumentNameConverter)
            };

            if (string.IsNullOrEmpty(info.Name))
            {
                throw new InvalidOperationException("Property directive type name null or empty.");
            }

            if (_argumentInfos.ContainsKey(info.Name))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Object directive '{0}' already exists.", directiveName));
            }

            if (string.IsNullOrEmpty(info.Type))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Property directive '{0}' type is null or empty.",info.Name));
            }
            
            _argumentInfos.Add(info.Name, info);
        }
        
        public override string GetClassCodeForProcessingRun()
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                foreach (ArgumentInfo argument in _argumentInfos.Values)
                {
                    //Create field
                    var field = new CodeMemberField(argument.Type, argument.FieldName)
                    {
                        Attributes = MemberAttributes.Private
                    };

                    _provider.GenerateCodeFromMember(field, writer, Options);
                    
                    var property = new CodeMemberProperty
                    {
                        Name = argument.Name,
                        Type = new CodeTypeReference(argument.Type),
                        Attributes = MemberAttributes.Public,
                        HasGet = true,
                        HasSet = false
                    };
                    property.GetStatements.Add(
                        new CodeMethodReturnStatement(new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            argument.FieldName)));

                    if (!string.IsNullOrEmpty(argument.ConverterType))
                    {
                        property.CustomAttributes.Add(new CodeAttributeDeclaration(
                            new CodeTypeReference(typeof(TypeConverter)),
                            new CodeAttributeArgument(
                                new CodeTypeOfExpression(argument.ConverterType))));
                    }

                    _provider.GenerateCodeFromMember(property, writer, Options);
                }

                return writer.ToString();
            }
        }
       
        public override string GetPostInitializationCodeForProcessingRun()
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                foreach (ArgumentInfo argument in _argumentInfos.Values)
                {
                    //Generate initialization code for each argument
                    var assignment = new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), argument.FieldName),
                        new CodeCastExpression(
                            new CodeTypeReference(argument.Type),
                            new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression(typeof(CallContext)),
                                MethodGetArgument,
                                new CodePrimitiveExpression(argument.Name))));

                    _provider.GenerateCodeFromStatement(assignment, writer, Options);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Finishes the processing run.
        /// </summary>
        public override void FinishProcessingRun()
        {
        }

        /// <summary>
        /// Gets the imports for processing run.
        /// </summary>
        /// <returns></returns>
        public override string[] GetImportsForProcessingRun()
        {
            return null;
        }

        /// <summary>
        /// Gets the pre initialization code for processing run.
        /// </summary>
        /// <returns></returns>
        public override string GetPreInitializationCodeForProcessingRun()
        {
            return null;
        }

        /// <summary>
        /// Gets the references for processing run.
        /// </summary>
        /// <returns></returns>
        public override string[] GetReferencesForProcessingRun()
        {
            return null;
        }

        /// <summary>
        /// Determines whether the directive with the specific name is supported.
        /// </summary>
        /// <param name="directiveName">Name of the directive.</param>
        /// <returns>
        /// 	<c>true</c> always returns true.
        /// </returns>
        public override bool IsDirectiveSupported(string directiveName)
        {
            return true;
        }

        /// <summary>
        /// Get the argument with the specified name.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static string GetArgument(IDictionary<string, string> arguments, string name)
        {
            return !arguments.ContainsKey(name) ? null : arguments[name];
        }

        /// <summary>
        /// Gets a type argument and normalize it.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static string GetTypeArgument(IDictionary<string, string> arguments, string name)
        {
            var argument = GetArgument(arguments, name);
            return NormalizeType(argument);
        }

        /// <summary>
        /// Normalizes the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        internal static string NormalizeType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }
            var charArray = type.ToCharArray();
            var genericTypes = 0;
            var commaIndex = -1;
            for (int i = 0; i < charArray.Length; i++)
            {
                var c = charArray[i];
                if (c == '<')
                {
                    genericTypes++;
                }
                else if (c == '>')
                {
                    genericTypes--;
                }
                else if (c == ',' && genericTypes == 0)
                {
                    commaIndex = i;
                    break;
                }
            }
            if (genericTypes != 0)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "Invalid generic type specified: '{0}'", type);
                throw new InvalidOperationException(message);
            }
            if (commaIndex >= 0)
            {
                return type.Substring(0, commaIndex).Trim();
            }
            return type;
        }
    }
}