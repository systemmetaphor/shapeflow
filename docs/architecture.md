# ShapeFlow Architecture Overview

ShapeFlow is a model-driven code generation engine built around a pipeline of shape transformations. Its core components work together to load input models, apply projection rules, and emit output artifacts such as source files.

## Entry Point and Dependency Registration

The `shapeflow` command-line tool starts in `Program.Main`, which delegates to `Application.Run` to configure the container and dispatch commands【F:src/engine/shapeflow/Program.cs†L10-L14】.  `Application` registers core services, loaders, rule engines, and commands in the dependency injection container before executing the requested command (defaulting to `generate`)【F:src/engine/ShapeFlow.Application/Application.cs†L24-L74】.

## Solution Declaration

Code generation is driven by a *solution declaration* – a JSON configuration file describing projections, shapes, and pipelines. `SolutionDeclaration.Parse` reads this configuration and builds in-memory representations of the projections, input shapes, and transformation pipelines it defines【F:src/engine/ShapeFlow.Core/Declaration/SolutionDeclaration.cs†L84-L175】.

### Shapes and Loaders

Each shape describes an input model and references a loader capable of materialising it. Loaders are registered via the DI container and surfaced through the `LoaderRegistry`. The `ShapeManager` uses this registry to validate and load shapes on demand, caching the resulting `ShapeContext` instances for reuse.

### Supported Shapes

ShapeFlow ships with loaders for JSON (`JsonLoader`), XML (`XmlLoader`), YAML (`YamlModelLoader`), database schemas (`DbModelLoader`), Excel templates (`ExcelLoader`), and file sets (`FileSetLoader`)【F:src/engine/ShapeFlow.Core/Loaders/JsonLoader.cs†L20-L22】【F:src/engine/ShapeFlow.Core/Loaders/XmlLoader.cs†L20-L23】【F:src/engine/ShapeFlow.Loaders.Yaml/YamlModelLoader.cs†L23-L28】【F:src/engine/ShapeFlow.Loaders.DbModel/DbModelLoader.cs†L55-L58】【F:src/engine/ShapeFlow.Loaders.Excel/ExcelLoader.cs†L21-L24】【F:src/engine/ShapeFlow.Core/Loaders/FileSetLoader.cs†L8-L12】. The available shape formats are defined by `ShapeFormat`, which includes `Json`, `Xml`, `Yaml`, `Clr`, `FileSet`, and `Text`【F:src/engine/ShapeFlow.Core/Shapes/ShapeFormat.cs†L6-L41】.

### Output Artifacts

Projections typically produce `FileSet` outputs. When a rule returns text, the engine wraps it into a `FileShape` and infers the file extension before adding it to the set【F:src/engine/ShapeFlow.Core/Projections/ProjectionEngine.cs†L128-L145】. Known output languages include CSharp, SQL, HTML, CSS, and plain Text, as listed in `OutputLanguages`【F:src/engine/ShapeFlow.Core/Output/OutputLanguages.cs†L3-L9】.

## Pipelines

A pipeline orchestrates the sequence of projections to execute. `PipelineDeclaration` captures the pipeline name and its ordered stages【F:src/engine/ShapeFlow.Core/Declaration/PipelineDeclaration.cs†L22-L72】. Each stage specifies a selector identifying the input shape and a projection to apply【F:src/engine/ShapeFlow.Core/Declaration/PipelineStageDeclaration.cs†L8-L32】. When the engine runs, `ShapeFlowEngine.AssemblePipeline` converts these declarations into executable pipelines made of projection handlers【F:src/engine/ShapeFlow.Core/ShapeFlowEngine.cs†L47-L64】.

## Projections and Rule Engines

A projection defines how an input shape is transformed. `ProjectionRegistry` resolves projection metadata, including rules sourced from inline definitions or NuGet packages. During execution `ProjectionEngine` locates the appropriate projection, prepares the output shape, and processes each rule through the appropriate rule engine【F:src/engine/ShapeFlow.Core/Projections/ProjectionEngine.cs†L47-L156】.

Rule engines implement `IProjectionRuleEngine` and support different template languages. For example, the `DotLiquidProjectionRuleEngine` interprets `.liquid` templates and renders them with the input model and pipeline parameters【F:src/engine/ShapeFlow.RuleEngines.DotLiquid/DotLiquidProjectionRuleEngine.cs†L27-L86】. Additional engines such as the T4 engine can be plugged in through the extensibility mechanism.

## Execution Flow

`ShapeFlowEngine.Run` loads the solution declaration, asks the `ProjectionRegistry` to resolve projection packages, assembles the pipeline, and finally executes it. Any errors during generation are logged via `AppTrace`【F:src/engine/ShapeFlow.Core/ShapeFlowEngine.cs†L26-L45】.

## Extensibility

ShapeFlow relies on a lightweight IoC container (`DefaultContainer`) and an `IExtensibilityService` to discover loaders, rule engines, and command extensions at runtime. Registering new implementations of these interfaces extends the engine without modifying core code.

## Summary

In summary, ShapeFlow loads models as *shapes*, passes them through projection stages defined in a solution configuration, and uses pluggable rule engines to produce output artifacts. Understanding these components and their interactions is key to implementing new features or extending the system with custom loaders and rule engines.

