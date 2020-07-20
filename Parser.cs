using Articy.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCompany.TestArticy
{
    public class Parser
    {
        private List<ArticyConversation> conversations = new List<ArticyConversation>(64);
        private List<ArticyEntity> entities = new List<ArticyEntity>(64);
        private List<ArticyAnswer> articyAnswers = new List<ArticyAnswer>(64);
        private ApiSession apiSession;

        public ArticyConversation[] Conversations => conversations.ToArray();
        public ArticyEntity[] Entities => entities.ToArray();
        public ArticyAnswer[] ArticyAnswers => articyAnswers.ToArray();

        public Parser(ApiSession apiSession)
        {
            this.apiSession = apiSession;

        }

        public void ProcessDialogues(ObjectProxy child)
        {
            var conversation = new ArticyConversation();

            conversation.ConversationId = "0x"+((long)child.Id).ToString("X16");
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

        public void ProcessPlayerChoices(ObjectProxy pc)
        {
            var answer = new ArticyAnswer();

            answer.DisplayId = pc.GetDisplayName();
            answer.Id = (long)pc.Id;
            answer.Text = (string)pc[ObjectPropertyNames.Text];

            articyAnswers.Add(answer);
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

            if (objectProxy.HasProperty(ValuesHelper.ComicsEffect))
            {
                var comicsEffect = objectProxy[ValuesHelper.ComicsEffect];

                if (comicsEffect != null)
                {
                    var objectProxyTemp = (comicsEffect as ObjectProxy);

                    dialogue.ArticyComicsEffect = new ArticyComicsEffect();
                    dialogue.ArticyComicsEffect.DisplayId = StringFixed(objectProxyTemp.GetDisplayName());
                    dialogue.ArticyComicsEffect.Id = (long)objectProxyTemp.Id;
                    dialogue.ArticyComicsEffect.FileName = (string)objectProxyTemp[ObjectPropertyNames.Filename];

                    if ((string)objectProxy[ObjectPropertyNames.TemplateName] == ValuesHelper.BubblePicture2d)
                    {
                        dialogue.ArticyComicsEffect.EffectType = ArticyComicsEffectType.DreamBubble;
                    }   
                    
                    if ((string)objectProxy[ObjectPropertyNames.TemplateName] == ValuesHelper.Emotion2D)
                    {
                        dialogue.ArticyComicsEffect.EffectType = ArticyComicsEffectType.Emotion2d;
                    }
                }
            }

            if (objectProxy.HasProperty(ValuesHelper.BubblePicture))
            {
                var bubblePicture = objectProxy[ValuesHelper.BubblePicture];

                if (bubblePicture != null)
                {
                    dialogue.BubblePicture = new ArticyComicsEffect();
                    dialogue.BubblePicture.DisplayId = StringFixed((bubblePicture as ObjectProxy).GetDisplayName());
                    dialogue.BubblePicture.Id = (long)(bubblePicture as ObjectProxy).Id;
                    dialogue.BubblePicture.FileName = (string)(bubblePicture as ObjectProxy)[ObjectPropertyNames.Filename];
                }
            }

            //настройка анимации
            if (objectProxy.HasProperty("AnimationController3d.SettedBools"))
            {
                var usedAnimation = objectProxy["AnimationController3d.UseAnimation"];
                var settedBools = (objectProxy["AnimationController3d.SettedBools"] as List<ObjectProxy>);

                var waitForStart = objectProxy["AnimationController3d.WaitBeforeStart"];
                var boolTimeOut = objectProxy["AnimationController3d.BoolTimeOut"];

                var intParamName = objectProxy["AnimationController3d.IntParamName"];
                var intParamValue = objectProxy["AnimationController3d.IntParamValue"];

                var floatParamName = objectProxy["AnimationController3d.FloatParamName"];
                var floatParamValue = objectProxy["AnimationController3d.FloatParamValue"];

                var animation = dialogue.ArticyAnimation;

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
                    animation.IntParamValue = intParamValue != null ?(int)intParamValue :0;
                }

                if (floatParamName != null)
                {
                    animation.FloatParamName = GetEnumNameFromProperty("AnimationController3d.FloatParamName", objectProxy);
                    animation.FloatParamValue = floatParamValue != null ? (float)(double)floatParamValue : 0;
                }
            }

            //здесь берем тексты для реплики, пока заложил локализацию тоже
            if (objectProxy.HasProperty("Text"))
            {
                dialogue.Text = (string)objectProxy["Text"];
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

        private string StringFixed(string id) => id.Replace("\"", " ");

        internal void ProcessEntities(ObjectProxy r)
        {
            var entity = new ArticyEntity();
            entity.EntityId = (long)r.Id;
            entity.DisplayId = r.GetDisplayName();

            //собираем все эмоции которые лежат в общей папке с главной ентити
            var emotions = apiSession.RunQuery($"SELECT * FROM Entities WHERE IsDescendantOf({r.GetParent().Id}) AND TemplateName == 'EmotionCharacters' ");

            foreach (var e in emotions.Rows)
            {
                var emotion = new ArticyEmotion();

                emotion.EmotionName = (e[ObjectPropertyNames.PreviewImageAsset] as ObjectProxy).GetDisplayName();
                emotion.EmotionId = (long)(e[ObjectPropertyNames.PreviewImageAsset] as ObjectProxy).Id;
                emotion.EmotionFileName = (string)(e[ObjectPropertyNames.PreviewImageAsset] as ObjectProxy)[ObjectPropertyNames.Filename];
                emotion.ParentEntityId = entity.EntityId;
                emotion.ParentEntityDisplayId = entity.DisplayId;
                entity.Emotions.Add(emotion);
            }

            var emotionsInDialogues = conversations.SelectMany(x => x.Dialogs).Select(z => z.Emotion);
            var neededEmotions = emotionsInDialogues.Where(x => entity.Emotions.Any(z => z.EmotionId == x.EmotionId));
            foreach (var ne in neededEmotions)
            {
                ne.ParentEntityId = entity.EntityId;
                ne.ParentEntityDisplayId = entity.DisplayId;
            }
            entities.Add(entity);
        }
    }
}