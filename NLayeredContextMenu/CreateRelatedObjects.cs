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
using NLayeredContextMenu.Models;
using NLayeredContextMenu.Services;
using NLayeredContextMenu.Constants;
using NLayeredContextMenu.Helpers;
using Humanizer;

namespace NLayeredContextMenu
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateRelatedObjects
    {

        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("d1103015-8d0f-4e7a-be44-465e8893d23e");
        private readonly AsyncPackage package;

        private CreateRelatedObjects(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static CreateRelatedObjects Instance
        {
            get;
            private set;
        }


        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return package;
            }
        }
        private IServiceProvider SyncServiceProvider
        {
            get
            {
                return package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CreateRelatedObjects's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CreateRelatedObjects(package, commandService);
        }


        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE dte = SyncServiceProvider.GetService(typeof(DTE)) as DTE ?? throw new ArgumentNullException();
            SelectedItems selectedItems = dte.SelectedItems;
            ProjectItem projectItem;
            Solution2 solution2 = (Solution2)dte.Solution;
            var dialogFactory = SyncServiceProvider.GetService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;


            if (selectedItems == null)
            {
                CommonHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, Messages.NoItemSelected, Messages.ApplicationName);
                return;
            }
            foreach (SelectedItem selectedItem in selectedItems)
            {
                projectItem = selectedItem.ProjectItem;
                if (!CommonHelpers.IsIEntityImplementation(projectItem))
                {
                    CommonHelpers.ShowMessageBox((IServiceProvider)ServiceProvider, Messages.MustBeImplementedFromIEntity, Messages.ApplicationName);
                    return;
                }

                if (projectItem != null)
                {

                    var dialog = CommonHelpers.ShowWaitDialog(dialogFactory);

                    foreach (Project project in dte.Solution.Projects)
                    {
                        GenerateDataAccess(projectItem, solution2, project);

                        GenerateBusiness(projectItem, solution2, project);
                    }

                    dialog.EndWaitDialog();
                }
            }
        }

        private void GenerateBusiness(ProjectItem projectItem, Solution2 solution2, Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project.Name.EndsWith("Business") || project.Name.EndsWith("Bll"))
            {
                CommonHelpers.CreateFoldersIfNotExists(project, new string[] { "Handlers" });

                foreach (ProjectItem item in project.ProjectItems)
                {
                    var projectTemplate = solution2.GetProjectItemTemplate("Interface", "CSharp");
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItem.Name);
                    var fileParameters = new CreateFileParameters
                    {
                        FileNameWithoutExtension = fileNameWithoutExtension,
                        ProjectItem = item,
                        ProjectName = project.Name,
                        ProjectTemplate = projectTemplate
                    };

                    if (item.Name == "Handlers")
                    {
                        CommonHelpers.CreateFoldersIfNotExists(project,
                            new string[] 
                            { 
                                "Handlers\\" + fileNameWithoutExtension.Pluralize() + "\\Commands",
                                "Handlers\\" + fileNameWithoutExtension.Pluralize() + "\\Queries"
                            });
                        BusinessFileService.CreateBusinessAbstract(fileParameters);
                    }

                    if (item.Name == "Common\\DependencyResolvers")
                    {
                        foreach (ProjectItem iocFolder in item.ProjectItems)
                        {
                            BusinessFileService.RegisterAddedFilesToIoc(iocFolder, fileParameters.FileNameWithoutExtension);
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
        private void GenerateDataAccess(ProjectItem projectItem, Solution2 solution2, Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project.Name.EndsWith("DataAccess") || project.Name.EndsWith("Dal"))
            {
                CommonHelpers.CreateFoldersIfNotExists(project, new string[] { "Abstract", "Concrete\\EntityFramework" });

                foreach (ProjectItem item in project.ProjectItems)
                {
                    var projectTemplate = solution2.GetProjectItemTemplate("Interface", "CSharp");
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItem.Name);
                    var fileParameters = new CreateFileParameters
                    {
                        ProjectItem = item,
                        ProjectTemplate = projectTemplate,
                        ProjectName = project.Name,
                        FileNameWithoutExtension = fileNameWithoutExtension,

                    };
                    if (item.Name == "Abstract")
                    {
                        DataAccessFileService.CreateDalAbstract(fileParameters);
                    }
                    if (item.Name == "Concrete")
                    {
                        var efFolder = item.ProjectItems.Cast<ProjectItem>().FirstOrDefault(x => x.Name == "EntityFramework");
                        if (efFolder != null)
                        {
                            fileParameters.ProjectItem = efFolder;
                            DataAccessFileService.CreateDalConcrete(fileParameters);

                            var dbContextItem = efFolder.ProjectItems.Cast<ProjectItem>()
                            .FirstOrDefault(x => x.Name == "Contexts").ProjectItems.Cast<ProjectItem>()
                            .FirstOrDefault();

                            DataAccessFileService.AddDbSetToContext(projectItem,dbContextItem);
                        }

                    }
                }

            }
        }
    }
}
