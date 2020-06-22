using System;
using System.Collections.Generic;

namespace MyCompany.TestArticy
{
    [Serializable]
    public class ArticyConversation
    {
        public long ConversationId;
        public List<ArticyEntity> DialogEntities = new List<ArticyEntity>(32);
        public List<ArticyDialogStep> Dialogs = new List<ArticyDialogStep>(32);
    }

    public class ArticyEntity
    {
        public string Id;
        public List<ArticyEmotion> Emotions = new List<ArticyEmotion>(8);
    }

    public class ArticyEmotion
    {
        public string ParentEntity;
        public long EmotionId;
        public string EmotionName;
        public string EmotionFileName;
    }

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
        public List<ArticyDialogStep> NextSteps = new List<ArticyDialogStep>(4);

        public bool IsActive => DialogStepType == DialogStepType.ReplyText;
    }

    public enum DialogStepType
    {
        Default = 0,
        ReplyText = 1,
        AdditionalEmotion = 2,
        BubbleText = 3,
    }
}