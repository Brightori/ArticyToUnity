using System;

namespace MyCompany.TestArticy
{
    [Serializable]
    class TriggersViewAdapter
    {
        [Newtonsoft.Json.JsonProperty("open-quest-list")]
        public bool OpenQuestList { get; }

        [Newtonsoft.Json.JsonProperty("image")]
        public string Image { get; set; }

        [Newtonsoft.Json.JsonProperty("title")]
        public string Title { get; }

        [Newtonsoft.Json.JsonProperty("activate-comix")]
        public string ActivateComix { get; }

        [Newtonsoft.Json.JsonProperty("always-show-activated-comix")]
        public bool AlwaysShowActivatedComix { get; }

        public TriggersViewAdapter(ArticyQuest quest)
        {
            ActivateComix = quest.ViewId;
            Title = quest.DisplayId;
            Image = quest.IconFileName;
            AlwaysShowActivatedComix = quest.AlwaysShowActivatedComix;
            OpenQuestList = quest.OpenQuestList;
        }
    }
}
