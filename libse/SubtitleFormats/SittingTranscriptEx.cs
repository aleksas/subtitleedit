using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SittingTranscriptEx : SittingTranscript
    {
        public override string Name
        {
            get { return "SittingTranscriptEx"; }
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            subtitle.Paragraphs.Clear();

            var sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            string allText = sb.ToString();

            var lrs = JsonConvert.DeserializeObject<LrsDataRecord2>(allText);
            
            foreach (var speach in lrs.speaches)
            {
                var p = new Paragraph(speach.text, speach.start, speach.end);
                p.Extra = speach.speaker;
                p.Actor = speach.speaker;
                subtitle.Paragraphs.Add(p);
            }
            lrs.speaches = new LrsSittingSpeach2[] { };

            subtitle.Header = JsonConvert.SerializeObject(lrs);

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }
        public override string ToText(Subtitle subtitle, string title)
        {
            var lrs = JsonConvert.DeserializeObject<LrsDataRecord2>(subtitle.Header);

            var speaches = new List<LrsSittingSpeach2>();
            foreach (var p in subtitle.Paragraphs)
            {
                var speach = new LrsSittingSpeach2()
                {
                    start = p.StartTime.TotalMilliseconds == 0 ? 359999999.0d : p.StartTime.TotalMilliseconds,
                    end = p.EndTime.TotalMilliseconds == 0 ? 359999999.0d : p.EndTime.TotalMilliseconds,
                    text = p.Text,
                    speaker = p.Actor
                };
                speaches.Add(speach);
            }

            lrs.speaches = speaches.ToArray();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(lrs);
        }


        public class LrsSittingSpeach2
        {
            public string text;
            public string speaker;

            [JsonProperty(Required = Required.AllowNull)]
            public double start;

            [JsonProperty(Required = Required.AllowNull)]
            public double end;
        }

        public class LrsDataRecord2
        {
            public string kind;
            public string hq_audio_url;
            public string lq_audio_url;
            public string sitting_url;
            public string transcript_url;
            public string date;
            public string[] speakers;
            public string[] full_names;
            public LrsSittingSpeach2[] speaches;
        }
    }
}
