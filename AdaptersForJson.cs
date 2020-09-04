using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System;
using System.Collections.Generic;

/// <summary>
/// это класс для адаптирования в джейсон  - квестов и используемых квестом сущностей из артиси, мы остальные сущности выгружаем напрямую, 
/// а квесты перекладываем в уже существующую структуру, в большинстве случаев будут отличаться нейминг, 
/// но также при выгрузке в джейсон присутствует дополнительная вложенность, со стороны артиси данных поддерживаем всё интерфейсами,
/// тут же дописываем в фабрику что нужно. 
/// </summary>

namespace MyCompany.TestArticy
{
    [Serializable]
    public class QuestAdapter
    {
        public string displayId;
        public string type = "concrete-grade";
        public long longId;
        public bool acceptable = true;

        [Newtonsoft.Json.JsonProperty("complete-price")]
        public Money completePrice { get; set; }
        public Data reward;

        [Newtonsoft.Json.JsonProperty("accepted-reward")]
        public Data acceptedReward { get; set; }

        public QuestAdapter(ArticyQuest quest)
        {
            displayId = quest.DisplayId;
            longId = quest.LongId;
            completePrice = new Money(quest.Cost);
            reward = new CompositeReward(quest.Rewards);
            acceptedReward = new QuestReward(quest);
        }
    }

    public abstract class Data
    {
        public abstract string type { get; set; }
    }

    public class QuestReward : CompositeReward
    {
        public QuestReward(ArticyQuest articyQuest)
        {
            rewards.Add(new EnableQuest(articyQuest));
            rewards.Add(new QuestCounter());
        }
    }

    public class EnableQuest : CompositeReward
    {
        new public string type = "requirement";
        public CompositeReward reward = new CompositeReward();
        public RequirementComposite requirement = new RequirementComposite();

        public EnableQuest(ArticyQuest articyQuest)
        {
            foreach (var q in articyQuest.NextQuests)
                reward.rewards.Add(new NextQuest(q));

            foreach (var q in articyQuest.PreviousQuests)
                requirement.requirements.Add(new QuestRequirement(q));
        }
    }

    public class NextQuest : Data
    {
        public override string type { get; set; } = "enable-trigger";
        public string path = "triggers.";

        public NextQuest(string nextQuestId)
        {
            if (nextQuestId != string.Empty)
                path += nextQuestId;
            else
                path += "0";
        }
    }

    public abstract class Requirement : Data
    {
        public override string type { get; set; } = "requirement";
    }

    public class RequirementComposite : Data
    {
        public override string type { get; set; } = "and";
        public List<Requirement> requirements = new List<Requirement>(4);
    }

    public class QuestRequirement : Requirement
    {
        public override string type { get; set; } = "trigger-state";
        public string state = "Accepted";
        public string path = "triggers.";

        public QuestRequirement(string questId)
        {
            if (questId != string.Empty)
                path += questId;
            else
                path += "0";
        }
    }

    public class QuestCounter : Data
    {
        override public string type { get; set; } = "simple-resource";
        public string path = "simple-resources.completed-quests";
        public AmountSimpleType amount = new AmountSimpleType();
    }

    public class AmountSimpleType
    {
        public string type = "simple";
        public int amount = 1;
    }

    public class CompositeReward : Data
    {
        override public string type { get; set; } = "composite";
        public List<Data> rewards = new List<Data>(4);

        public CompositeReward()
        {
        }

        public CompositeReward(List<IReward> rewards)
        {
            foreach (var r in rewards)
                this.rewards.Add(RewardFactory.GetReward(r));
        }
    }

    public class GiftReward : CompositeReward
    {
        public override string type { get; set; } = "gift";
        
        public GiftReward()
        {
        }

        public GiftReward(List<IReward> rewards)
        {
            foreach (var r in rewards)
                this.rewards.Add(RewardFactory.GetReward(r));
        }
    }

    public class NightReward : Data
    {
        public bool night = false;
        public string startColor = "FFFFFF";
        public string color = "FFFFFF";
        public bool characters = true;
        public bool buildings = true;
        public float duration = 2;
        public override string type { get; set; }

        public NightReward(ArticyNightSettings articyNight)
        {
            type = articyNight.Type;
            color = articyNight.ToColor;
            startColor = articyNight.FromColor;
            night = articyNight.DisableAfterCompletion;
            characters = articyNight.AffectCharacters;
            buildings = articyNight.AffectBuildings;
            duration = articyNight.Duration;
        }
    }

    public class UpgradeBuildingReward : Data
    {
        override public string type { get; set; } = "upgrade-building";
        public string path;

        public UpgradeBuildingReward(UpgradeBuildingInfo upgradeBuildingInfo)
        {
            path = upgradeBuildingInfo.ExternalId;
        }
    }

    public class Money : Data
    {
        override public string type { get; set; } = "simple-resource";
        public string path = "simple-resources.";
        public int amount = 0;

        public Money(IMoney valuable)
        {
            if (valuable != null)
            {
                path += CurrencyTypeConverter(Convert.ToInt32(valuable.Type));
                amount = valuable.Amount;
            }
            else
                path += "premium";
        }

        public string CurrencyTypeConverter(int type)
        {
            switch (type)
            {
                case 0:
                    return "premium";
                default:
                    return "premium";
            }
        }
    }

    public static class RewardFactory
    {
        public static Data GetReward(IReward reward)
        {
            switch (reward)
            {
                case ArticyNightSettings articyNight:
                    return new NightReward(articyNight);
                case UpgradeBuildingInfo upgradeBuildingInfo:
                    return new UpgradeBuildingReward(upgradeBuildingInfo);
                case IMoney money:
                    return new Money(money);
                default:
                    return null;
            }
        }
    }
}