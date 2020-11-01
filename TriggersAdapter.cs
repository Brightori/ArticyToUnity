using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System;
using System.Collections.Generic;

namespace MyCompany.TestArticy
{
    [Serializable]
    public class TriggersAdapter
    {
        public string type = "concrete-grade";
        public bool acceptable = true;
        public bool essential;

        [Newtonsoft.Json.JsonProperty("complete-price")]
        public SimpleResourceReward completePrice { get; set; }
        public Data reward;

        [Newtonsoft.Json.JsonProperty("accepted-reward")]
        public Data acceptedReward { get; set; }

        public TriggersAdapter(ArticyQuest quest)
        {
            essential = quest.Essential;
            completePrice = new SimpleResourceReward(quest.Cost);
            reward = new CompositeReward(quest.Rewards);
            acceptedReward = new CompositeReward(quest.AcceptedRewards);
        }
    }

    public abstract class Data
    {
        public abstract string type { get; set; }
    }

    public class NextQuest : Data
    {
        public override string type { get; set; } = "enable-trigger";
        public string path = "triggers.";

        public NextQuest(string nextQuestId)
        {
            if (nextQuestId != string.Empty)
                path += nextQuestId;
        }
    }

    public class QuestCounter : Data
    {
        override public string type { get; set; } = "simple-resource";
        public string path = "simple-resources.completed-quests";
        public AmountSimpleType amount;

        public QuestCounter(IMoney money)
        {
            amount = new AmountSimpleType(money.Amount);
        }
    }

    public class AmountSimpleType
    {
        public string type = "simple";
        public int amount = 1;

        public AmountSimpleType(int amount)
        {
            this.amount = amount;
        }
    }

    public class CompositeReward : Data
    {
        override public string type { get; set; } = "composite";
        public List<Data> rewards = new List<Data>();

        public CompositeReward()
        {
        }

        public CompositeReward(List<IReward> rewards)
        {
            if (rewards != null)
            {
                foreach (var r in rewards)
                    this.rewards.Add(RewardFactory.GetReward(r));
            }
        }
    }

    public class GiftReward : Data
    {
        public override string type { get; set; } = "deal";
        public string path = "deals.";

        public GiftReward(IReward giftId)
        {
            path += giftId.Path;
        }
    }

    public class SimpleResourceReward : Data
    {
        public string path = "simple-resources.";
        public int amount = 0;

        public SimpleResourceReward(IMoney valuable)
        {
            if (valuable != null)
            {
                path += valuable.Path;
                amount = valuable.Amount;
            }
            else
                path += "premium";
        }

        public override string type { get; set; } = "simple-resource";
    }

    public static class RewardFactory
    {
        public static Data GetReward(IReward reward)
        {
            switch (reward)
            {
                case ArticyGiftReward gift:
                    return new GiftReward(gift);

                case ArticyQuestTriggerEnableReward triggerReward:
                    return new NextQuest(triggerReward.Path);

                case ArticyQuestTriggerReward questCounter:
                    return new QuestCounter(questCounter);

                case IMoney money:
                    return new SimpleResourceReward(money);
                default:
                    return null;
            }
        }
    }
}