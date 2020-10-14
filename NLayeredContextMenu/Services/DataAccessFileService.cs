using EnvDTE;
using Microsoft.VisualStudio.Shell;
using NLayeredContextMenu.Constants;
using NLayeredContextMenu.Models;
using System.Linq;

namespace NLayeredContextMenu.Services
{
    public static class DataAccessFileService
    {
        #region Abstract
        public static void CreateDalAbstract(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate,
                                                                        $"I{fileParameters.FileNameWithoutExtension}Dal.cs");

                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateDalAbstractFileContent(fileParameters.FileNameWithoutExtension, fileParameters.ProjectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }

        private static string CreateDalAbstractFileContent(string fileName, string projectName)
        {
            return FileContents.DataAccessAbstract
                  .Replace("[projectName]", projectName)
                  .Replace("[fileName]", fileName);
        }
        
        #endregion

        #region Concrete
        public static void CreateDalConcrete(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate, $"Ef{fileParameters.FileNameWithoutExtension}Dal.cs");
                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateDalConcreteFileContent(fileParameters.FileNameWithoutExtension, fileParameters.ProjectName, GetDbContext(fileParameters.ProjectItem)));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
                AddDbSetToContext(fileParameters.ProjectItem);
            }
            catch
            {
                throw;
            }

        }
        private static string CreateDalConcreteFileContent(string fileName, string projectName, string dbContextName)
        {
            return FileContents.DataAccessConcrete
                  .Replace("[projectName]", projectName)
                  .Replace("[fileName]", fileName)
                  .Replace("[DbContextName]", dbContextName);
        }
       
        private static string GetDbContext(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (ProjectItem item in projectItem.ProjectItems)
            {
                if (item.Name == "Contexts")
                {
                    if (item.ProjectItems.Count > 1)
                    {
                        return "MultiContextNotSupported";
                    }
                    return item.ProjectItems.Cast<ProjectItem>().FirstOrDefault().Name.Replace(".cs", "");
                }
            }
            return "";
        }
        private static void AddDbSetToContext(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var contextProjectItem = projectItem.ProjectItems.Cast<ProjectItem>()
                .FirstOrDefault(x => x.Name == "Contexts").ProjectItems.Cast<ProjectItem>()
                .FirstOrDefault();
            foreach (CodeElement codeElement in contextProjectItem.FileCodeModel.CodeElements)
            {
                if (codeElement is CodeNamespace)
                {
                    var nspace = codeElement as CodeNamespace;
                    foreach (CodeClass classMember in nspace.Members)
                    {
                        if (classMember is null)
                            continue;

                        var selectedItemType = projectItem.Name.Replace(".cs", "");
                        var pluralizedName = selectedItemType;//.Pluralize();
                        var codeDocument = classMember.ProjectItem.Open().Document;
                        var textCodeDocument = codeDocument.Object() as TextDocument;
                        var edited = textCodeDocument.CreateEditPoint(classMember.GetEndPoint(vsCMPart.vsCMPartBody));
                        edited.Insert($"public DbSet<{selectedItemType}> {pluralizedName}" + "{ get; set; }");
                        edited.SmartFormat(textCodeDocument.StartPoint);
                        codeDocument.Save();
                    }
                }
            }
        }
        #endregion
    }
}
