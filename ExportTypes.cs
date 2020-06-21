using System.Collections.Generic;

namespace MyCompany.TestArticy
{
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
        public string AnimationId;
    }

    public class DialogStep
    {
        public string DisplayId;
        public int Id;
        public int Orientation;

        public DialogStepType DialogStepType;
        public ArticyEntity CurrentEntity;
        public List<DialogStep> NextSteps;
        public bool IsActive => DialogStepType == DialogStepType.ReplyText;
    }

    public enum DialogStepType
    {
        Default = 0,
        ReplyText = 1,
        AdditionalTemplate = 2,
    }
}