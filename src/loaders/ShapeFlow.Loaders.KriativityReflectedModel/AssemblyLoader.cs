using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using ShapeFlow.Infrastructure;

namespace Kriativity.ModelDriven.Infrastructure
{
    internal class AssemblyLoader : IDisposable
    {
        private readonly Dictionary<string, AssemblyDef> _loadedAssemblies;

        public AssemblyLoader()
        {
            _loadedAssemblies = new Dictionary<string, AssemblyDef>();
            InstallAssemblyResolver();
        }

        public Func<string, bool> IgnoreFilter { get; set; }

        public IEnumerable<AssemblyDef> LoadAll(string baseDirectory, string pattern)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(baseDirectory);
            FileInfo[] allAssembliesInDirectory = dirInfo.GetFiles();
            
            // Get all the assemblies  in the directory that end with .dll
            foreach (var assemblyFileInfo in allAssembliesInDirectory)
            {
                if(!assemblyFileInfo.Name.MatchesGlobExpression(pattern))
                {
                    continue;
                }

                string assemblyName = assemblyFileInfo.Name.ToLower();

                if (IsToIgnore(assemblyName))
                {
                    AppTrace.Verbose("Assembly to ignore: {0}", assemblyFileInfo.FullName);
                    continue;
                }

                // load assembly
                LoadAssembly(assemblyFileInfo);
            }

            return _loadedAssemblies.Values;
        }

        public IEnumerable<AssemblyDef> GetAll(string pattern)
        {
            var kvps = from entry in _loadedAssemblies
                       where entry.Key.MatchesGlobExpression(pattern)
                       select entry;

            return kvps.Select(kvp => kvp.Value).ToArray();
        }

        public Assembly GetOrLoad(string pattern)
        {
            bool FullNameStartsWith(Assembly a) => a.FullName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
            var result = AppDomain.CurrentDomain.GetAssemblies().Where(FullNameStartsWith).FirstOrDefault();
            return result;
        }

        public void Dispose()
        {
            RemoveAssemblyResolver();
        }

        private void InstallAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        private void RemoveAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnResolveAssembly;
        }

        private Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly ass = null;

            if (args.RequestingAssembly != null)
            {
                ass = this.ResolveAssembly(args.Name.Substring(0, args.Name.IndexOf(", Version=")) + ".dll", Path.GetDirectoryName(args.RequestingAssembly.Location));
            }

            return ass;
        }

        private Assembly ResolveAssembly(AssemblyName assemblyName, string path)
        {
            var foundAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == assemblyName.FullName).ToList();

            if (foundAssembly.Any())
            {
                return foundAssembly.FirstOrDefault();
            }
            else
            {
                return this.ResolveAssembly(assemblyName.Name + ".dll", path);
            }
        }

        private Assembly ResolveAssembly(string assemblyName, string path)
        {
            FileInfo fi = new FileInfo(path + Path.DirectorySeparatorChar + assemblyName);
            AppTrace.Verbose("Assembly {0} not found, trying to load it now.", fi.FullName);
            Assembly ass = Assembly.LoadFile(fi.FullName);
            return ass;
        }

        private void LoadAssembly(FileInfo assembly)
        {
            AppTrace.Verbose("Trying to load assembly: {0}", assembly.FullName);

            if (!_loadedAssemblies.ContainsKey(assembly.FullName))
            {
                AppTrace.Verbose("Loading assembly {0} ", assembly.FullName);


                ModuleDefMD loadedAssembly = null;
                try
                {
                    loadedAssembly = ModuleDefMD.Load(assembly.FullName);
                    _loadedAssemblies.Add(loadedAssembly.FullName, loadedAssembly.Assembly);
                }
                catch
                {
                    AppTrace.Verbose("Error while trying to get types from {0}", loadedAssembly?.FullName);
                }

            }
            else
            {
                AppTrace.Verbose("{0} already loaded, nothing to do", assembly.FullName);
            }
        }

        private bool IsToIgnore(string assemblyName)
        {
            if (IgnoreFilter != null)
            {
                return IgnoreFilter(assemblyName);
            }

            return false;
        }
    }
}
