using UnityEditor;


namespace FVTC.LearningInnovations.Unity.Initiator
{
    public class MenuUnityLibInstaller : LearningInnovationsInitator
    {
        const string UNITY_LIB_MODULE_URL = "https://github.com/Wisc-Online/Unity-Lib.git";
        const string UNITY_LIB_MODULE_PATH = "Assets/FVTC/LearningInnovations";

        [MenuItem("Learning Innovations/Initiator/Install Unity-Lib")]
        static void InstallUnityLib()
        {
            if (PromptUserToDownloadGitIfNotInstalled())
            {
                if (!IsProjectGitRepository)
                {
                    if (EditorUtility.DisplayDialog("Initialize new Git Repository?", "It appears that this project is not a Git repository.  Do you want to intialize a new Git repository?", "Yes", "No"))
                    {
                        GitInit();


                        AddUnityLibSubmodule();
                    }
                }
                else
                {
                    AddUnityLibSubmodule();
                }
            }
        }

        [MenuItem("Learning Innovations/Initiator/Install Unity-Lib", true)]
        static bool ValidateInstallUnityLib()
        {
            return !IsGitModuleInstalled(UNITY_LIB_MODULE_URL);
        }

        protected static void AddUnityLibSubmodule()
        {
            AddGitSubmodule(UNITY_LIB_MODULE_URL, UNITY_LIB_MODULE_PATH);
        }
    }
}
