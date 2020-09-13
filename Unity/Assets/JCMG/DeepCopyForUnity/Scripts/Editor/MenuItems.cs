/*

MIT License

Copyright (c) 2020 Jeff Campbell

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using TypeInfo = Microsoft.CodeAnalysis.TypeInfo;

namespace JCMG.DeepCopyForUnity.Editor
{
	/// <summary>
	/// Menu items for this library
	/// </summary>
	internal static class MenuItems
	{
		[MenuItem("Tools/JCMG/DeepCopyForUnity/Submit bug or feature request")]
		internal static void OpenURLToGitHubIssuesSection()
		{
			const string GITHUB_ISSUES_URL = "https://github.com/jeffcampbellmakesgames/DeepCopyForUnity/issues";

			Application.OpenURL(GITHUB_ISSUES_URL);
		}

		[MenuItem("Tools/JCMG/DeepCopyForUnity/Donate to support development")]
		internal static void OpenURLToKoFi()
		{
			const string KOFI_URL = "https://ko-fi.com/stampyturtle";

			Application.OpenURL(KOFI_URL);
		}

		[MenuItem("Tools/JCMG/DeepCopyForUnity/About")]
		internal static void OpenAboutModalDialog()
		{
			AboutWindow.View();
		}

		private const string FILEPATH = "DeepCloneAOTSupport.cs";
		private const string FILE_TEMPLATE = @"
using UnityEngine.Scripting;

[Preserve]
internal sealed class DeepCloneAOTSupport
{{
    private void Foo()
    {{
		// Array Generic Calls
{0}

		// Class Generic Calls
{1}

		// Struct Generic Calls
{2}
    }}
}}
";

		[MenuItem("TEST/Generate AOT Support")]
		internal static void GenerateAOTSupport()
		{
			// Create the workspace
			var workspace = new AdhocWorkspace();

			// Create a CSharp project
			var projName = "NewProject";
			var projectId = ProjectId.CreateNewId();
			var versionStamp = VersionStamp.Create();
			var projectInfo = ProjectInfo.Create(
				projectId,
				versionStamp,
				projName,
				projName,
				LanguageNames.CSharp);
			var newProject = workspace.AddProject(projectInfo);
			var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
			newProject = newProject.WithCompilationOptions(compilationOptions);

			// Add references to all loaded assemblies
			var assemblies = GetAvailableAssemblies();
			foreach (var assembly in assemblies)
			{
				try
				{
					if (assembly.IsDynamic)
					{
						continue;
					}

					var assemblyPath = assembly.Location;
					var metaDataReference = MetadataReference.CreateFromFile(assemblyPath);
					newProject = newProject.AddMetadataReference(metaDataReference);
				}
				catch (Exception e)
				{
					Debug.LogWarningFormat(
						"Could not Load Assembly [{0}]. \n\n {1}\n{2}",
						assembly.FullName,
						e.Message,
						e.StackTrace);
				}
			}

			// Discover and load all CSharp source files
			var csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
			for (var i = 0; i < csFiles.Length; i++)
			{
				var fileName = Path.GetFileName(csFiles[i]);
				var doc = newProject.AddDocument(fileName, File.ReadAllText(csFiles[i]), filePath: csFiles[i]);
				newProject = doc.Project;
			}

			// Compile the CSharp project and log any diagnostic issues
			var projectCompilation = newProject.GetCompilationAsync().Result;

			var excludeWarnings = new List<string>
			{
				"CS1701", // May need to load runtime set Policy
				"CS0121", // Unambiguous type
				"CS0436", // Conflicting type
				"CS0433", // Duplicate type definition
				"CS0227", // Unsafe code
				"CS8019", // Unnecessary using directive
			};

			foreach (var diagnostic in projectCompilation.GetDiagnostics())
			{
				if (excludeWarnings.Contains(diagnostic.Id))
				{
					continue;
				}

				// TODO Add an option to turn on logging these warnings.
				// Debug.LogWarning(diagnostic);
			}

			// Find the member symbols for the DeepClone and ShallowClone extension methods.
			var symbols = projectCompilation
				.GetSymbolsWithName(s => s.EndsWith("DeepClone") ||
										 s.EndsWith("DeepCloneTo") ||
										 s.EndsWith("ShallowClone") ||
										 s.EndsWith("ShallowCloneTo"), SymbolFilter.Member)
				.Where(x => x.ContainingType.Name == "DeepClonerExtensions")
				.ToList();

			// Discover all classes in the loaded CSharp source files and find all instances where the DeepClone and
			// ShallowClone extension methods are called. Get the type information of the objects that make those calls.
			var discoveredTypes = new List<TypeInfo>();
			foreach (var syntaxTree in projectCompilation.SyntaxTrees)
			{
				var root = syntaxTree.GetRoot();
				var allClassDeclarationSyntax = root.DescendantNodes()
					.OfType<ClassDeclarationSyntax>();

				var semanticModel = projectCompilation.GetSemanticModel(syntaxTree);

				foreach (var classDeclarationSyntax in allClassDeclarationSyntax)
				{
					var members = classDeclarationSyntax.Members.AsEnumerable();

					FindDeepCloneInvocations(members, semanticModel, symbols, discoveredTypes);
				}
			}

			var discoveredTypeSymbols = discoveredTypes.Select(x => x.Type).ToList();

			var allMemberTypesOnDiscoveredTypes = discoveredTypeSymbols
				.SelectMany(x => GetAllMemberTypes(x, new List<ITypeSymbol>()))
				.Distinct()
				.ToArray();

				// Separate and filter out each of the types into groups based on the AOT-supporting generic code-snippets
			// that need to be created.
			var arrayTypes = allMemberTypesOnDiscoveredTypes
				.SelectMany(GetArrayTypes)
				.Distinct()
				.OrderBy(x => x.ToString())
				.ToArray();

			var arrayElementTypes = arrayTypes
				.Select(x => x.Item1)
				.SelectMany(GetArrayElementTypes)
				.Distinct()
				.ToArray();

			var genericTypes = allMemberTypesOnDiscoveredTypes
				.SelectMany(GetGenericTypes)
				.Distinct()
				.OrderBy(x => x.ToString())
				.ToArray();

			var genericElementTypes = genericTypes
				.SelectMany(GetGenericElementTypes)
				.Distinct()
				.OrderBy(x => x.ToString())
				.ToArray();

			var interfaceTypes = allMemberTypesOnDiscoveredTypes
				.Concat(arrayElementTypes)
				.Concat(genericElementTypes)
				.Where(x => x.TypeKind == TypeKind.Interface)
				.Distinct()
				.OrderBy(x => x.ToString())
				.ToArray();

			var closedTypes = allMemberTypesOnDiscoveredTypes
				.Where(x => x.TypeKind != TypeKind.Array && !x.IsAnonymousType)
				.Concat(genericTypes)
				.Concat(genericElementTypes)
				.Concat(interfaceTypes)
				.Distinct()
				.OrderBy(x => x.ToString())
				.ToArray();

			var classTypes = closedTypes.Where(x => x.IsReferenceType);
			var structTypes = closedTypes.Where(x => x.IsValueType);

			var anonymousTypes = allMemberTypesOnDiscoveredTypes
				.Where(x => x.IsAnonymousType)
				.Distinct()
				.OrderBy(x => x.ToString())
				.ToArray();

			// Create file contents
			var sb = new StringBuilder();
			var arrayContents = string.Empty;
			var classContents = string.Empty;
			var structContents = string.Empty;

			// Create array contents
			PopulateGenericArrayCodeSnippets(arrayTypes, sb);
			arrayContents = sb.ToString();

			// Create class contents
			PopulateGenericClassCodeSnippets(classTypes, sb);
			classContents = sb.ToString();

			// Create struct contents
			PopulateGenericStructCodeSnippets(structTypes, sb);
			structContents = sb.ToString();

			// Write file contents to disk.
			var fileContents = string.Format(
				FILE_TEMPLATE,
				arrayContents,
				classContents,
				structContents).Replace("\r\n", "\n");
			var filePath = Path.GetFullPath(Path.Combine(Application.dataPath, FILEPATH));

			File.WriteAllText(filePath, fileContents);
			AssetDatabase.ImportAsset("Assets/" + FILEPATH);
		}

		public static List<ITypeSymbol> GetAllMemberTypes(ITypeSymbol typeSymbol, List<ITypeSymbol> foundTypeSymbols)
		{
			// First add this symbol
			foundTypeSymbols.Add(typeSymbol);

			var memberSymbols = typeSymbol
				.GetBaseTypesAndThis()
				.SelectMany(x => x.GetMembers())
				.ToArray();

			var memberFields = memberSymbols.Where(x => x.Kind == SymbolKind.Field).ToArray();
			var memberProperties = memberSymbols.Where(x => x.Kind == SymbolKind.Property).ToArray();
			var memberTypes = memberFields
				.OfType<IFieldSymbol>()
				.Select(x => x.Type)
				.Concat(memberProperties.OfType<IPropertySymbol>().Select(x => x.Type))
				.ToArray();

			// Attempt getting all member types of this symbol
			foreach (var memberType in memberTypes)
			{
				if (foundTypeSymbols.Contains(memberType))
				{
					continue;
				}

				GetAllMemberTypes(memberType, foundTypeSymbols);
			}

			// Check to see if this type is generic and get all inner generic types, element types
			var genericTypes = GetGenericTypes(typeSymbol);
			foreach (var genericType in genericTypes)
			{
				if (foundTypeSymbols.Contains(genericType))
				{
					continue;
				}

				GetAllMemberTypes(genericType, foundTypeSymbols);

				var genericElementTypes = GetGenericElementTypes(genericType);
				foreach (var genericElementType in genericElementTypes)
				{
					if (foundTypeSymbols.Contains(genericElementType))
					{
						continue;
					}

					GetAllMemberTypes(genericElementType, foundTypeSymbols);
				}
			}

			// Check to see if this type is an array and get all inner array types, element types
			var arrayTypes = GetArrayTypes(typeSymbol);
			foreach (var arrayType in arrayTypes)
			{
				if (foundTypeSymbols.Contains(arrayType.Item1))
				{
					continue;
				}

				GetAllMemberTypes(arrayType.Item1, foundTypeSymbols);

				var arrayElementTypes = GetArrayElementTypes(arrayType.Item1);
				foreach (var arrayElementType in arrayElementTypes)
				{
					if (foundTypeSymbols.Contains(arrayElementType))
					{
						continue;
					}

					GetAllMemberTypes(arrayElementType, foundTypeSymbols);
				}
			}

			return foundTypeSymbols;
		}

		public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
		{
			var current = type;
			while (current != null)
			{
				yield return current;
				current = current.BaseType;
			}
		}

		/// <summary>
		/// Returns an IEnumerable of all loaded <see cref="Assembly"/> instances in <see cref="AppDomain"/>.
		/// </summary>
		public static IEnumerable<Assembly> GetAvailableAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		private static List<Tuple<IArrayTypeSymbol, int>> GetArrayTypes(ITypeSymbol typeSymbol)
		{
			var arrayTypes = new List<Tuple<IArrayTypeSymbol, int>>();
			if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
			{
				arrayTypes.Add(new Tuple<IArrayTypeSymbol, int>(arrayTypeSymbol, arrayTypeSymbol.Rank));
				arrayTypes.AddRange(GetArrayTypes(arrayTypeSymbol.ElementType));
			}

			return arrayTypes;
		}

		private static List<ITypeSymbol> GetArrayElementTypes(ITypeSymbol typeSymbol)
		{
			// Mapping of Element Type to Array Rank
			var elementTypes = new List<ITypeSymbol>();
			var typeSymbolToInspect = typeSymbol;
			while (typeSymbolToInspect.TypeKind == TypeKind.Array)
			{
				var arrayTypeSymbol = (IArrayTypeSymbol)typeSymbolToInspect;
				typeSymbolToInspect = arrayTypeSymbol.ElementType;

				elementTypes.Add(arrayTypeSymbol.ElementType);
			}

			return elementTypes;
		}

		private static List<ITypeSymbol> GetGenericTypes(ITypeSymbol typeSymbol)
		{
			var elementTypes = new List<ITypeSymbol>();
			var typeSymbolToInspect = typeSymbol;
			if (typeSymbolToInspect is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
			{
				elementTypes.Add(namedTypeSymbol);

				var typeArguments = namedTypeSymbol.TypeArguments;
				foreach (var typeArgument in typeArguments)
				{
					elementTypes.AddRange(GetGenericTypes(typeArgument));
				}
			}

			return elementTypes;
		}

		private static List<ITypeSymbol> GetGenericElementTypes(ITypeSymbol typeSymbol)
		{
			var elementTypes = new List<ITypeSymbol>();
			var typeSymbolToInspect = typeSymbol;
			if (typeSymbolToInspect is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
			{
				var typeArguments = namedTypeSymbol.TypeArguments;
				foreach (var typeArgument in typeArguments)
				{
					elementTypes.Add(typeArgument);
					elementTypes.AddRange(GetGenericElementTypes(typeArgument));
				}
			}

			return elementTypes;
		}

		private static void FindDeepCloneInvocations(IEnumerable<SyntaxNode> nodes,
		                                             SemanticModel semanticModel,
		                                             List<ISymbol> matchingSymbols,
		                                             List<TypeInfo> discoveredTypes)
		{
			if (!nodes.Any())
			{
				return;
			}

			foreach (var syntaxNode in nodes)
			{
				if(syntaxNode is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
				{
					if (matchingSymbols.Any(x => x.Name == memberAccessExpressionSyntax.Name.ToString()))
					{
						var subjectType = semanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression);
						if (!discoveredTypes.Contains(subjectType))
						{
							discoveredTypes.Add(subjectType);
						}
					}

					FindDeepCloneInvocations(
						memberAccessExpressionSyntax.DescendantNodes(),
						semanticModel,
						matchingSymbols,
						discoveredTypes);
				}
				else if (syntaxNode is MemberDeclarationSyntax memberDeclarationSyntax)
				{
					FindDeepCloneInvocations(
						memberDeclarationSyntax.DescendantNodes(),
						semanticModel,
						matchingSymbols,
						discoveredTypes);
				}
			}
		}

		private static void PopulateGenericArrayCodeSnippets(Tuple<IArrayTypeSymbol, int>[] arrayTypeMappings, StringBuilder sb)
		{
			sb.Clear();

			// Create easy collection/association of required type info
			var complexArrayTypeList = new List<Tuple<IArrayTypeSymbol, string, ITypeSymbol, string>>();
			foreach (var arrayTypeMapping in arrayTypeMappings)
			{
				// Get full type name
				var arrayTypeSymbol = arrayTypeMapping.Item1;
				var fullArrayTypeName = arrayTypeSymbol.ToString();
				var arrayElementType = arrayTypeSymbol.ElementType;
				var fullArrayElementTypeName = arrayElementType.ToString();

				// Skip unsafe types
				if (fullArrayTypeName.Contains("*"))
				{
					continue;
				}

				complexArrayTypeList.Add(new Tuple<IArrayTypeSymbol, string, ITypeSymbol, string>(
					arrayTypeSymbol,
					fullArrayTypeName,
					arrayElementType,
					fullArrayElementTypeName));
			}

			// Add all shallow copies
			var rank1ArrayTypes = complexArrayTypeList.Where(x => x.Item1.Rank == 1);
			foreach (var rank1ArrayType in rank1ArrayTypes)
			{
				sb.AppendLine($"		JCMG.DeepCopyForUnity.ClonerToExprGenerator.ShallowClone1DimArraySafeInternal<{rank1ArrayType.Item4}>(null, null);");
				sb.AppendLine($"		JCMG.DeepCopyForUnity.ClonerToExprGenerator.Clone1DimArraySafeInternal<{rank1ArrayType.Item4}>(null, null, null);");
			}

			// Create all 1D class deep copies
			var rank1ArrayOfClasses = complexArrayTypeList.Where(x => x.Item1.Rank == 1 && x.Item3.IsReferenceType);
			foreach (var rank1ArrayOfClass in rank1ArrayOfClasses)
			{
				sb.AppendLine($"		JCMG.DeepCopyForUnity.ClonerToExprGenerator.Clone1DimArrayClassInternal<{rank1ArrayOfClass.Item4}>(null, null, null);");
				sb.AppendLine($"		JCMG.DeepCopyForUnity.ClonerToExprGenerator.Clone1DimArraySafeInternal<{rank1ArrayOfClass.Item4}>(null, null, null);");
				sb.AppendLine($"		JCMG.DeepCopyForUnity.DeepClonerGenerator.Clone1DimArraySafeInternal<{rank1ArrayOfClass.Item4}>(null, null);");
				sb.AppendLine($"		JCMG.DeepCopyForUnity.DeepClonerGenerator.Clone1DimArrayClassInternal<{rank1ArrayOfClass.Item4}>(null, null);");
			}

			// Create all 1D struct deep copies
			var rank1ArrayOfValueTypes = complexArrayTypeList.Where(x => x.Item1.Rank == 1 && x.Item3.IsValueType);
			foreach (var rank1ArrayOfValueType in rank1ArrayOfValueTypes)
			{
				sb.AppendLine($"		JCMG.DeepCopyForUnity.ClonerToExprGenerator.Clone1DimArraySafeInternal<{rank1ArrayOfValueType.Item4}>(null, null, null);");
				sb.AppendLine($"		JCMG.DeepCopyForUnity.ClonerToExprGenerator.Clone1DimArrayStructInternal<{rank1ArrayOfValueType.Item4}>(null, null, null);");
				sb.AppendLine($"		JCMG.DeepCopyForUnity.DeepClonerGenerator.Clone1DimArraySafeInternal<{rank1ArrayOfValueType.Item4}>(null, null);");
				sb.AppendLine($"		JCMG.DeepCopyForUnity.DeepClonerGenerator.Clone1DimArrayStructInternal<{rank1ArrayOfValueType.Item4}>(null, null);");
			}

			// Create all 2D deep copies
			var rank2ArrayTypes= complexArrayTypeList.Where(x => x.Item1.Rank == 2);
			foreach (var rank2ArrayType in rank2ArrayTypes)
			{
				sb.AppendLine($"		JCMG.DeepCopyForUnity.ClonerToExprGenerator.Clone2DimArrayInternal<{rank2ArrayType.Item4}>(null, null, null, true);");
				sb.AppendLine($"		JCMG.DeepCopyForUnity.DeepClonerGenerator.Clone2DimArrayInternal<{rank2ArrayType.Item4}>(null, null);");
			}
		}

		private static void PopulateGenericClassCodeSnippets(IEnumerable<ITypeSymbol> classTypes, StringBuilder sb)
		{
			sb.Clear();
			foreach (var classType in classTypes)
			{
				// Get type info
				var fullTypeName = classType.ToString();

				// Skip unsafe types
				if (fullTypeName.Contains("*") || classType.Kind == SymbolKind.ErrorType)
				{
					continue;
				}

				sb.AppendLine($"		JCMG.DeepCopyForUnity.DeepClonerGenerator.GetClonerForValueType<{fullTypeName}>();");
			}
		}

		private static void PopulateGenericStructCodeSnippets(IEnumerable<ITypeSymbol> structTypes, StringBuilder sb)
		{
			sb.Clear();
			foreach (var structType in structTypes)
			{
				// Get type info
				var fullTypeName = structType.ToString();

				// Skip unsafe types
				if (fullTypeName.Contains("*"))
				{
					continue;
				}

				sb.AppendLine($"		JCMG.DeepCopyForUnity.DeepClonerGenerator.CloneStructInternal<{fullTypeName}>(default, null);");
				//sb.AppendLine($"		JCMG.DeepCopyForUnity.DeepClonerGenerator.GetClonerForValueType<{fullTypeName}>();");

				//sb.AppendLine($"		{{ System.Func<{fullTypeName},JCMG.DeepCopyForUnity.DeepCloneState,{fullTypeName}> func; }}");
			}
		}
	}
}
