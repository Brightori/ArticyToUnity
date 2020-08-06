using Articy.Api;
using System.Collections.Generic;
using System.Linq;

namespace MyCompany.TestArticy
{
    public class Parser
    {
        private List<ArticyConversation> conversations = new List<ArticyConversation>(64);
        private List<ArticyEntity> entities = new List<ArticyEntity>(64);
        private List<ArticyAnswer> articyAnswers = new List<ArticyAnswer>(64);
        private List<ArticyBubbleText> articyBubbleTexts = new List<ArticyBubbleText>(256);
        private List<ArticyQuest> articyQuests = new List<ArticyQuest>(64);
        private ApiSession apiSession;

        public ArticyConversation[] Conversations => conversations.ToArray();
        public ArticyEntity[] Entities => entities.ToArray();
        public ArticyAnswer[] ArticyAnswers => articyAnswers.ToArray();
        public ArticyBubbleText[] ArticyBubbleTexts => articyBubbleTexts.ToArray();
        public ArticyQuest[] ArticyQuests => articyQuests.ToArray();

        public Parser(ApiSession apiSession)
        {
            this.apiSession = apiSession;

        }

        public void ProcessDialogues(ObjectProxy child)
        {
            var conversation = new ArticyConversation();

            conversation.ConversationId = "0x" + ((long)child.Id).ToString("X16");
            conversations.Add(conversation);

            var childs = child.GetChildren();

            foreach (var c in childs)
                GetDataForConversation(c, conversation);

            var firstConnections = childs.Where(x => x.ObjectType == ObjectType.Connection).Where(z => (z[ObjectPropertyNames.Source] as ObjectProxy).Id == child.Id);
            foreach (var f in firstConnections)
            {
                if (f.HasProperty(ObjectPropertyNames.Target))
                {
                    if ((f[ObjectPropertyNames.Target] as ObjectProxy).ObjectType == ObjectType.DialogueFragment)
                        conversation.StartDialogs.Add((long)(f[ObjectPropertyNames.Target] as ObjectProxy).Id);
                }
            }
        }

        private void GetDataForConversation(ObjectProxy objectProxy, ArticyConversation conversation)
        {
            switch (objectProxy.ObjectType)
            {
                case ObjectType.Connection:
                    ProccessConnection(objectProxy, conversation);
                    break;

                case ObjectType.DialogueFragment:
                    ProccessDialogueFragment(objectProxy, conversation);
                    break;
            }
        }

        private void ProccessConnection(ObjectProxy connection, ArticyConversation conversation)
        {
            if (connection.HasProperty(ValuesHelper.Parent))
            {
                if (!connection.HasProperty(ObjectPropertyNames.SourcePin))
                    return;

                var parent = (connection[ObjectPropertyNames.SourcePin] as ObjectProxy).GetParent().Id;

                if (connection.HasProperty(ValuesHelper.Target))
                {
                    var convertToProxy = connection[ValuesHelper.Target] as ObjectProxy;
                    if (convertToProxy.ObjectType != ObjectType.DialogueFragment)
                        return;

                    var target = convertToProxy.Id;

                    if (conversation.Dialogs == null || conversation.Dialogs.Count == 0)
                        return;

                    foreach (var d in conversation.Dialogs)
                    {
                        if (d.Id == (long)parent)
                        {
                            d.NextStepsIds.Add((long)target);
                        }
                    }
                }
            }
        }

        public void ProcessQuests(ObjectProxy q)
        {
            var quest = new ArticyQuest();

            quest.Id = "0x" + ((long)q.Id).ToString("X16");
            quest.LongId = (long)q.Id;
            quest.DisplayId = q.GetDisplayName();

            if (q[ObjectPropertyNames.PreviewImageAsset] != null)
                quest.IconFileName = (string)(q[ObjectPropertyNames.PreviewImageAsset] as ObjectProxy)[ObjectPropertyNames.Filename];

            var questAttachments = q[ObjectPropertyNames.Attachments] as List<ObjectProxy>;

            if (questAttachments != null && questAttachments.Count > 0)
            {
                foreach (var go in questAttachments)
                {
                    int resourceType = (int)go["SingleResource.ResourceType"];
                    float resourceamount = (float)(double)go["SingleResource.Amount"];
                    quest.rewards.Add(new Сurrency { Type = resourceType, Amount = resourceamount });
                }
            }

            if (q.HasProperty(ValuesHelper.BuildingToUpgrade))
            {
                var checkBuildingToUpgrade = q[ValuesHelper.BuildingToUpgrade];
                if (checkBuildingToUpgrade != null)
                {
                    var upgrade = (checkBuildingToUpgrade as ObjectProxy);
                    quest.UpgradeBuildingInfo.DisplayId = upgrade.GetDisplayName();
                    quest.UpgradeBuildingInfo.Id = (long)upgrade.Id;
                    quest.UpgradeBuildingInfo.ExternalId = upgrade.GetExternalId();
                    quest.UpgradeBuildingInfo.FileInfo = (string)upgrade[ObjectPropertyNames.Filename];
                }
            }

            articyQuests.Add(quest);
        }

        internal void ProcessConnections(List<ObjectProxy> rows)
        {
            ProcessQuestsConnections(rows);
        }

        private string ConvertLongIdToStringId(long id)
        {
            return "0x" + (id).ToString("X16");
        }

        private void ProcessQuestsConnections(List<ObjectProxy> rows)
        {
            foreach (var q in articyQuests)
            {
                //находим первые связи прикрепленные к квесту, идем по ним вниз
                var startPoint = rows.Where(x => x[ObjectPropertyNames.Source] != null
                    && (long)((x[ObjectPropertyNames.Source] as ObjectProxy).Id) == q.LongId).ToList(); ;
                var test = rows.FirstOrDefault(x => (string)(x[ObjectPropertyNames.Target] as ObjectProxy)[ObjectPropertyNames.TemplateName] == "StandartQuest");

                if (startPoint != null)
                {
                    foreach (var c in startPoint)
                    {
                        //тут рекурсивный поиск в глубину с костылем в виде доплиста для развлетвлений
                        var list = rows.ToArray().ToList();
                        var additionalList = new List<ObjectProxy>(256);
                        var nextQuest = CalculateTargetQuests(c, list, additionalList);

                        if (nextQuest != null || additionalList.Count > 0)
                        {
                            if (nextQuest != null)
                                q.NextQuests.Add(ConvertLongIdToStringId((long)nextQuest.Id));

                            foreach (var aq in additionalList)
                            {
                                if (aq != null)
                                    q.NextQuests.Add(ConvertLongIdToStringId((long)aq.Id));
                            }
                        }
                    }

                }
            }

            //тут проходимся и заполняем входящие квесты
            foreach (var q in articyQuests)
            {
                foreach (var nq in q.NextQuests)
                {
                    var neededQuest = articyQuests.FirstOrDefault(x => x.Id == nq);
                    neededQuest.PreviousQuests.Add(q.Id);
                }
            }

            int tttt = 0;
        }

        private ObjectProxy CalculateTargetQuests(ObjectProxy startPoint, List<ObjectProxy> rows, List<ObjectProxy> additionalList)
        {
            if (startPoint == null || startPoint[ObjectPropertyNames.Target] == null || rows == null || rows.Count == 0)
                return default;

            if ((string)(startPoint[ObjectPropertyNames.Target] as ObjectProxy)[ObjectPropertyNames.TemplateName] == "StandartQuest")
            {
                return (startPoint[ObjectPropertyNames.Target] as ObjectProxy);
            }
            else
            {
                var currentTarget = (startPoint[ObjectPropertyNames.Target] as ObjectProxy).Id;
                var connection = rows.Where(x => (x[ObjectPropertyNames.Source] as ObjectProxy).Id == currentTarget).ToArray();

                if (connection.Length == 0)
                    return default;

                if (connection.Length == 1)
                {
                    rows.Remove(connection[0]);
                    return CalculateTargetQuests(connection[0], rows, additionalList);
                }
                else 
                {
                    foreach (var c in connection)
                        rows.Remove(c);
                    foreach (var c in connection)
                        additionalList.Add(CalculateTargetQuests(c, rows, additionalList));
                }
            }

            return default;
        }

        public void ProcessBubbleTexts(ObjectProxy bd)
        {
            if (bd.HasProperty(ObjectPropertyNames.Text))
            {
                if (bd.GetText() == string.Empty)
                    return;
            }

            var bubbleText = new ArticyBubbleText();
            bubbleText.ExternalEntityId = (bd[ObjectPropertyNames.Speaker] as ObjectProxy).GetExternalId();
            bubbleText.Id = "0x" + ((long)bd.Id).ToString("X16");
            bubbleText.Text = StringFixed(bd.GetText());
            bubbleText.ArticyAnimation = GetArticyAnimation(bd);
            bubbleText.WaitBeforeStartBubble = (float)(double)bd["BubbleTextController.WaitBeforeStartBubble"];
            bubbleText.BubbleViewTime = (float)(double)bd["BubbleTextController.BubbleViewTime"];
            articyBubbleTexts.Add(bubbleText);
        }

        public void ProcessPlayerChoices(ObjectProxy pc)
        {
            var answer = new ArticyAnswer();

            answer.DisplayId = StringFixed(pc.GetDisplayName());
            answer.Id = "0x" + ((long)pc.Id).ToString("X16");
            answer.Text = StringFixed((string)pc[ObjectPropertyNames.Text]);

            articyAnswers.Add(answer);
        }

        private ArticyAnimation3dControl GetArticyAnimation(ObjectProxy objectProxy)
        {
            if (!objectProxy.HasProperty("AnimationController3d.SettedBools"))
                return default;

            var animation = new ArticyAnimation3dControl();

            var usedAnimation = objectProxy["AnimationController3d.UseAnimation"];
            var settedBools = (objectProxy["AnimationController3d.SettedBools"] as List<ObjectProxy>);

            var waitForStart = objectProxy["AnimationController3d.WaitBeforeStart"];
            var boolTimeOut = objectProxy["AnimationController3d.BoolTimeOut"];

            var intParamName = objectProxy["AnimationController3d.IntParamName"];
            var intParamValue = objectProxy["AnimationController3d.IntParamValue"];

            var floatParamName = objectProxy["AnimationController3d.FloatParamName"];
            var floatParamValue = objectProxy["AnimationController3d.FloatParamValue"];

            if (usedAnimation != null)
            {
                animation.UsedAnimation = (usedAnimation as ObjectProxy).GetDisplayName();
            }

            if (settedBools != null && settedBools.Count > 0)
            {
                foreach (var sb in settedBools)
                    animation.SettedBools.Add(sb.GetDisplayName());
            }

            animation.WaitForStart = (float)(double)waitForStart;
            animation.BoolTimeOut = (float)(double)boolTimeOut;

            if (intParamName != null)
            {
                animation.IntParamName = GetEnumNameFromProperty("AnimationController3d.IntParamName", objectProxy);
                animation.IntParamValue = intParamValue != null ? (int)intParamValue : 0;
            }

            if (floatParamName != null)
            {
                animation.FloatParamName = GetEnumNameFromProperty("AnimationController3d.FloatParamName", objectProxy);
                animation.FloatParamValue = floatParamValue != null ? (float)(double)floatParamValue : 0;
            }

            return animation;
        }

        private string GetEnumNameFromProperty(string propertyName, ObjectProxy objectProxy)
        {
            var getFullProperty = apiSession.GetFeaturePropertyInfo(propertyName);
            var paramNameValue = objectProxy[propertyName];

            if (getFullProperty != null && getFullProperty.EnumValues != null)
            {
                foreach (var kp in getFullProperty.EnumValues)
                    if (kp.Key == (int)paramNameValue)
                        return kp.Value;
            }

            return string.Empty;
        }

        internal void FillEmotionsInConversations()
        {
            foreach (var c in conversations)
            {
                foreach (var d in c.Dialogs)
                {
                    if (c.DialogsEmotions.Contains(d.Emotion))
                        continue;

                    c.DialogsEmotions.Add(d.Emotion);
                }
            }
        }

        private void ProccessDialogueFragment(ObjectProxy objectProxy, ArticyConversation conversation)
        {
            var dialogue = new ArticyDialogStep();
            conversation.Dialogs.Add(dialogue);

            var displayName = StringFixed(objectProxy.GetDisplayName());
            var templateName = objectProxy.GetTemplateTechnicalName();

            var objectId = objectProxy.Id;

            //здесь получаем данные касательно персонажа который говорит
            if (objectProxy.HasProperty(ValuesHelper.Speaker))
            {
                var speaker = objectProxy[ValuesHelper.Speaker];

                if (speaker != null)
                {
                    dialogue.Emotion.EmotionName = ((speaker as ObjectProxy)[ValuesHelper.PreviewImageAsset] as ObjectProxy).GetDisplayName();
                    dialogue.Emotion.EmotionId = (long)((speaker as ObjectProxy)[ValuesHelper.PreviewImageAsset] as ObjectProxy).Id;
                    dialogue.Emotion.EmotionFileName = (string)((speaker as ObjectProxy)[ValuesHelper.PreviewImageAsset] as ObjectProxy)["Filename"];
                }
            }

            dialogue.ArticyAnimation = GetArticyAnimation(objectProxy);

            //тут обрабатываем 2д баблы с эмоджи для персонажей диалога
            if (objectProxy.HasProperty(ValuesHelper.ComicsEffect))
            {
                var comicsEffect = objectProxy[ValuesHelper.ComicsEffect];

                if (comicsEffect != null)
                {
                    dialogue.ArticyComicsEffect = ArticyComicsEffect(comicsEffect as ObjectProxy);
                    //таймаут для комикс эффекта берем рядом в шаблоне
                    dialogue.ArticyComicsEffect.TimeOutComicsEffect = (float)(double)objectProxy[ValuesHelper.TimeoutComixEffect];
                }
            }

            //тут обрабатываем 2д баблы с эмоджи для 3д персонажей 
            if (objectProxy.HasProperty(ValuesHelper.BubblePicture))
            {
                var bubblePicture = objectProxy[ValuesHelper.BubblePicture];

                if (bubblePicture != null)
                    dialogue.BubblePicture = ArticyComicsEffect(bubblePicture as ObjectProxy);
            }

            //здесь берем тексты для реплики, пока заложил локализацию тоже
            if (objectProxy.HasProperty("Text"))
            {
                dialogue.Text = StringFixed((string)objectProxy["Text"]);
            }

            if (objectProxy.HasProperty("ReplySettings.CharacterOrientSetting"))
            {
                dialogue.Orientation = (int)objectProxy["ReplySettings.CharacterOrientSetting"];
            }

            dialogue.DisplayId = displayName;
            dialogue.Id = (long)objectId;

            switch (templateName)
            {
                case "ReplyText":
                    dialogue.DialogStepType = DialogStepType.ReplyText;
                    break;
                case "AdditionalEmotion":
                    dialogue.DialogStepType = DialogStepType.AdditionalEmotion;
                    break;
                case "BubbleText":
                    dialogue.DialogStepType = DialogStepType.AdditionalEmotion;
                    break;
            }
        }

        private string StringFixed(string id) => id.Replace("\"", "'");


        private ArticyComicsEffect ArticyComicsEffect(ObjectProxy comicsEffect)
        {
            var articyEffect = new ArticyComicsEffect();

            articyEffect.DisplayId = StringFixed(comicsEffect.GetDisplayName());
            articyEffect.Id = (long)comicsEffect.Id;
            articyEffect.FileName = (string)comicsEffect[ObjectPropertyNames.Filename];
            articyEffect.ExternalId = (string)comicsEffect[ObjectPropertyNames.ExternalId];

            if ((string)(comicsEffect as ObjectProxy)[ObjectPropertyNames.TemplateName] == ValuesHelper.DreamBubble2d)
            {
                articyEffect.EffectType = ArticyComicsEffectType.DreamBubble2d;
            }

            if ((string)(comicsEffect as ObjectProxy)[ObjectPropertyNames.TemplateName] == ValuesHelper.Emotion2D)
            {
                articyEffect.EffectType = ArticyComicsEffectType.Emotion2d;
            }

            if ((string)(comicsEffect as ObjectProxy)[ObjectPropertyNames.TemplateName] == "BubblePicture")
            {
                articyEffect.EffectType = ArticyComicsEffectType.Emoji3d;
            }

            return articyEffect;
        }

        internal void ProcessEntities(ObjectProxy r)
        {
            var entity = new ArticyEntity();
            entity.EntityId = (long)r.Id;
            entity.DisplayId = r.GetDisplayName();
            entity.ExternalId = r.GetExternalId();

            //собираем все эмоции которые лежат в общей папке с главной ентити
            var emotions = apiSession.RunQuery($"SELECT * FROM Entities WHERE IsDescendantOf({r.GetParent().Id}) AND TemplateName == 'EmotionCharacters' ");

            foreach (var e in emotions.Rows)
            {
                var emotion = new ArticyEmotion();

                emotion.EmotionName = (e[ObjectPropertyNames.PreviewImageAsset] as ObjectProxy).GetDisplayName();
                emotion.EmotionId = (long)(e[ObjectPropertyNames.PreviewImageAsset] as ObjectProxy).Id;
                emotion.EmotionFileName = (string)(e[ObjectPropertyNames.PreviewImageAsset] as ObjectProxy)[ObjectPropertyNames.Filename];
                emotion.ParentEntityId = entity.EntityId;
                emotion.ParentEntityExternalId = entity.ExternalId;
                emotion.ParentEntityDisplayId = entity.DisplayId;
                entity.Emotions.Add(emotion);
            }

            var emotionsInDialogues = conversations.SelectMany(x => x.Dialogs).Select(z => z.Emotion);
            var neededEmotions = emotionsInDialogues.Where(x => entity.Emotions.Any(z => z.EmotionId == x.EmotionId));
            foreach (var ne in neededEmotions)
            {
                ne.ParentEntityId = entity.EntityId;
                ne.ParentEntityDisplayId = entity.DisplayId;
                ne.ParentEntityExternalId = entity.ExternalId;
            }
            entities.Add(entity);
        }
    }
}