using System;
using System.Collections.Generic;

public interface IMoney : IReward
{
    int Amount { get; }
}

public interface IReward
{
    string Type { get; }
    string Path { get; }
}

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
    public string ConversationId;
    public bool HideComixAfterComplete = true;
    public List<long> StartDialogs = new List<long>();
    public List<ArticyEmotion> DialogsEmotions = new List<ArticyEmotion>(32);
    public List<ArticyDialogStep> Dialogs = new List<ArticyDialogStep>(32);
}

[Serializable]
public class ArticyEntity
{
    public long EntityId;
    public string DisplayId;
    public string ExternalId;
    public List<ArticyEmotion> Emotions = new List<ArticyEmotion>(8);
}

[Serializable]
public class ArticyTextureOffset
{
    public string Id;
    public int[] DreamBubbleOffset = new int[2];
    public int[] AccentBubbleOffset = new int[2];
}


[Serializable]
public class ArticyEmotion
{
    public long ParentEntityId;
    public string ParentEntityExternalId;
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

    //сюда кладётся информация для квест аноунсера
    public ArticyAnnouncer ArticyQuestLink;

    // эмоции это всё что связано с отображением UI 2д персонажа в текущий диалоговый шаг, тобишь просто картинка текущего персонажа
    // они из артиси переносятся в юнити c именем файла состоящим из имени главперсонажа + уникальный айди эмоции. 
    //там есть доп. данные чтобы потом можно было быстро отдебажить (по отображаемому имени в артиси + оригинальному названию граф файла)
    public ArticyEmotion Emotion = new ArticyEmotion();

    //здесь всё что связано с 3д, за исключение баблов
    public ArticyAnimation3dControl ArticyAnimation = new ArticyAnimation3dControl();

    //сюда мы берем данные с поля ComicsEffect  для 2д диалогов
    public ArticyComicsEffect ArticyComicsEffect;

    //сюда мы берем данные с поля BubblePicture это для 3д персонажей
    public ArticyComicsEffect BubblePicture;

    //это тип диалог шага, тут мы узнаем кто говорит, кто на вторых ролях и тд
    public DialogStepType DialogStepType;

    //сюда складываем айдишники следущих шагов диалога
    public List<long> NextStepsIds = new List<long>(4);
    public bool IsActive => DialogStepType == DialogStepType.ReplyText;
}

[Serializable]
public class ArticyComicsEffect
{
    public long Id;
    public string ExternalId;
    public string DisplayId;
    public string FileName;
    public float TimeOutComicsEffect;
    public ArticyComicsEffectType EffectType;
}

[Serializable]
public class ArticyAnswer
{
    public string DisplayId;
    public string Id;

    public string Text;
    public long NextDialogueStepId;
}

[Serializable]
public class ArticyBubbleText
{
    public string ExternalEntityId;
    public string Id;
    public string Text;
    public float WaitBeforeStartBubble;
    public float BubbleViewTime;
    public ArticyAnimation3dControl ArticyAnimation = new ArticyAnimation3dControl();
}


[Serializable]
public class ArticyText
{
    public string Id;
    public string Text;
}

[Serializable]
public class SimpleReward : IMoney
{
    public int Amount { get; set; }

    public string Type { get; set; }

    public string Path { get; set; }
}

[Serializable]
public class ArticyGiftReward : IReward
{
    public string Path { get; set; }

    public string Type { get; set; } = "deal";
}

[Serializable]
public class ArticyQuestTriggerReward : IMoney
{
    public string Path { get; set; }

    public string Type { get; set; }

    public int Amount { get; set; }
}

[Serializable]
public class ArticyQuestTriggerEnableReward : IReward
{
    public string Path { get; set; }

    public string Type { get; set; }

}

[Serializable]
public class ArticyQuestLink
{
    public string Id;
    public long LongId;
}

[Serializable]
public class ArticyQuest
{
    public string Id;
    public long LongId;
    public string ViewId;
    public string DisplayId;
    public string IconFileName;
    public bool AlwaysShowActivatedComix;
    public bool OpenQuestList;
    public bool Essential;

    public IMoney Cost;
    public List<IReward> Rewards = new List<IReward>(4);
    public List<IReward> AcceptedRewards = new List<IReward>(4);

    public List<ArticyQuestLink> PreviousQuests = new List<ArticyQuestLink>(4);
    public List<ArticyQuestLink> NextQuests = new List<ArticyQuestLink>(4);
}

// это просто общая инфа для аноунсера, берется из прокинутого в диалог квеста
public class ArticyAnnouncer
{
    public string[] QuestIds;
    public float Duration;
    public float Delay;
}

[Serializable]
public class Сurrency : IMoney
{
    public int Amount { get; set; }

    public string Type { get; set; }

    public string Path { get; set; }
}


public enum ArticyComicsEffectType
{
    Default = 0,
    Emotion2d = 1,
    Emoji3d = 2,
    DreamBubble2d = 3,
    Anoncer2D = 4,
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
    public const string DreamBubble2d = "DreamBubble";
    public const string Emotion2D = "Emotion2D";
    public const string BubblePicture = "AnimationController3d.BubblePicture";
    public const string TimeoutComixEffect = "ReplySettings.TimeoutComixEffect";
    public const string QuestIcon = "QuestIcon";
    public const string BuildingToUpgrade = "RewardQuest.BuildingToUpgrade";
    public const string FileName = "FileName";
    public const string SimpleReward = "SimpleReward";
    public const string SimpleRewardResourceType = "SimpleReward.RewardType";
    public const string SimpleRewardAmount = "SimpleReward.Amount";
    public const string SimpleResource = "SimpleResource";
    public const string SimpleResourceResourceType = "SimpleResource.ResourceType";
    public const string SimpleResourceAmount = "SimpleResource.Amount";
    public const string NightSettings = "NightSettings";
    public const string NightSettingsDuration = "NightSettings.Duration";
    public const string NightSettingsDisableNightAfterCompletion = "NightSettings.DisableNightAfterCompletion";
    public const string NightSettingsAffectCharacters = "NightSettings.AffectCharacters";
    public const string NightSettingsAffectBuildings = "NightSettings.AffectBuildings";
    public const string NightSettingsToColor = "NightSettings.ToColor";
    public const string NightSettingsFromColor = "NightSettings.FromColor";
    public const string HideComixAfterComplete = "HideComixAfterComplete.HideComixAfterComplete";
    public const string QuestAnnouncer = "QuestAnonserActual.QuestId";
    public const string QuestAnnouncerId = "QuestAnonserActual.QuestId";
    public const string QuestAnnouncerDelay = "QuestAnonserActual.Delay";
    public const string QuestAnnouncerDuration = "QuestAnonserActual.Duration";
    public const string QuestAnnouncerUseAnnouncer = "QuestAnonserActual.UseAnonser";
    public const string DreamBubbleOffsetX = "DreamBubble_offset.X";
    public const string DreamBubbleOffsetY = "DreamBubble_offset.Y";
    public const string EmojiOffsetX = "EmojiOffset.X";
    public const string EmojiOffsetY = "EmojiOffset.Y";
    public const string ViewId = "QuestSettings.ActivateComix";
    public const string OpenQuestList = "QuestSettings.OpenQuestList";
    public const string AlwaysShowActivatedComix = "QuestSettings.AlwaysShowActivatedComix";
    public const string Essential = "QuestSettings.Essential";
    public const string GiftId = "Gift.id";
}