﻿using System;
using System.Collections.Generic;

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
    public string externalEntityId;
    public string Id;
    public string Text;
    public float WaitBeforeStartBubble;
    public float BubbleViewTime;
    public ArticyAnimation3dControl ArticyAnimation = new ArticyAnimation3dControl();
}

public enum ArticyComicsEffectType
{
    Default = 0,
    Emotion2d =1, 
    Emoji3d =2, 
    DreamBubble2d =3,
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
}