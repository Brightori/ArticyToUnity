using Articy.Api;
using System.Collections.Generic;

namespace MyCompany.TestArticy
{
    public class Parser
    {
        private List<ArticyConversation> conversations = new List<ArticyConversation>(64);

        public void ProcessDialogues(ObjectProxy child)
        {
            var conversation = new ArticyConversation();
            conversations.Add(conversation);

            var childs = child.GetChildren();

            foreach (var c in childs)
                GetDataForConversation(c, conversation);
        }

        private void GetDataForConversation(ObjectProxy objectProxy, ArticyConversation conversation)
        {
            switch (objectProxy.ObjectType)
            {
                case ObjectType.Connection:
                    break;

                case ObjectType.DialogueFragment:
                    ProccessDialogueFragment(objectProxy, conversation);
                    break;
            }
        }

        private void ProccessDialogueFragment(ObjectProxy objectProxy, ArticyConversation conversation)
        {
            var dialogue = new ArticyDialogStep();
            conversation.Dialogs.Add(dialogue);

            var displayName = objectProxy.GetDisplayName();
            var templateName = objectProxy.GetTemplateTechnicalName();

            var objectId = objectProxy.Id;
            
            //здесь получаем данные касательно персонажа который говорит
            if (objectProxy.HasProperty("Speaker"))
            {
                var sp1 = objectProxy["Speaker"];
                dialogue.Emotion.EmotionName = ((sp1 as ObjectProxy)["PreviewImageAsset"] as ObjectProxy).GetDisplayName();
                dialogue.Emotion.EmotionId = (long)((sp1 as ObjectProxy)["PreviewImageAsset"] as ObjectProxy).Id;
                dialogue.Emotion.EmotionFileName = (string)((sp1 as ObjectProxy)["PreviewImageAsset"] as ObjectProxy)["Filename"];
            }

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
            }

            //здесь берем тексты для реплики, пока заложил локализацию тоже
            if (objectProxy.HasProperty("Text"))
            {
                dialogue.Text = (string)objectProxy["Text"];
            }

            //настройка анимации


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
    }
}
