#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using LAS.Config;

namespace LAS.Editor
{
    /// <summary>
    /// Centralized menu structure for all LAS (Ladders and Snakes) editor tools
    /// Professional, organized hierarchy for easy access
    /// </summary>
    public static class LASEditorMenu
    {
        private const string MENU_ROOT = "LAS/";

        // Menu priorities to control ordering
        private const int PRIORITY_SCENES = 0;
        private const int PRIORITY_BOARD = 100;
        private const int PRIORITY_UI = 200;
        private const int PRIORITY_SETUP = 300;
        private const int PRIORITY_DOCS = 400;

        #region Scenes Menu

        [MenuItem(MENU_ROOT + "Scenes/Build All Scenes", false, PRIORITY_SCENES + 1)]
        public static void BuildAllScenes()
        {
            SceneBuilder.BuildAllScenes();
        }

        [MenuItem(MENU_ROOT + "Scenes/Build MainMenu Scene", false, PRIORITY_SCENES + 2)]
        public static void BuildMainMenuScene()
        {
            SceneBuilder.BuildMainMenuScene();
        }

        [MenuItem(MENU_ROOT + "Scenes/Build Game Scene", false, PRIORITY_SCENES + 3)]
        public static void BuildGameScene()
        {
            SceneBuilder.BuildGameScene();
        }

        [MenuItem(MENU_ROOT + "Scenes/Configure Build Settings", false, PRIORITY_SCENES + 4)]
        public static void ConfigureBuildSettings()
        {
            SceneBuilder.ConfigureBuildSettings();
        }

        #endregion

        #region Board Menu

        [MenuItem(MENU_ROOT + "Board/Setup Board in Scene", false, PRIORITY_BOARD + 1)]
        public static void SetupDefaultBoard()
        {
            DefaultBoardSceneSetup.SetupDefaultBoard();
        }

        [MenuItem(MENU_ROOT + "Board/Clear Board from Scene", false, PRIORITY_BOARD + 2)]
        public static void ClearBoard()
        {
            DefaultBoardSceneSetup.ClearBoard();
        }

        [MenuItem(MENU_ROOT + "Board/Board Generator Window", false, PRIORITY_BOARD + 11)]
        public static void OpenBoardGenerator()
        {
            BoardGeneratorWindow.ShowWindow();
        }

        [MenuItem(MENU_ROOT + "Board/Create Default Board Config", false, PRIORITY_BOARD + 21)]
        public static void CreateDefaultBoardConfig()
        {
            BoardConfigGenerator.CreateDefaultBoardConfig();
        }

        [MenuItem(MENU_ROOT + "Board/Select Board Config", false, PRIORITY_BOARD + 22)]
        public static void SelectBoardConfig()
        {
            BoardConfigGenerator.SelectBoardConfig();
        }

        [MenuItem(MENU_ROOT + "Board/Generate Random Board/Easy", false, PRIORITY_BOARD + 31)]
        public static void GenerateRandomBoardEasy()
        {
            BoardConfigGenerator.GenerateRandomBoardEasy();
        }

        [MenuItem(MENU_ROOT + "Board/Generate Random Board/Medium", false, PRIORITY_BOARD + 32)]
        public static void GenerateRandomBoardMedium()
        {
            BoardConfigGenerator.GenerateRandomBoardMedium();
        }

        [MenuItem(MENU_ROOT + "Board/Generate Random Board/Hard", false, PRIORITY_BOARD + 33)]
        public static void GenerateRandomBoardHard()
        {
            BoardConfigGenerator.GenerateRandomBoardHard();
        }

        [MenuItem(MENU_ROOT + "Board/Generate Random Board/Extreme", false, PRIORITY_BOARD + 34)]
        public static void GenerateRandomBoardExtreme()
        {
            BoardConfigGenerator.GenerateRandomBoardExtreme();
        }

        #endregion

        #region UI Menu

        [MenuItem(MENU_ROOT + "UI/UI Generator Window", false, PRIORITY_UI + 1)]
        public static void OpenUIGenerator()
        {
            UIGeneratorWindow.ShowWindow();
        }

        [MenuItem(MENU_ROOT + "UI/UI Reference Tool Window", false, PRIORITY_UI + 2)]
        public static void OpenUIReferenceTool()
        {
            UIReferenceToolWindow.ShowWindow();
        }

        [MenuItem(MENU_ROOT + "UI/Bind All UI References in Scene", false, PRIORITY_UI + 11)]
        public static void BindAllUIReferences()
        {
            UIReferenceEditorMenu.BindAllUIReferencesInScene();
        }

        [MenuItem(MENU_ROOT + "UI/Bind Selected GameObject UI References", false, PRIORITY_UI + 12)]
        public static void BindSelectedUIReferences()
        {
            UIReferenceEditorMenu.BindSelectedUIReferences();
        }

        [MenuItem(MENU_ROOT + "UI/Validate All UI References", false, PRIORITY_UI + 13)]
        public static void ValidateAllUIReferences()
        {
            UIReferenceEditorMenu.ValidateAllUIReferences();
        }

        [MenuItem(MENU_ROOT + "UI/Add Auto-Binding Component to Selected", false, PRIORITY_UI + 21)]
        public static void AddAutoBindingComponent()
        {
            UIReferenceEditorMenu.AddAutoBindingComponent();
        }

        [MenuItem(MENU_ROOT + "UI/Generate UI from Selected Component", false, PRIORITY_UI + 22)]
        public static void GenerateUIFromComponent()
        {
            UIReferenceEditorMenu.GenerateUIFromSelectedComponent();
        }

        #endregion

        #region Setup Menu

        [MenuItem(MENU_ROOT + "Setup/Quick Scene Setup", false, PRIORITY_SETUP + 1)]
        public static void QuickSceneSetup()
        {
            QuickUISetup.SetupRuntimeUIBuilder();
        }

        #endregion

        #region Documentation

        [MenuItem(MENU_ROOT + "Documentation/Open Implementation Guide", false, PRIORITY_DOCS + 1)]
        public static void OpenImplementationGuide()
        {
            string path = "Assets/IMPLEMENTATION_GUIDE.md";
            if (System.IO.File.Exists(path))
            {
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, 1);
            }
            else
            {
                EditorUtility.DisplayDialog("Guide Not Found",
                    "Implementation guide not found at: " + path + "\n\nPlease ensure IMPLEMENTATION_GUIDE.md exists in the Assets folder.",
                    "OK");
            }
        }

        [MenuItem(MENU_ROOT + "Documentation/Project Architecture", false, PRIORITY_DOCS + 2)]
        public static void ShowArchitectureInfo()
        {
            EditorUtility.DisplayDialog("LAS Project Architecture",
                "📁 PROJECT STRUCTURE:\n\n" +
                "LAS/\n" +
                "├── Config/         - ScriptableObject configurations\n" +
                "├── Core/           - Core utilities & service locator\n" +
                "├── Editor/         - Editor tools & windows\n" +
                "├── Entities/       - Game entities (Board, Dice, Player)\n" +
                "├── Events/         - Event system\n" +
                "├── Gameplay/       - Game logic & controllers\n" +
                "├── Networking/     - Multiplayer networking\n" +
                "└── UI/             - UI controllers\n\n" +
                "All tools are accessible via the LAS menu.",
                "OK");
        }

        [MenuItem(MENU_ROOT + "Documentation/About", false, PRIORITY_DOCS + 11)]
        public static void ShowAbout()
        {
            EditorUtility.DisplayDialog("About LAS",
                "LADDERS AND SNAKES\n" +
                "Professional Unity Implementation\n\n" +
                "Version: 1.0\n" +
                "Architecture: Clean, modular, event-driven\n\n" +
                "Features:\n" +
                "• Procedural board generation\n" +
                "• AI opponents\n" +
                "• Local & online multiplayer\n" +
                "• Automated UI generation\n" +
                "• Complete editor tooling\n\n" +
                "All tools organized under the LAS menu.",
                "OK");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Opens a documentation file in the default text editor
        /// </summary>
        private static void OpenDocumentation(string filename)
        {
            string path = "Assets/" + filename;
            if (System.IO.File.Exists(path))
            {
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, 1);
            }
            else
            {
                EditorUtility.DisplayDialog("File Not Found",
                    $"Documentation file not found:\n{path}",
                    "OK");
            }
        }

        #endregion
    }
}
#endif
