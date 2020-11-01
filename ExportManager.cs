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
        private const string SavePathKey = "SavePathKey";
        public const string SettingsJsonName = "\\settings.json";
        public const string ConversationsJsonName = "\\conversations.json";
        public const string CharactersJsonName = "\\characters.json";
        public const string BubbleTextsJsonName = "\\bubble-texts.json";
        public const string NotificationTextsJsonName = "\\notification-texts.json";
        public const string FadeTextsJsonName = "\\fade-texts.json";
        public const string AnswersJsonName = "\\answers.json";
        public const string TexturesOffsetJsonName = "\\textures-offset.json";
        public const string ArticyTriggersJsonName = "\\articy-triggers.json";

        public const string AssetsPathForEmotions = "\\Assets\\ResourcesRaw\\Characters\\Emotions\\";
        public const string AssetsPathForEmoji = "\\Assets\\ResourcesRaw\\Characters\\Emoji\\";
        public const string AssetsPathForQuestIcon = "\\Assets\\ResourcesRaw\\Characters\\QuestIcon\\";
        public const string AssetsPathForBackgrounds = "\\Assets\\ResourcesRaw\\Characters\\Backgrounds\\";


        private Dictionary<int, string> ResourcesKeys = new Dictionary<int, string>()
        {
            {0, "money" },
        };


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

                DirectoryInfo dataViewDir = new DirectoryInfo(outDir.FullName + @"\_data\view");
                DirectoryInfo dataGameDir = new DirectoryInfo(outDir.FullName + @"\_data\game");
                //if (!pluginsDir.Exists) pluginsDir.Create();

                // prepare export run
                parser = new Parser(mSession);

                //собираем все диалоги 
                var flow = mSession.RunQuery("SELECT * FROM Flow WHERE ObjectType=Dialogue");
                var connections = mSession.RunQuery("SELECT * FROM Flow WHERE ObjectType=Connection");


                //собираем всех персонажей
                var mainCharacters = mSession.RunQuery("SELECT * FROM Entities WHERE TemplateName= 'MainCharacters'");

                //собираем варианты ответов в диалогах
                var playerChoises = mSession.RunQuery("SELECT * FROM Flow WHERE TemplateName= 'playerChoise'");

                //тут собираем все тексты для текст баблов
                var bubbleTexts = mSession.RunQuery("SELECT * FROM Flow WHERE TemplateName = 'BubbleText'");

                //удивительно, но тут мы собираем квесты
                var quests = mSession.RunQuery("SELECT * FROM Flow WHERE TemplateName = 'StandartQuest'");

                //тут собираем все тексты для затемнения
                var screenFaderTexts = mSession.RunQuery("SELECT * FROM Flow WHERE TemplateName = 'ScreenFader'");

                //тут собираем все тексты для затемнения
                var notificationTexts = mSession.RunQuery("SELECT * FROM Flow WHERE TemplateName = 'NotificationText'");

                //обработка диалогов
                foreach (var r in flow.Rows)
                    parser.ProcessDialogues(r);

                //обработка квестов
                foreach (var q in quests.Rows)
                    parser.ProcessQuests(q);

                //обработка персонажей
                foreach (var r in mainCharacters.Rows)
                    parser.ProcessEntities(r);

                //обработка ответов
                foreach (var pc in playerChoises.Rows)
                    parser.ProcessPlayerChoices(pc);

                //сюда заталкиваем все связи вообще, чтобы например прокинуть для квестов линки
                parser.ProcessConnections(connections.Rows);

                //пробегаемся по диалогам и прокидываем в эмоции диалогов айди их персонажей
                parser.FillEmotionsInConversations();

                //обрабатываем баблы
                foreach (var bd in bubbleTexts.Rows)
                    parser.ProcessBubbleTexts(bd);

                //обрабатываем тексты для фейда
                foreach (var sft in screenFaderTexts.Rows)
                    parser.ProcessScreenFaderText(sft);

                //обрабатываем тексты для нотификаций
                foreach (var nt in notificationTexts.Rows)
                    parser.ProcessNotificationText(nt);

                //сериализуем и закидываем в юнити
                {
                    string conversations = JsonConvert.SerializeObject(ConversationAdapter());
                    string characters = JsonConvert.SerializeObject(CharactersAdapter());
                    string answers = JsonConvert.SerializeObject(AnswersAdapter());
                    string bubbleTextsToJson = JsonConvert.SerializeObject(BubbleTextsAdapter());
                    string screenFaderTextsToJson = JsonConvert.SerializeObject(FadeTextsAdapter());
                    string notificationTextsToJson = JsonConvert.SerializeObject(NotificationTextsAdapter());
                    string textureOffsetsToJson = JsonConvert.SerializeObject(TextureOffsetsAdapter());
                    //string testTriggerView = JsonConvert.SerializeObject(TriggersViewAdapter());
                    //string testTrigger = JsonConvert.SerializeObject(TriggersAdapter());

                    File.WriteAllText(dataViewDir.FullName + ConversationsJsonName, conversations);
                    File.WriteAllText(dataViewDir.FullName + CharactersJsonName, characters);
                    File.WriteAllText(dataViewDir.FullName + AnswersJsonName, answers);
                    File.WriteAllText(dataViewDir.FullName + BubbleTextsJsonName, bubbleTextsToJson);
                    File.WriteAllText(dataViewDir.FullName + FadeTextsJsonName, screenFaderTextsToJson);
                    File.WriteAllText(dataViewDir.FullName + NotificationTextsJsonName, notificationTextsToJson);
                    File.WriteAllText(dataViewDir.FullName + TexturesOffsetJsonName, textureOffsetsToJson);
                    //File.WriteAllText(dataViewDir.FullName + ArticyTriggersJsonName, testTriggerView);
                    //File.WriteAllText(dataGameDir.FullName + ArticyTriggersJsonName, testTrigger);

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

        private Dictionary<string, ArticyConversation> ConversationAdapter()
        {
            Dictionary<string, ArticyConversation> conversationFacade = new Dictionary<string, ArticyConversation>(100);
            foreach (var c in parser.Conversations)
                conversationFacade.Add(c.ConversationId, c);

            return conversationFacade;
        }

        private Dictionary<long, ArticyEntity> CharactersAdapter()
        {
            Dictionary<long, ArticyEntity> conversationFacade = new Dictionary<long, ArticyEntity>(100);
            foreach (var e in parser.Entities)
                conversationFacade.Add(e.EntityId, e);

            return conversationFacade;
        }

        private Dictionary<string, TriggersAdapter> TriggersAdapter()
        {
            Dictionary<string, TriggersAdapter> conversationFacade = new Dictionary<string, TriggersAdapter>(64);
            foreach (var e in parser.ArticyQuests)
                conversationFacade.Add(e.Id, new TriggersAdapter(e));

            return conversationFacade;
        }

        private Dictionary<string, TriggersViewAdapter> TriggersViewAdapter()
        {
            Dictionary<string, TriggersViewAdapter> conversationFacade = new Dictionary<string, TriggersViewAdapter>(64);
            foreach (var e in parser.ArticyQuests)
                conversationFacade.Add(e.Id, new TriggersViewAdapter(e));

            return conversationFacade;
        }

        private Dictionary<string, ArticyBubbleText> BubbleTextsAdapter()
        {
            Dictionary<string, ArticyBubbleText> conversationFacade = new Dictionary<string, ArticyBubbleText>(256);
            foreach (var e in parser.ArticyBubbleTexts)
                conversationFacade.Add(e.Id, e);

            return conversationFacade;
        }

        private Dictionary<string, ArticyText> FadeTextsAdapter()
        {
            Dictionary<string, ArticyText> conversationFacade = new Dictionary<string, ArticyText>(256);
            foreach (var e in parser.ScreenFaderTexts)
                conversationFacade.Add(e.Id, e);

            return conversationFacade;
        }

        private Dictionary<string, ArticyText> NotificationTextsAdapter()
        {
            Dictionary<string, ArticyText> conversationFacade = new Dictionary<string, ArticyText>(256);
            foreach (var e in parser.NotificationTexts)
                conversationFacade.Add(e.Id, e);

            return conversationFacade;
        }

        private Dictionary<string, ArticyTextureOffset> TextureOffsetsAdapter()
        {
            Dictionary<string, ArticyTextureOffset> conversationFacade = new Dictionary<string, ArticyTextureOffset>(256);
            foreach (var e in parser.ArticyTextureOffsets)
                conversationFacade.Add(e.Id, e);

            return conversationFacade;
        }

        private Dictionary<string, ArticyAnswer> AnswersAdapter()
        {
            Dictionary<string, ArticyAnswer> conversationFacade = new Dictionary<string, ArticyAnswer>(64);
            foreach (var e in parser.ArticyAnswers)
                conversationFacade.Add(e.Id, e);

            return conversationFacade;
        }


        private void CopyAssetsToUnity()
        {
            var emotionsFolder = mSession.RunQuery($"SELECT * FROM Assets WHERE DisplayName == '2dEmotion'").Rows.FirstOrDefault();
            var emoji = mSession.RunQuery($"SELECT * FROM Assets WHERE TemplateName == 'DreamBubble'");
            var emojiStandart = mSession.RunQuery($"SELECT * FROM Assets WHERE TemplateName == 'Emoji2d'");
            var backgound = mSession.RunQuery($"SELECT * FROM Assets WHERE TemplateName == 'Background '");
            var bubblePicture = mSession.RunQuery($"SELECT * FROM Assets WHERE TemplateName == 'BubblePicture'");
            var questIcon = mSession.RunQuery($"SELECT * FROM Assets WHERE TemplateName == 'QuestIcon'");

            if (emotionsFolder == null)
            {
                mSession.ShowMessageBox("нет папки с эмоциями");
                return;
            }

            foreach (var c in emotionsFolder.GetChildren())
            {
                DirectoryInfo charDir = new DirectoryInfo(savePath + AssetsPathForEmotions + $@"\{c.GetDisplayName()}\");
                if (!charDir.Exists) charDir.Create();

                //это выборка эмоций лежащих в той же папке что и ентити (персонаж) 
                var images = mSession.RunQuery($"SELECT * FROM Assets WHERE IsDescendantOf({c.Id}) AND TemplateName == 'EmotionTextures'");
                var emotions = parser.Entities.SelectMany(x => x.Emotions).ToList();

                foreach (var img in images.Rows)
                {
                    var assetFullFilename = (string)img[ObjectPropertyNames.AbsoluteFilePath];
                    var assetFilename = (string)img[ObjectPropertyNames.Filename];
                    var assetFile = new FileInfo(assetFullFilename);

                    var neededEmotion = emotions.FirstOrDefault(x => x.EmotionFileName == assetFilename);

                    if (neededEmotion == null)
                        continue;

                    var unityFileName = charDir.FullName + neededEmotion.EmotionName + neededEmotion.EmotionId + assetFile.Extension;
                    FileInfo unityFile = new FileInfo(unityFileName);

                    if (assetFile.Exists)
                    {
                        if (unityFile.Exists)
                        {
                            if (unityFile.Length == assetFile.Length)
                                continue;
                        }

                        File.Copy(assetFile.FullName, unityFile.FullName, true);
                    }
                }


                SaveBackgrounds(backgound.Rows);
                SaveEmoji(emoji.Rows);
                SaveEmoji(emojiStandart.Rows);
                SaveBubblePicture(bubblePicture.Rows);
                SaveQuestrIcon(questIcon.Rows);
            }
        }

        private void SaveBubblePicture(List<ObjectProxy> emojies)
        {
            foreach (var e in emojies)
            {
                var assetFullFilename = (string)e[ObjectPropertyNames.AbsoluteFilePath];
                var assetFilename = (string)e[ObjectPropertyNames.Filename];
                var assetFile = new FileInfo(assetFullFilename);

                if (!parser.Conversations.Any(x => x.Dialogs.Any(z => z.BubblePicture != null && z.BubblePicture.FileName == assetFilename)))
                    continue;

                SaveToPath(assetFile, AssetsPathForEmoji, ((long)e.Id).ToString());
            }
        }

        private void SaveQuestrIcon(List<ObjectProxy> emojies)
        {
            foreach (var e in emojies)
            {
                var assetFullFilename = (string)e[ObjectPropertyNames.AbsoluteFilePath];
                var assetFilename = (string)e[ObjectPropertyNames.Filename];
                var assetFile = new FileInfo(assetFullFilename);
                var assetFilenameWithoutExtention = Path.GetFileNameWithoutExtension(assetFilename);

                if (!parser.ArticyQuests.Any(x => x.IconFileName == assetFilenameWithoutExtention))
                    continue;

                SaveToPath(assetFile, AssetsPathForQuestIcon, assetFilenameWithoutExtention);
            }
        }


        private void SaveBackgrounds(List<ObjectProxy> emojies)
        {
            foreach (var e in emojies)
            {
                var assetFullFilename = (string)e[ObjectPropertyNames.AbsoluteFilePath];
                var assetFilename = (string)e[ObjectPropertyNames.Filename];
                var assetFile = new FileInfo(assetFullFilename);
                var assetFilenameWithoutExtention = Path.GetFileNameWithoutExtension(assetFilename);

                if (!parser.Conversations.Any(x => x.Dialogs.Any(z => z.ArticyBackground != null && z.ArticyBackground.FileName == assetFilename)))
                    continue;

                SaveToPath(assetFile, AssetsPathForBackgrounds, assetFilenameWithoutExtention);
            }
        }

        private void SaveEmoji(List<ObjectProxy> emojies)
        {
            foreach (var e in emojies)
            {
                var assetFullFilename = (string)e[ObjectPropertyNames.AbsoluteFilePath];
                var assetFilename = (string)e[ObjectPropertyNames.Filename];
                var assetFile = new FileInfo(assetFullFilename);

                if (!parser.Conversations.Any(x => x.Dialogs.Any(z => z.ArticyComicsEffect != null && z.ArticyComicsEffect.FileName == assetFilename)))
                    continue;

                SaveToPath(assetFile, AssetsPathForEmoji, ((long)e.Id).ToString());
            }
        }

        private void SaveToPath(FileInfo copyFromFile, string subFolderUnityPath, string filename)
        {
            if (!copyFromFile.Exists)
                return;

            var needDir = new DirectoryInfo(savePath + subFolderUnityPath);
            if (!needDir.Exists) Directory.CreateDirectory(needDir.FullName);

            var unityFileName = needDir.FullName + filename + copyFromFile.Extension;

            FileInfo unityFile = new FileInfo(unityFileName);

            if (unityFile.Exists)
            {
                if (unityFile.Length == copyFromFile.Length)
                    return;
            }

            File.Copy(copyFromFile.FullName, unityFile.FullName, true);
        }

        private bool IsHaveSavePath()
        {
            savePath = Properties.Settings.Default.SavePath;
            return savePath != string.Empty;
        }

        public void SetUnityFolderFolder(MacroCommandDescriptor aDescriptor, List<ObjectProxy> aSelectedObjects)
        {
            var folderBrowserDialog = new FolderBrowserDialog();

            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result != DialogResult.OK)
                return;

            savePath = folderBrowserDialog.SelectedPath;
            Properties.Settings.Default.SavePath = savePath;
            Properties.Settings.Default.Save();
        }
    }
}