using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace FVTC.LearningInnovations.Unity.Initiator
{

    public abstract class LearningInnovationsInitator
    {   
        protected static void GitInit()
        {
            bool success;
            using (var process = Git("init"))
            {
                string stdOutLine;

                try
                {
                    while ((stdOutLine = process.StandardError.ReadLine()) != null)
                    {
                        EditorUtility.DisplayProgressBar("git init", stdOutLine, 0f);
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
                const string gitIgnoreUrl = "https://raw.githubusercontent.com/github/gitignore/master/Unity.gitignore";

                var assetsDir = new DirectoryInfo(Application.dataPath);

                if (assetsDir.Exists)
                {
                    var projectDir = assetsDir.Parent;

                    if (projectDir.Exists)
                    {
                        string gitIgnorePath = System.IO.Path.Combine(projectDir.FullName, ".gitignore");

                        if (!System.IO.File.Exists(gitIgnorePath))
                        {

                            try
                            {
                                EditorUtility.DisplayProgressBar("Creating .gitignore file.", "Downloading .gitignore file from " + gitIgnoreUrl, 0f);


#if !UNITY_2018_1_OR_NEWER
                                using (WWW www = new WWW(gitIgnoreUrl))
                                {
                                    while (!www.isDone)
                                    {
                                        EditorUtility.DisplayProgressBar("Creating .gitignore file.", "Downloading .gitignore file from " + gitIgnoreUrl, www.progress);
                                        System.Threading.Thread.Sleep(100);
                                    }
                                    System.IO.File.WriteAllText(gitIgnorePath, www.text);
                                }
#else
                                using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(gitIgnoreUrl))
                                {
                                    request.SendWebRequest();

                                    while (!request.isDone)
                                    {
                                        EditorUtility.DisplayProgressBar("Creating .gitignore file.", "Downloading .gitignore file from " + gitIgnoreUrl, request.downloadProgress);
                                        System.Threading.Thread.Sleep(100);
                                    }

                                    if (request.isNetworkError)
                                    {
                                        EditorUtility.DisplayDialog("Download Failed", string.Format("Downloading .gitignore file from {0} failed.", gitIgnoreUrl), "Close");
                                    }
                                    else
                                    {
                                        File.WriteAllText(gitIgnorePath, request.downloadHandler.text);
                                    }
                                }
#endif

                            }
                            finally
                            {
                                EditorUtility.ClearProgressBar();
                            }
                        }
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Git", "Something went wrong when initializing new Git repository", "OK");
            }
        }

        protected static bool IsGitModuleInstalled(string url)
        {
            var gitModulesFile = new FileInfo(Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".gitmodules"));

            if (gitModulesFile.Exists)
            {
                bool isModule = false;

                using (var reader = gitModulesFile.OpenText())
                {
                    string line;
                    string[] lineParts;

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
                                    case "url":
                                        if (lineParts[1].Equals(url, StringComparison.OrdinalIgnoreCase))
                                        {
                                            return true;
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

            return false;
        }

        protected static void AddGitSubmodule(string url, string path)
        {
            using (var process = Git(string.Format("submodule add {0} {1}", url, path)))
            {
                string stdOutLine;

                try
                {
                    while ((stdOutLine = process.StandardError.ReadLine()) != null)
                    {
                        EditorUtility.DisplayProgressBar("git submodule add", stdOutLine, 0f);
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }

                process.WaitForExit();
            }

            AssetDatabase.Refresh();
        }

        protected static bool PromptUserToDownloadGitIfNotInstalled()
        {
            if (!IsGitInstalled)
            {
                if (EditorUtility.DisplayDialog("Install Git?", "It looks like you may not have Git installed.\r\nDo you want to download it now?", "Download", "Close"))
                {
                    Process.Start("https://git-scm.com/downloads");
                }

                return false;
            }

            return true;
        }

        protected static bool IsGitInstalled
        {
            get
            {
                return GetExecutablePath("git.exe") != null;
            }
        }

        protected static string GetExecutablePath(string executableName)
        {
            var exePath = Environment.GetEnvironmentVariable("PATH").Split(';').Select(p => Path.Combine(p, executableName)).Where(p => File.Exists(p)).FirstOrDefault();

            return exePath;
        }

        protected static bool IsProjectGitRepository
        {
            get
            {
                // enable InitializeGitRepository only if the .git dir does not exist already

                var assetsDir = new DirectoryInfo(Application.dataPath);

                if (assetsDir.Exists)
                {
                    var projectDir = assetsDir.Parent;

                    if (projectDir.Exists)
                    {
                        var dotGitDir = projectDir.GetDirectories(".git");

                        return dotGitDir != null && dotGitDir.Length == 1 && dotGitDir[0].Exists;
                    }
                }

                return false;
            }
        }

        public static Process Git(string command)
        {
            var gitExe = GetExecutablePath("git.exe");

            if (string.IsNullOrEmpty(gitExe))
            {
                return null;
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = gitExe,
                    WorkingDirectory = System.IO.Directory.GetParent(Application.dataPath).FullName,
                    UseShellExecute = false,
                    Arguments = command,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                return Process.Start(startInfo);
            }
        }
    }
}
