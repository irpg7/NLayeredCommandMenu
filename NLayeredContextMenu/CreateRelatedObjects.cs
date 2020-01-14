using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using System.Linq;
using EnvDTE80;
using System.IO;
using System.Text;
using EnvDTE;

namespace NLayeredContextMenu
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateRelatedObjects
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d1103015-8d0f-4e7a-be44-465e8893d23e");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRelatedObjects"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CreateRelatedObjects(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CreateRelatedObjects Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }
        private IServiceProvider SyncServiceProvider
        {
            get
            {
                return this.package;
            }
        }
        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CreateRelatedObjects's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CreateRelatedObjects(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "CreateRelatedObjects";
            EnvDTE.DTE dte;
            EnvDTE.SelectedItems selectedItems;
            EnvDTE.ProjectItem projectItem;
            EnvDTE80.Solution2 solution2;
            dte = SyncServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE ?? throw new ArgumentNullException();
            solution2 = (Solution2)dte.Solution;
            selectedItems = dte.SelectedItems;
            if (selectedItems.Count > 1)
            {
                message = $"Can't Support multiple creation yet.";
                VsShellUtilities.ShowMessageBox((IServiceProvider)ServiceProvider, message, title, OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            if (selectedItems != null)
            {
                foreach (EnvDTE.SelectedItem selectedItem in selectedItems)
                {
                    projectItem = selectedItem.ProjectItem as EnvDTE.ProjectItem;

                    if (projectItem != null)
                    {
                        foreach (EnvDTE.Project project in dte.Solution.Projects)
                        {

                            if (project.Name.EndsWith("DataAccess") || project.Name.EndsWith("Dal"))
                            {
                                if (!DoesProjectFolderExists(project.FullName, "Abstract"))
                                    project.ProjectItems.AddFolder("Abstract");

                                if (!DoesProjectFolderExists(project.FullName, "Concrete\\EntityFramework"))
                                    project.ProjectItems.AddFolder("Concrete\\EntityFramework");

                                foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                                {
                                    var projectTemplate = solution2.GetProjectItemTemplate("Interface", "CSharp");
                                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItem.Name);
                                    if (item.Name == "Abstract")
                                    {
                                        CreateDalAbstract(item, projectTemplate, fileNameWithoutExtension, project.Name);
                                    }
                                    if (item.Name == "Concrete")
                                    {
                                        foreach (ProjectItem concrete in item.ProjectItems)
                                        {
                                            if (concrete.Name == "EntityFramework")
                                            {
                                                CreateDalConcrete(concrete, projectTemplate, fileNameWithoutExtension, project.Name);
                                            }
                                        }
                                    }
                                }

                            }

                            if (project.Name.EndsWith("Business") || project.Name.EndsWith("Bll"))
                            {
                                if (!DoesProjectFolderExists(project.FullName, "Abstract"))
                                    project.ProjectItems.AddFolder("Abstract");

                                if (!DoesProjectFolderExists(project.FullName, "Concrete"))
                                    project.ProjectItems.AddFolder("Concrete");

                                foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                                {
                                    var projectTemplate = solution2.GetProjectItemTemplate("Interface", "CSharp");
                                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItem.Name);
                                    if (item.Name == "Abstract")
                                    {
                                        CreateBusinessAbstract(item, projectTemplate, fileNameWithoutExtension, project.Name);
                                    }
                                    if (item.Name == "Concrete")
                                    {
                                        CreateBusinessConcrete(item, projectTemplate, fileNameWithoutExtension, project.Name);
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        private void CreateDalAbstract(ProjectItem item, string projectTemplate, string fileNameWithoutExtension, string projectName)
        {
            try
            {
                var addedItem = item.ProjectItems.AddFromTemplate(projectTemplate,
                                                                        $"I{fileNameWithoutExtension}Dal.cs");

                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateDalAbstractFileContent(fileNameWithoutExtension, projectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }
        private void CreateDalConcrete(ProjectItem item, string projectTemplate, string fileNameWithoutExtension, string projectName)
        {
            try
            {
                var addedItem = item.ProjectItems.AddFromTemplate(projectTemplate, $"Ef{fileNameWithoutExtension}Dal.cs");
                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateDalConcreteFileContent(fileNameWithoutExtension, projectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }

        }

        private void CreateBusinessAbstract(ProjectItem item, string projectTemplate, string fileNameWithoutExtension, string projectName)
        {
            try
            {
                var addedItem = item.ProjectItems.AddFromTemplate(projectTemplate,
                                                                        $"I{fileNameWithoutExtension}Service.cs");

                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateBusinessAbstractFileContent(fileNameWithoutExtension, projectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }
        private void CreateBusinessConcrete(ProjectItem item, string projectTemplate, string fileNameWithoutExtension, string projectName)
        {
            try
            {
                var addedItem = item.ProjectItems.AddFromTemplate(projectTemplate, $"{fileNameWithoutExtension}Manager.cs");
                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateBusinessConcreteFileContent(fileNameWithoutExtension, projectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }

        private bool DoesProjectFolderExists(string projectFullName, string folderNameToCheck)
        {
            var projectPath = Path.GetDirectoryName(projectFullName);

            return Directory.Exists(Path.Combine(projectPath, folderNameToCheck));
        }
        private string CreateDalAbstractFileContent(string fileName, string projectName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("\n");
            stringBuilder.AppendLine($"namespace {projectName}.Abstract");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"public interface I{fileName}Dal:IEntityRepository<{fileName}>");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
        private string CreateDalConcreteFileContent(string fileName, string projectName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine($"using {projectName}.Abstract;");
            stringBuilder.AppendLine("\n");
            stringBuilder.AppendLine($"namespace {projectName}.Concrete.EntityFramework");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"public class Ef{fileName}Dal:EfEntityRepositoryBase<{fileName},context>,I{fileName}Dal");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        private string CreateBusinessAbstractFileContent(string fileName, string projectName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Linq;");
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
        private string CreateBusinessConcreteFileContent(string fileName, string projectName)
        {
            string camelCasedFileName = char.ToLowerInvariant(fileName[0]) + fileName.Substring(1);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Linq;");
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

    }
}
