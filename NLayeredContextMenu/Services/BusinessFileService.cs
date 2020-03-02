using EnvDTE;
using Microsoft.VisualStudio.Shell;
using NLayeredContextMenu.Models;
using System;
using System.Collections.Generic;
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
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using Entities.Concrete;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("\n");
            stringBuilder.AppendLine($"namespace {projectName}.Abstract");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"public interface I{fileName}Service");
            stringBuilder.AppendLine("{");

            stringBuilder.AppendLine($"{fileName} Get(int id);");
            stringBuilder.AppendLine($"List<{fileName}> GetList();");
            stringBuilder.AppendLine($"void Add({fileName} entity);");
            stringBuilder.AppendLine($"void Update({fileName} entity);");
            stringBuilder.AppendLine($"void Delete(int id);");

            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();

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
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Linq;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("using Entities.Concrete;");
            stringBuilder.AppendLine("using DataAccess.Abstract;");
            stringBuilder.AppendLine($"using {projectName}.Abstract;");
            stringBuilder.AppendLine("\n");
            stringBuilder.AppendLine($"namespace {projectName}.Concrete");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"public class {fileName}Manager:I{fileName}Service");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"private I{fileName}Dal _{camelCasedFileName}Dal;");
            stringBuilder.AppendLine($"public {fileName}Manager(I{fileName}Dal {camelCasedFileName}Dal)");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"_{camelCasedFileName}Dal={camelCasedFileName}Dal;");
            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine($"public {fileName} Get(int id)");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"return _{camelCasedFileName}Dal.Get(id);");
            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine($"public List<{fileName}> GetList()");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"return _{camelCasedFileName}Dal.GetList();");
            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine($"public void Add({fileName} entity)");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"_{camelCasedFileName}Dal.Add(entity);");
            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine($"public void Update({fileName} entity)");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"_{camelCasedFileName}Dal.Update(entity);");
            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine($"public void Delete(int id)");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"_{camelCasedFileName}Dal.Delete(id);");
            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine("}");

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
        #endregion
    }
}
