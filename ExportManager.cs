using Articy.Api;
using Articy.Api.Plugins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MyCompany.TestArticy
{
    public class ExportManager
    {
        private string savePath = string.Empty;
        public const string SettingsJsonName = "\\settings.json";
        public const string ConversationsJsonName = "\\conversations.json";
        public const string CharactersJsonName = "\\characters.json";

        public const string AssetsPathForEmotions = "\\Assets\\ResourcesRaw\\Characters\\Emotions\\";
        private Parser parser;
        private readonly ApiSession mSession;

        public ArticyConversation[] ArticyConversations => parser.Conversations;

        public ExportManager(ApiSession mSession)
        {
            this.mSession = mSession;
        }

        public void Export()
        {
            try
            {
                if (!IsHaveSavePath())
                {
                    mSession.ShowMessageBox("Нужно сначала указать путь до Unity");
                    return;
                }

                DirectoryInfo outDir = new DirectoryInfo(savePath);
                if (!outDir.Exists)
                {
                    mSession.ShowMessageBox("Output directory does not exist! Please specify the asset directory of your Unity project.");
                    return;
                }

                var lookAfterAssets = outDir.GetDirectories("Assets");
                if (lookAfterAssets.Length == 0)
                {
                    mSession.ShowMessageBox("Выбранная папка не является проектом юнити");
                    return;
                }

                DirectoryInfo dataDir = new DirectoryInfo(outDir.FullName + @"\_data\view");
                //if (!pluginsDir.Exists) pluginsDir.Create();

                // prepare export run
                parser = new Parser(mSession);

                //собираем всех персонажей 
                var flow = mSession.RunQuery("SELECT * FROM Flow WHERE ObjectType=Dialogue");

                //собираем всех персонажей
                var mainCharacters = mSession.RunQuery("SELECT * FROM Entities WHERE TemplateName= 'MainCharacters'");

                //обработка диалогов
                foreach (var r in flow.Rows)
                    parser.ProcessDialogues(r);

                //обработка персонадей
                foreach (var r in mainCharacters.Rows)
                    parser.ProcessEntities(r);

                //пробегаемся по диалогам и прокидываем в эмоции диалогов айди их персонажей
                parser.FillEmotionsInConversations();

                //сериализуем и закидываем в юнити
                {
                    string conversations = JsonConvert.SerializeObject(new ArticyConversationsData(parser.Conversations));
                    string characters = JsonConvert.SerializeObject(new ArticyEntitiesData(parser.Entities));

                    File.WriteAllText(dataDir.FullName + ConversationsJsonName, conversations);
                    File.WriteAllText(dataDir.FullName + CharactersJsonName, characters);

                    //копируем ассеты эмоций
                    CopyAssetsToUnity();
                }

                mSession.ShowMessageBox("Complete", null, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                mSession.ShowMessageBox("Error");
            }
        }

        private void CopyAssetsToUnity()
        {
            var emotionsFolder = mSession.RunQuery($"SELECT * FROM Assets WHERE DisplayName == '2dEmotion'").Rows.FirstOrDefault();

            if (emotionsFolder == null)
            {
                mSession.ShowMessageBox("нет папки с эмоциями");
                return;
            }

            foreach (var c in emotionsFolder.GetChildren())
            {
                DirectoryInfo charDir = new DirectoryInfo(savePath + AssetsPathForEmotions + $@"\{c.GetDisplayName()}");
                if (!charDir.Exists) charDir.Create();

                var images = mSession.RunQuery($"SELECT * FROM Assets WHERE IsDescendantOf({c.Id}) AND TemplateName == 'EmotionTextures'");

                foreach (var img in images.Rows)
                {
                    var assetFullFilename = (string)img[ObjectPropertyNames.AbsoluteFilePath];
                    var assetFile = new FileInfo(assetFullFilename);

                    if (assetFile.Exists)
                    {
                        var unityFileName = charDir.FullName + "\\" + img[ObjectPropertyNames.Filename];
                        FileInfo unityFile = new FileInfo(unityFileName);

                        if (unityFile.Exists)
                            continue;

                        assetFile.CopyTo(unityFileName, true);
                    }
                }
            }
        }

        private bool IsHaveSavePath()
        {
            if (savePath == string.Empty)
            {
                if (!File.Exists(Environment.CurrentDirectory + SettingsJsonName))
                    return false;

                var openSetting = File.ReadAllText(Environment.CurrentDirectory + SettingsJsonName);
                savePath = (string)JsonConvert.DeserializeObject(openSetting);
            }

            return true;
        }

        public void SetUnityFolderFolder(MacroCommandDescriptor aDescriptor, List<ObjectProxy> aSelectedObjects)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();
            savePath = folderBrowserDialog.SelectedPath;
            File.WriteAllText(Environment.CurrentDirectory + SettingsJsonName, JsonConvert.SerializeObject(savePath));
        }
    }
}