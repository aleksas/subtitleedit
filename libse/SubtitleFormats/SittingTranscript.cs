using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SittingTranscript : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".json"; }
        }

        public override string Name
        {
            get { return "SittingTranscript"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }
        public override bool HasStyleSupport
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var lrs = JsonConvert.DeserializeObject<LrsDataRecord>(subtitle.Header);

            var speaches = new List<LrsSittingSpeach>();
            foreach (var p in subtitle.Paragraphs)
            {
                var speach = new LrsSittingSpeach()
                {
                    //start = p.StartTime.TotalMilliseconds,
                    //end = p.EndTime.TotalMilliseconds,
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

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            subtitle.Paragraphs.Clear();

            var sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            string allText = sb.ToString();

            var lrs = JsonConvert.DeserializeObject<LrsDataRecord>(allText);

            foreach (var speach in lrs.speaches)
            {
                var p = new Paragraph(speach.text, 0, 0);
                p.Extra = speach.speaker;
                p.Actor = speach.speaker;
                subtitle.Paragraphs.Add(p);
            }
            lrs.speaches = new LrsSittingSpeach[] { };

            subtitle.Header = JsonConvert.SerializeObject(lrs);

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }


        public class LrsSittingSpeach
        {
            public string text;
            public string speaker;
        }

        public class LrsDataRecord
        {
            public string kind;
            public string hq_audio_url;
            public string lq_audio_url;
            public string sitting_url;
            public string transcript_url;
            public string date;
            public string[] speakers;
            public string[] full_names;
            public LrsSittingSpeach[] speaches;

            public string Year
            {
                get
                {
                    return date.Trim().ToLowerInvariant().Split('-')[0];
                }
            }

            public string Month
            {
                get
                {
                    return date.Trim().ToLowerInvariant().Split('-')[1];
                }
            }

            public string Day
            {
                get
                {
                    return date.Trim().ToLowerInvariant().Split('-')[2];
                }
            }

            public string Kind
            {
                get
                {
                    return kind.Trim().ToLowerInvariant();
                }
            }

            public static string GetExtension(string url)
            {
                if (url.Trim().EndsWith(".asf", true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    return ".asf";
                }
                return ".mp3";
            }
        }

        public class LrsDataResult
        {
            public string url;
            public string updatetime;
            public string taskid;

            public LrsDataRecord result;
        }
    }
}
