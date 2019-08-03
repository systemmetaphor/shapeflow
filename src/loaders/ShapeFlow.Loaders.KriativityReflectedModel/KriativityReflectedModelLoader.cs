using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        private static BindingFlags HierarchyProperties = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
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
            var root = new KriativityReflectedModelRoot();

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
            
            var typesToConsider = new Dictionary<string, Type>();

            // TODO: Include the types that were explicitly added in the configuration

            var assemblyInclusionPattern = context.GetParameter("assemblySearchPattern") ?? DefaultAssembliesSearchPattern;
            var assemblies = _assemblyLoader.GetAll(assemblyInclusionPattern);

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(GeneratedCodeAttribute), false).Any())
                    {
                        continue;
                    }

                    if (!typesToConsider.ContainsKey(type.FullName))
                    {
                        typesToConsider.Add(type.FullName, type);
                    }
                }
            }

            var dataObjectTypes = new List<Type>();
            var eventObjectTypes = new List<Type>();
            var stateObjectTypes = new List<Type>();

            dataObjectTypes.AddRange(typesToConsider.Values
                .Where(t => t.ShouldIncludeAsDataObject())
                .Where(t => string.IsNullOrWhiteSpace(ns) || t.Namespace.MatchesGlobExpression(ns)));

            eventObjectTypes.AddRange(typesToConsider.Values
                .Where(t => t.ShouldIncludeAsEventObject())
                .Where(t => string.IsNullOrWhiteSpace(ns) || t.Namespace.MatchesGlobExpression(ns)));

            stateObjectTypes.AddRange(typesToConsider.Values
                .Where(t => t.ShouldIncludeAsStateObject())
                .Where(t => string.IsNullOrWhiteSpace(ns) || t.Namespace.MatchesGlobExpression(ns)));

            var imported = new List<Type>();

            var bos = dataObjectTypes.Select(t => ConvertToReflectObject(t, imported)).ToList();
            if (imported.Count > 0)
            {
                // we are ignoring these imports
                var innerImported = new List<Type>();

                var tmpCollection = imported.Select(t => ConvertToReflectObject(t, innerImported)).ToList();
                tmpCollection.AddRange(bos);
                bos = tmpCollection;
                imported.Clear();
            }

            var evos = eventObjectTypes.Select(t => ConvertToReflectObject(t, imported)).ToList();
            if (imported.Count > 0)
            {
                // we are ignoring these imports
                var innerImported = new List<Type>();

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

            root.AddModels(bos, evos, sos);

            var shape = new KriativityReflectedModelShape(root, ShapeFormat.Clr, context.Name);

            return Task.FromResult(new ShapeContext(context, shape));
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

        private ReflectedObject ConvertToReflectObject(Type t, List<Type> importedTypes)
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
                DotNetNamespace = t.Namespace,
                DotNetAssembly = t.Assembly.FullName,
                DotNetType = t,
                BaseName = t.BaseType != null && t.BaseType != typeof(object) ? t.BaseType.Name : string.Empty,
                HasBaseEventObject = t.BaseType != null && t.BaseType.ShouldIncludeAsEventObject()
            };

            // handle type

            var attributeGroups = t.GetCustomAttributes(false).GroupBy(a => a.GetType());
            var customAttributes = attributeGroups.ToDictionary(g => g.Key);
            var hasObsoleteAttribute = customAttributes.Any(a => a.Key == typeof(ObsoleteAttribute));

            if (hasObsoleteAttribute)
            {
                result.IsObsolete = true;
            }

            ProcessTypeProperties(t, result, importedTypes);

            return result;
        }

        private static void ProcessTypeProperties(Type t, ReflectedObject b, List<Type> importedTypes)
        {
            // handle type properties
            var properties = t.GetProperties(HierarchyProperties).OrderBy(p => p.Name).ToList();

            var thisPrefix = b.DotNetType.Assembly.FullName.Split('.').FirstOrDefault() ?? string.Empty;

            foreach (var property in properties)
            {
                var businessObjectProperty = new ReflectedObjectProperty();
                businessObjectProperty.Name = property.Name;

                var customAttributes = property.GetCustomAttributes(false).ToDictionary(a => a.GetType());
                var hasObsoleteAttribute = customAttributes.ContainsKey(typeof(ObsoleteAttribute));

                if (hasObsoleteAttribute)
                {
                    businessObjectProperty.IsObsolete = true;
                }


                businessObjectProperty.DotNetType = property.PropertyType;
                businessObjectProperty.TargetName = businessObjectProperty.Name.ToCamelCase();
                businessObjectProperty.IsBusinessObject = businessObjectProperty.DotNetType.ShouldIncludeAsDataObject();

                var theName = businessObjectProperty.DotNetType.Name;

                if (theName.EndsWith("Data"))
                {
                    theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                }

                businessObjectProperty.BusinessObjectType = businessObjectProperty.IsBusinessObject ? theName : string.Empty;
                businessObjectProperty.DataTransferObjectType = businessObjectProperty.DotNetType.Name;

                var collectionElementType = TypeScriptSyntax.GetCollectionElementType(businessObjectProperty.DotNetType);

                businessObjectProperty.IsBusinessObjectCollection = collectionElementType != null && collectionElementType.ShouldIncludeAsDataObject();
                if (businessObjectProperty.IsBusinessObjectCollection)
                {
                    theName = collectionElementType.Name;

                    if (theName.EndsWith("Data"))
                    {
                        theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                    }

                    businessObjectProperty.BusinessObjectType = theName;
                }

                if (businessObjectProperty.IsBusinessObject || businessObjectProperty.IsBusinessObjectCollection)
                {
                    var referencedType = businessObjectProperty.IsBusinessObject ? businessObjectProperty.DotNetType : collectionElementType;

                    if (referencedType != null)
                    {
                        var referencedPrefix = referencedType.Assembly.FullName.Split('.').FirstOrDefault() ?? string.Empty;

                        if (!string.Equals(thisPrefix, referencedPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!importedTypes.Any(element => element.FullName.Equals(referencedType.FullName)))
                            {
                                importedTypes.Add(referencedType);
                            }
                        }
                    }
                }

                if (businessObjectProperty.IsBusinessObject)
                {
                    theName = businessObjectProperty.DotNetType.Name;
                    if (theName.EndsWith("Data"))
                    {
                        theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                    }

                    businessObjectProperty.TargetType = $"KnockoutObservable<{theName}>";
                    businessObjectProperty.DataTransferObjectType = businessObjectProperty.DotNetType.Name;
                }
                else
                {
                    businessObjectProperty.TargetType = TypeScriptSyntax.ConvertClrTypeToDomainTypeScriptType(businessObjectProperty.DotNetType);
                    businessObjectProperty.DataTransferObjectType = TypeScriptSyntax.ConvertClrTypeToDtoTypeScriptType(businessObjectProperty.DotNetType);
                }

                if (businessObjectProperty.IsBusinessObject)
                {
                    theName = businessObjectProperty.DotNetType.Name;
                    if (theName.EndsWith("Data"))
                    {
                        theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                    }

                    businessObjectProperty.TargetInitialValue = $"ko.observable(new {theName}())";
                    businessObjectProperty.DataTransferObjectTargetInitialValue = $"new {theName}()";
                }
                else
                {
                    businessObjectProperty.TargetInitialValue = TypeScriptSyntax.GetDomainDefaultValue(businessObjectProperty.DotNetType);
                    businessObjectProperty.DataTransferObjectTargetInitialValue = TypeScriptSyntax.GetDtoDefaultValue(businessObjectProperty.DotNetType);
                }

                b.AddProperty(businessObjectProperty);
            }
        }
    }
}
