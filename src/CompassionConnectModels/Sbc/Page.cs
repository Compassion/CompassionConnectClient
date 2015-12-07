using System.Collections.Generic;

namespace CompassionConnectModels.Sbc
{
    public class Page
    {
        public string OriginalPageURL { get; set; }

        public List<string> OriginalText { get; set; }

        public List<string> EnglishTranslatedText { get; set; }

        public List<string> TranslatedText { get; set; }

        public string FinalPageURL { get; set; }
    }
}