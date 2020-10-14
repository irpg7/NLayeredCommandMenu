using EnvDTE;
using Microsoft.VisualStudio.Shell;
using NLayeredContextMenu.Constants;
using NLayeredContextMenu.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayeredContextMenu.Services
{
    public static class BusinessFileService
    {
        #region Abstract

        public static void CreateBusinessAbstract(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate,
                                                                        $"I{fileParameters.FileNameWithoutExtension}Service.cs");

                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateBusinessAbstractFileContent(fileParameters.FileNameWithoutExtension, fileParameters.ProjectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }
        private static string CreateBusinessAbstractFileContent(string fileName, string projectName)
        {
            return FileContents.BusinessAbstractContent
                    .Replace("[projectName]", projectName)
                    .Replace("[fileName]", fileName);

        }
      
        #endregion

        #region Concrete
        public static void CreateBusinessConcrete(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate, $"{fileParameters.FileNameWithoutExtension}Manager.cs");
                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateBusinessConcreteFileContent(fileParameters.FileNameWithoutExtension, fileParameters.ProjectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }

        private static string CreateBusinessConcreteFileContent(string fileName, string projectName)
        {
            string camelCasedFileName = char.ToLowerInvariant(fileName[0]) + fileName.Substring(1);
            return FileContents.BusinessConcreteContent
                  .Replace("[projectName]", projectName)
                  .Replace("[fileName]", fileName)
                  .Replace("[camelCasedFileName]", camelCasedFileName);
        }
        
        #endregion


        #region DependencyResolvers
        public static void RegisterAddedFilesToIoc(ProjectItem iocFolder,string entityName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (iocFolder.Name.ToLowerInvariant() == "autofac")
            {
                foreach (ProjectItem module in iocFolder.ProjectItems)
                {
                    module.Open();
                    var codeDocument = module.Document;
                    var textDocument = codeDocument.Object() as TextDocument;
                    var lines = textDocument.CreateEditPoint().GetLines(textDocument.StartPoint.Line, textDocument.EndPoint.Line + 1);
                    var valueToSearch = "(ContainerBuilder builder)\r\n        {\r\n";
                    lines = lines.Insert(lines.IndexOf(valueToSearch) + valueToSearch.Length, $"builder.RegisterType<{entityName}Manager>().As<I{entityName}Service>().SingleInstance();\r\n");
                    lines = lines.Insert(lines.IndexOf(valueToSearch) + valueToSearch.Length, $"builder.RegisterType<Ef{entityName}Dal>().As<I{entityName}Dal>().SingleInstance();\r\n");
                    var editedDocument = textDocument.CreateEditPoint();
                    editedDocument.Delete(textDocument.EndPoint);
                    editedDocument.Insert(lines);
                    editedDocument.SmartFormat(textDocument.StartPoint);
                    codeDocument.Save();
                }
            }
            else if (iocFolder.Name.ToLowerInvariant() == "microsoft")
            {
                foreach (ProjectItem module in iocFolder.ProjectItems)
                {
                    module.Open();
                    var codeDocument = module.Document;
                    var textDocument = codeDocument.Object() as TextDocument;
                    var lines = textDocument.CreateEditPoint().GetLines(textDocument.StartPoint.Line, textDocument.EndPoint.Line + 1);
                    var valueToSearch = "(IServiceCollection services)\r\n        {\r\n";
                    lines = lines.Insert(lines.IndexOf(valueToSearch) + valueToSearch.Length, $"services.AddSingleton<I{entityName}Service,{entityName}Manager>();\r\n");
                    lines = lines.Insert(lines.IndexOf(valueToSearch) + valueToSearch.Length, $"services.AddSingleton<I{entityName}Dal,Ef{entityName}Dal>();\r\n");
                    var editedDocument = textDocument.CreateEditPoint();
                    editedDocument.Delete(textDocument.EndPoint);
                    editedDocument.Insert(lines);
                    editedDocument.SmartFormat(textDocument.StartPoint);
                    codeDocument.Save();
                }
            }
        }
        #endregion
    }
}
