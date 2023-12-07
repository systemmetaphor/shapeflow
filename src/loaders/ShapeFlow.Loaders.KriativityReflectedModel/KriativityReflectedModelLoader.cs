using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using dnlib.DotNet;
using Kriativity.ModelDriven.Infrastructure;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public class KriativityReflectedModelLoader : ILoader
    {
        private const string DefaultAssembliesInclusionPattern = "**\\*.dll";
        private const string DefaultAssembliesSearchPattern = "**";
        private const string LoaderName = "kriativityReflectedModel";
        private const string ModelPathParameter = "model-path";
        private const string AssemblySearchPatternParameter = "assembly-search-pattern";

        private readonly AssemblyLoader _assemblyLoader;

        public KriativityReflectedModelLoader()
        {
            Name = LoaderName;
            Format = ShapeFormat.Clr;
            _assemblyLoader = new AssemblyLoader();
        }

        public string Name { get; }

        public ShapeFormat Format { get; }

        public Task<ShapeContext> Load(ShapeDeclaration context)
        {
            //context.GeneratorContext.SearchDirectories;

            var searchDirectories = new List<string>();
            var assemblyLoadPatterns = new List<string>();

            searchDirectories.Add(context.GetParameter(ModelPathParameter));
            assemblyLoadPatterns.Add(context.GetParameter(AssemblySearchPatternParameter));

            foreach (var binariesDirectory in searchDirectories)
            {
                var actualDirectory = binariesDirectory.Replace("{{__dirname}}", Environment.CurrentDirectory);

                if (!Directory.Exists(actualDirectory))
                {
                    throw new DirectoryNotFoundException($"The directory '{binariesDirectory}' does not exist.");
                }

                foreach (var loadPattern in assemblyLoadPatterns)
                {
                    _assemblyLoader.LoadAll(actualDirectory, loadPattern ?? DefaultAssembliesInclusionPattern);
                }
            }

            var ns = context.GetParameter("namespace-inclusion-pattern");

            var typesToConsider = new Dictionary<string, TypeDef>();

            // TODO: Include the types that were explicitly added in the configuration

            var assemblyInclusionPattern = context.GetParameter("assemblySearchPattern") ?? DefaultAssembliesSearchPattern;
            var assemblies = _assemblyLoader.GetAll(assemblyInclusionPattern);

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.Modules.SelectMany(m => m.GetTypes()))
                {
                    if (type.CustomAttributes.Any(t => t.TypeFullName.Contains(typeof(GeneratedCodeAttribute).FullName)))
                    {
                        continue;
                    }

                    if (!typesToConsider.ContainsKey(type.FullName))
                    {
                        typesToConsider.Add(type.FullName, type);
                    }
                }
            }

            var root = ReflectModel(ns, typesToConsider);

            var shape = new KriativityReflectedModelShape(root, ShapeFormat.Clr, context.Name);

            return Task.FromResult(new ShapeContext(context, shape));
        }

        public KriativityReflectedModelRoot ReflectModel(string ns, Dictionary<string, TypeDef> typesToConsider)
        {
            var root = new KriativityReflectedModelRoot();
            var dataObjectTypes = new List<TypeDef>();
            var eventObjectTypes = new List<TypeDef>();
            var stateObjectTypes = new List<TypeDef>();

            dataObjectTypes.AddRange(typesToConsider.Values
                .Where(t => t.ShouldIncludeAsDataObject())
                .Where(t => string.IsNullOrWhiteSpace(ns) || t.Namespace.String.MatchesGlobExpression(ns)));

            eventObjectTypes.AddRange(typesToConsider.Values
                .Where(t => t.ShouldIncludeAsEventObject())
                .Where(t => string.IsNullOrWhiteSpace(ns) || t.Namespace.String.MatchesGlobExpression(ns)));

            stateObjectTypes.AddRange(typesToConsider.Values
                .Where(t => t.ShouldIncludeAsStateObject())
                .Where(t => string.IsNullOrWhiteSpace(ns) || t.Namespace.String.MatchesGlobExpression(ns)));

            var imported = new List<TypeDef>();

            var bos = dataObjectTypes.Select(t => ConvertToReflectObject(t, imported)).ToList();
            if (imported.Count > 0)
            {
                // we are ignoring these imports
                var innerImported = new List<TypeDef>();

                var tmpCollection = imported.Select(t => ConvertToReflectObject(t, innerImported)).ToList();
                tmpCollection.AddRange(bos);
                bos = tmpCollection;
                imported.Clear();
            }

            var evos = eventObjectTypes.Select(t => ConvertToReflectObject(t, imported)).ToList();
            if (imported.Count > 0)
            {
                // we are ignoring these imports
                var innerImported = new List<TypeDef>();

                var tmpCollection = imported.Select(t => ConvertToReflectObject(t, innerImported)).ToList();
                // nasty hack: we are adding this to the bo collection because its a way to get the sort
                // algorithm to place them before the events
                bos.AddRange(tmpCollection);
                imported.Clear();
            }

            var sos = new List<StateProjection>();

            foreach (var bo in bos)
            {
                var so = stateObjectTypes.FirstOrDefault(t => t.Name.Equals(bo.Name.Replace("Model", "State")));
                if (so != null)
                {
                    var projection = new StateProjection { BusinessObject = bo, StateObject = ConvertToReflectObject(so, imported) };

                    var reference = projection.BusinessObject.Properties.Count() > projection.StateObject.Properties.Count() ?
                        projection.BusinessObject : projection.StateObject;

                    var other = reference == projection.BusinessObject ? projection.StateObject : projection.BusinessObject;

                    foreach (var p in reference.Properties)
                    {
                        var otherp = other.Properties.FirstOrDefault(element => element.Name.Equals(p.Name));
                        if (otherp != null)
                        {
                            projection.AddPropertyProjection(new PropertyProjection
                            {
                                BusinessObjectProperty = reference == projection.BusinessObject ? p : otherp,
                                StateProperty = reference == projection.BusinessObject ? otherp : p
                            });
                        }
                    }

                    sos.Add(projection);
                }
            }

            root.AddModels(bos, evos, sos, Enumerable.Empty<ReflectedObject>());

            return root;
        }

        public Task Save(ShapeContext context)
        {
            throw new NotImplementedException();
        }

        public ShapeContext Create(ShapeDeclaration decl)
        {
            throw new NotImplementedException();
        }

        public bool ValidateArguments(ShapeDeclaration context)
        {
            return true;
        }

        private ReflectedObject ConvertToReflectObject(TypeDef t, List<TypeDef> importedTypes)
        {
            var theName = t.Name;

            if (theName.EndsWith("Data"))
            {
                theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
            }

            var result = new ReflectedObject()
            {
                Name = theName,
                DtoName = t.Name,
                Namespace = t.Namespace,
                Type = t.Name.String,
                BaseType = t.BaseType != null && t.BaseType.Name.String != typeof(object).Name ? t.BaseType.Name.String : string.Empty,
                HasBaseEventObject = t.BaseType?.ResolveTypeDef() != null && t.BaseType.ResolveTypeDef().ShouldIncludeAsEventObject()
            };

            // handle type

            var attributeGroups = t.CustomAttributes.GroupBy(a => a.GetType());
            var customAttributes = attributeGroups.ToDictionary(g => g.Key);
            var hasObsoleteAttribute = customAttributes.Any(a => a.Key == typeof(ObsoleteAttribute));

            if (hasObsoleteAttribute)
            {
                result.IsObsolete = true;
            }

            ProcessTypeProperties(t, result, importedTypes);

            return result;
        }

        private static void ProcessTypeProperties(TypeDef t, ReflectedObject b, List<TypeDef> importedTypes)
        {
            // handle type properties
            var properties = t.Properties.OrderBy(p => p.Name).ToList();

            var thisPrefix = b.Type.Split('.').FirstOrDefault() ?? string.Empty;

            foreach (var property in properties)
            {
                var businessObjectProperty = new ReflectedObjectProperty();
                businessObjectProperty.Name = property.Name;

                var customAttributes = property.CustomAttributes.ToDictionary(a => a.GetType());
                var hasObsoleteAttribute = customAttributes.ContainsKey(typeof(ObsoleteAttribute));

                if (hasObsoleteAttribute)
                {
                    businessObjectProperty.IsObsolete = true;
                }

                var propertyTypeDef = property.GetMethod.ReturnType.TryGetTypeDef();

                businessObjectProperty.Type = property.GetMethod.ReturnType.TypeName;
                businessObjectProperty.TargetName = businessObjectProperty.Name.ToCamelCase();
                businessObjectProperty.IsBusinessObject = propertyTypeDef?.ShouldIncludeAsDataObject() ?? false;

                var theName = businessObjectProperty.Type;

                if (theName.EndsWith("Data"))
                {
                    theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                }

                businessObjectProperty.BusinessObjectType = businessObjectProperty.IsBusinessObject ? theName : string.Empty;
                businessObjectProperty.DataTransferObjectType = businessObjectProperty.Type;

                var collectionElementTypeName = string.Empty;

                if (propertyTypeDef != null)
                {
                    var collectionElementType = TypeScriptSyntax.GetCollectionElementType(propertyTypeDef);

                    businessObjectProperty.IsBusinessObjectCollection = collectionElementType != null && collectionElementType.ShouldIncludeAsDataObject();
                    if (businessObjectProperty.IsBusinessObjectCollection)
                    {
                        theName = collectionElementType.Name;

                        if (theName.EndsWith("Data"))
                        {
                            theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                        }

                        collectionElementTypeName = theName;
                        businessObjectProperty.BusinessObjectType = collectionElementTypeName;
                    }
                }

                if (businessObjectProperty.IsBusinessObject || businessObjectProperty.IsBusinessObjectCollection)
                {
                    var referencedType = businessObjectProperty.IsBusinessObject ? businessObjectProperty.Type : collectionElementTypeName;

                    if (referencedType != null)
                    {
                        var referencedPrefix = referencedType.Split('.').FirstOrDefault() ?? string.Empty;

                        if (!string.Equals(thisPrefix, referencedPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!importedTypes.Any(element => element.Equals(referencedType)))
                            {
                                // TODO: This will break when its a collection
                                importedTypes.Add(propertyTypeDef);
                            }
                        }
                    }
                }

                if (businessObjectProperty.IsBusinessObject)
                {
                    theName = businessObjectProperty.Type;
                    if (theName.EndsWith("Data"))
                    {
                        theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                    }

                    businessObjectProperty.TargetType = $"KnockoutObservable<{theName}>";
                    businessObjectProperty.DataTransferObjectType = businessObjectProperty.Type;
                }
                else
                {
                    businessObjectProperty.TargetType = TypeScriptSyntax.ConvertClrTypeToDomainTypeScriptType(propertyTypeDef);
                    businessObjectProperty.DataTransferObjectType = TypeScriptSyntax.ConvertClrTypeToDtoTypeScriptType(propertyTypeDef);
                }

                if (businessObjectProperty.IsBusinessObject)
                {
                    theName = businessObjectProperty.Type;
                    if (theName.EndsWith("Data"))
                    {
                        theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                    }

                    businessObjectProperty.TargetInitialValue = $"ko.observable(new {theName}())";
                    businessObjectProperty.DataTransferObjectTargetInitialValue = $"new {theName}()";
                }
                else
                {
                    businessObjectProperty.TargetInitialValue = TypeScriptSyntax.GetDomainDefaultValue(propertyTypeDef);
                    businessObjectProperty.DataTransferObjectTargetInitialValue = TypeScriptSyntax.GetDtoDefaultValue(propertyTypeDef);
                }

                b.AddProperty(businessObjectProperty);
            }
        }
    }
}
