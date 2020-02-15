using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FVTC.LearningInnovations.Unity.Initiator
{
    public class MenuSubmoduleInit : LearningInnovationsInitator
    {
        [MenuItem("Learning Innovations/Initiator/Initialize Submodule(s)")]
        static void SubmoduleInit()
        {
            if (PromptUserToDownloadGitIfNotInstalled())
            {
                bool success;

                string stdOutLine;
                try
                {
                    using (var process = Git("submodule init"))
                    {
                        try
                        {
                            while ((stdOutLine = process.StandardError.ReadLine()) != null)
                            {
                                EditorUtility.DisplayProgressBar("Initializing Submodules", stdOutLine, 0f);
                            }
                        }
                        finally
                        {
                            EditorUtility.ClearProgressBar();
                        }

                        process.WaitForExit();

                        success = process.ExitCode == 0;
                    }

                    if (success)
                    {
                        using (var process = Git("submodule update"))
                        {
                            try
                            {
                                while ((stdOutLine = process.StandardError.ReadLine()) != null)
                                {
                                    EditorUtility.DisplayProgressBar("Updating Submodules", stdOutLine, 0f);
                                }
                            }
                            finally
                            {
                                EditorUtility.ClearProgressBar();
                            }

                            process.WaitForExit();

                            success = process.ExitCode == 0;
                        }
                    }

                    if (success)
                    {
                        using (var process = Git("submodule foreach git checkout master"))
                        {
                            try
                            {
                                while ((stdOutLine = process.StandardError.ReadLine()) != null)
                                {
                                    EditorUtility.DisplayProgressBar("Checking out submodules to master branch.", stdOutLine, 0f);
                                }
                            }
                            finally
                            {
                                EditorUtility.ClearProgressBar();
                            }

                            process.WaitForExit();

                            success = process.ExitCode == 0;
                        }
                    }
                }
                finally
                {
                    AssetDatabase.Refresh();
                }
            }
        }

        [MenuItem("Learning Innovations/Initiator/Initialize Submodule(s)", true)]
        static bool ValidateSubmoduleInit()
        {

            FileInfo gitModulesFile = new FileInfo(Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".gitmodules"));

            if (gitModulesFile.Exists)
            {
                bool isModule = false;

                using (var reader = gitModulesFile.OpenText())
                {
                    string line;
                    string[] lineParts;
                    DirectoryInfo submoduleDirectory;

                    const string subModulePrefix = "[submodule ";

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith(subModulePrefix))
                        {
                            isModule = true;
                        }
                        else if (isModule)
                        {
                            lineParts = line.Split(new char[] { '=' }, 2).Select(p => p.Trim()).ToArray();

                            if (lineParts.Length == 2)
                            {
                                switch (lineParts[0])
                                {
                                    case "path":

                                        // ensure the path exists (and contains a .git file) 

                                        submoduleDirectory = new DirectoryInfo(Path.Combine(Directory.GetParent(Application.dataPath).FullName, lineParts[1]));

                                        if (!submoduleDirectory.Exists)
                                        {
                                            // the submodule's directory does not exist yet, it needs initialization
                                            return true;
                                        }
                                        else
                                        {
                                            var dotGitFiles = submoduleDirectory.GetFiles(".git");

                                            if (dotGitFiles == null || dotGitFiles.Length != 1)
                                            {
                                                // the submodule's directory exists, but does not contain a .git file

                                                return true;
                                            }
                                        }

                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            // if we get this far then there are either no submodules or all submodules are initialized.
            return false;
        }
    }
}
