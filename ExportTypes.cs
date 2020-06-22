using System;
using System.Collections.Generic;

namespace MyCompany.TestArticy
{
    [Serializable]
    public class Conversation
    {
        public List<ArticyEntity> DialogEntities = new List<ArticyEntity>(32);
        public List<DialogStep> Dialogs = new List<DialogStep>(32);
    }

    public class ArticyEntity
    {
        public string Id;
        public List<Emotion> Emotions = new List<Emotion>(8);
    }

    public class Emotion
    {
        public string ParentEntity;
        public string EmotionId;
    }

    [Serializable]
    public class DialogStep
    {
        public string DisplayId;
        public string TechId;
        public long Id;

        public string Text;
        public int Orientation;

        public Emotion Emotion = new Emotion();
        
        public DialogStepType DialogStepType;
        public List<DialogStep> NextSteps = new List<DialogStep>(4);

        public bool IsActive => DialogStepType == DialogStepType.ReplyText;
    }

    public enum DialogStepType
    {
        Default = 0,
        ReplyText = 1,
        AdditionalEmotion = 2,
    }
}