using System;
using System.Collections.Generic;

namespace MyCompany.TestArticy
{
    [Serializable]
    public class ArticyConversationsData
    {
        public ArticyConversation[] Conversations;

        public ArticyConversationsData(ArticyConversation[] conversations)
        {
            Conversations = conversations;
        }
    }    
    
    [Serializable]
    public class ArticyEntitiesData
    {
        public ArticyEntity[] Entities;

        public ArticyEntitiesData(ArticyEntity[] entities)
        {
            Entities = entities;
        }
    }

    [Serializable]
    public class ArticyConversation
    {
        public long ConversationId;
        public List<ArticyEmotion> DialogsEmotions = new List<ArticyEmotion>(32);
        public List<ArticyDialogStep> Dialogs = new List<ArticyDialogStep>(32);
    }

    [Serializable]
    public class ArticyEntity
    {
        public long EntityId;
        public string DisplayId;
        public List<ArticyEmotion> Emotions = new List<ArticyEmotion>(8);
    }

    [Serializable]
    public class ArticyEmotion
    {
        public long ParentEntityId;
        public string ParentEntityDisplayId;
        public long EmotionId;
        public string EmotionName;
        public string EmotionFileName;

        public override bool Equals(object obj)
        {
            if (obj is ArticyEmotion emotion)
            {
                return emotion.EmotionId == EmotionId;
            }

            return false;
        }
    }

    [Serializable]
    public class ArticyAnimation
    {
        public string UsedAnimation = string.Empty;
        public List<string> SettedBools = new List<string>(4);

        public float WaitForStart = 0;
        public float BoolTimeOut = 5;

        public string IntParamName = string.Empty;
        public int IntParamValue = 0;

        public string FloatParamName = string.Empty;
        public float FloatParamValue = 0;
    }

    [Serializable]
    public class ArticyDialogStep
    {
        public string DisplayId;
        public long Id;

        public string Text;
        public int Orientation;

        public ArticyEmotion Emotion = new ArticyEmotion();
        public ArticyAnimation ArticyAnimation = new ArticyAnimation();

        public DialogStepType DialogStepType;
        public List<long> NextStepsIds = new List<long>(4);

        public bool IsActive => DialogStepType == DialogStepType.ReplyText;
    }

    public enum DialogStepType
    {
        Default = 0,
        ReplyText = 1,
        AdditionalEmotion = 2,
        BubbleText = 3,
    }

    public class ValuesHelper
    {
        public const string Parent = "Parent";
        public const string Target = "Target";
        public const string PreviewImageAsset = "PreviewImageAsset";
        public const string Speaker = "Speaker";
    }
}