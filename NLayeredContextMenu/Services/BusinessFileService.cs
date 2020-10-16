using EnvDTE;
using Humanizer;
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
        private static void FileGenerator(string physicalFileName,CreateFileParameters parameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var fileContent = GenerateFileContent(parameters);
                parameters.FileContent = fileContent;
                var addedItem = parameters.ProjectItem.ProjectItems.AddFromTemplate(parameters.ProjectTemplate,
                                                                        $"{physicalFileName}.cs");

                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(parameters.FileContent);
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }
        private static string GenerateFileContent(CreateFileParameters parameters)
        {
            return parameters.FileContent.Replace("[projectName]", parameters.ProjectName)
                .Replace("[fileName]", parameters.FileNameWithoutExtension)
                .Replace("[camelCasedFileName]", 
                parameters.FileNameWithoutExtension[0].ToString().ToLowerInvariant() + parameters.FileNameWithoutExtension.Substring(1))
                .Replace("[pluralizedFileName]", parameters.FileNameWithoutExtension.Pluralize());
        }
        #region Commands
        public static void GenerateCreateCommand(CreateFileParameters parameters)
        {
            var fileNameToGenerate = $"Create{parameters.FileNameWithoutExtension}Command";
            parameters.FileContent = FileContents.BusinessCreateCommand;
            FileGenerator(fileNameToGenerate, parameters);
        }
        public static void GenerateUpdateCommand(CreateFileParameters parameters)
        {
            var fileNameToGenerate = $"Update{parameters.FileNameWithoutExtension}Command";
            parameters.FileContent = FileContents.BusinessUpdateCommand;
            FileGenerator(fileNameToGenerate, parameters);
        }
        public static void GenerateDeleteCommand(CreateFileParameters parameters)
        {
            var fileNameToGenerate = $"Delete{parameters.FileNameWithoutExtension}Command";
            parameters.FileContent = FileContents.BusinessDeleteCommand;
            FileGenerator(fileNameToGenerate, parameters);
        }


        #endregion

        #region Queries
        public static void GenerateGetQuery(CreateFileParameters parameters)
        {
            var fileNameToGenerate = $"Get{parameters.FileNameWithoutExtension}Query";
            parameters.FileContent = FileContents.BusinessGetQuery;
            FileGenerator(fileNameToGenerate, parameters);
        }
        public static void GenerateGetListQuery(CreateFileParameters parameters)
        {
            var fileNameToGenerate = $"Get{parameters.FileNameWithoutExtension.Pluralize()}Query";
            parameters.FileContent = FileContents.BusinessGetListQuery;
            FileGenerator(fileNameToGenerate, parameters);
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
