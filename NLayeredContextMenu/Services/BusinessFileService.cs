using EnvDTE;
using Microsoft.VisualStudio.Shell;
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
            return _fmtClassFile
                    .Replace("[projectName]", projectName)
                    .Replace("[fileName]", fileName);

        }
        private const string _fmtClassFile = @"
            using Entities.Concrete;
            using System.Collections.Generic;
            using Core.Utilities.Results;
            
            
            namespace [projectName].Abstract
            {
                public interface I[fileName]Service
                {
                    IDataResult<[fileName]> Get(int id);
                    IDataResult<List<[fileName]>> GetList();
                    IResult Add([fileName] entity);
                    IResult Update([fileName] entity);
                    IResult Delete(int id);
                }
            }
";
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
            return _fmtConcreteFile
                  .Replace("[projectName]", projectName)
                  .Replace("[fileName]", fileName)
                  .Replace("[camelCasedFileName]", camelCasedFileName);
        }
        private const string _fmtConcreteFile = @"
           using System;
           using System.Linq;
           using System.Collections.Generic;
           using Entities.Concrete;
           using DataAccess.Abstract;
           using [projectName].Abstract;
           using Core.Utilities.Results;
           using Business.Constants;
          
            namespace [projectName].Concrete
           {
            public class [fileName]Manager:I[fileName]Service
           {
            private I[fileName]Dal _[camelCasedFileName]Dal;
            public [fileName]Manager(I[fileName]Dal [camelCasedFileName]Dal)
           {
            _[camelCasedFileName]Dal=[camelCasedFileName]Dal;
           }

            public IDataResult<[fileName]> Get(int id)
           {
            return new SuccessDataResult<[fileName]>(_[camelCasedFileName]Dal.Get(x=>x.Id == id));
           }

            public IDataResult<List<[fileName]>> GetList()
           {
            return new SuccessDataResult<List<[fileName]>>(_[camelCasedFileName]Dal.GetList());
           }

            public IResult Add([fileName] entity)
           {
              _[camelCasedFileName]Dal.Add(entity);
              return new SuccessResult(Messages.[fileName]Added);
           }

            public IResult Update([fileName] entity)
           {
              _[camelCasedFileName]Dal.Update(entity);
              return new SuccessResult(Messages.[fileName]Updated);
           }

            public IResult Delete(int id)
           {
             var entity = Get(id).Data;
             _[camelCasedFileName]Dal.Delete(entity);
             return new SuccessResult(Messages.[fileName]Deleted);
           }

           }

           }";
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
