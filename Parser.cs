using Articy.Api;
using System.Collections.Generic;

namespace MyCompany.TestArticy
{
    public class Parser
    {
        private List<Conversation> conversations = new List<Conversation>(64);

        public void ProcessDialogues(ObjectProxy child)
        {
            var conversation = new Conversation();
            conversations.Add(conversation);

            var childs = child.GetChildren();

            foreach (var c in childs)
                GetDataForConversation(c, conversation);
        }

        private void GetDataForConversation(ObjectProxy objectProxy, Conversation conversation)
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

        private void ProccessDialogueFragment(ObjectProxy objectProxy, Conversation conversation)
        {
            var dialogue = new DialogStep();
            conversation.Dialogs.Add(dialogue);

            var displayName = objectProxy.GetDisplayName();
            var templateName = objectProxy.GetTemplateTechnicalName();
            var techName = objectProxy.GetTechnicalName();

            var objectId = objectProxy.Id;
            //var speaker = objectProxy["Speaker"];
            
            if (objectProxy.HasProperty("Speaker"))
            {
                var sp1 = objectProxy["Speaker"];
                dialogue.Emotion.EmotionId = ((sp1 as ObjectProxy)["PreviewImageAsset"] as ObjectProxy).GetDisplayName();
            }

            if (objectProxy.HasProperty("Text"))
            {
                dialogue.Text = (string)objectProxy["Text"];
            }

            if (objectProxy.HasProperty("ReplySettings.CharacterOrientSetting"))
            {
                dialogue.Orientation = (int)objectProxy["ReplySettings.CharacterOrientSetting"];
            }

            dialogue.DisplayId = displayName;
            dialogue.TechId = techName;
            dialogue.Id = (long)objectId;

            switch (templateName)
            {
                case "ReplyText":
                    dialogue.DialogStepType = DialogStepType.ReplyText;
                    break;
                case "AdditionalEmotion":
                    dialogue.DialogStepType = DialogStepType.AdditionalEmotion;
                    break;
            }
        }
    }
}
