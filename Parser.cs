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
            var getProperties = objectProxy.GetAvailableProperties();

            var dialogue = new DialogStep();
            conversation.Dialogs.Add(dialogue);

            var displayName = objectProxy.GetDisplayName();
            var templateName = objectProxy.GetTemplateTechnicalName();
            var shortId = objectProxy.GetShortId();

            dialogue.DisplayId = displayName;
            dialogue.Id = shortId;

            switch (templateName)
            {
                case "ReplyText":
                    dialogue.DialogStepType = DialogStepType.ReplyText;
                    break;
            }
        }
    }
}
