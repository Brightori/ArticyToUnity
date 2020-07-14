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
        public List<long> StartDialogs = new List<long>();
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
    public class ArticyAnimation3dControl
    {
        public string UsedAnimation = string.Empty;
        public List<string> SettedBools = new List<string>(4);

        public float WaitForStart = 0.0f;
        public float BoolTimeOut = 5.0f;

        public string IntParamName = string.Empty;
        public int IntParamValue = 0;

        public string FloatParamName = string.Empty;
        public float FloatParamValue = 0.0f;
    }

    [Serializable]
    public class ArticyDialogStep
    {
        public string DisplayId;
        public long Id;

        public string Text;
        public int Orientation;

        // эмоции это всё что связано с отображением UI 2д персонажа в текущий диалоговый шаг, тобишь просто картинка текущего персонажа
        // они из артиси переносятся в юнити c именем файла состоящим из имени главперсонажа + уникальный айди эмоции. 
        //там есть доп. данные чтобы потом можно было быстро отдебажить (по отображаемому имени в артиси + оригинальному названию граф файла)
        public ArticyEmotion Emotion = new ArticyEmotion();
        public ArticyAnimation3dControl ArticyAnimation = new ArticyAnimation3dControl();

        public ArticyComicsEffect ArticyComicsEffect;

        public DialogStepType DialogStepType;
        public List<ArticyAnswer> Answers = new List<ArticyAnswer>(3);
        public List<long> NextStepsIds = new List<long>(4);

        public bool IsActive => DialogStepType == DialogStepType.ReplyText;
    }

    [Serializable]
    public class ArticyComicsEffect
    {
        public long Id;
        public string DisplayId;
        public string FileName;
    }

    [Serializable]
    public class ArticyAnswer
    {
        public string DisplayId;
        public long Id;

        public string Text;
        public long NextDialogueStepId;
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
        public const string ComicsEffect = "ReplySettings.ComicsEffect";
    }
}