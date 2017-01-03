using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace iSpyApplication
{
    public static class LocRm
    {
        private static Translations _translationsList;
        public static TranslationsTranslationSet CurrentSet;

        public static List<TranslationsTranslationSet> TranslationSets
        {
            get { return TranslationsList.TranslationSet.ToList(); }
        }

        public static Translations TranslationsList
        {
            get
            {
                if (_translationsList != null)
                    return _translationsList;
                var s = new XmlSerializer(typeof (Translations));
                var fs = new FileStream(Program.AppDataPath + @"\XML\Translations.xml", FileMode.Open);
                TextReader reader = new StreamReader(fs);
                fs.Position = 0;
                var t = (Translations) s.Deserialize(reader);
                fs.Close();
                reader.Dispose();
                fs.Dispose();
                _translationsList = t;

                //decode
                foreach (TranslationsTranslationSet set in t.TranslationSet)
                {
                    if (set.Translation != null)
                    {
                        foreach (TranslationsTranslationSetTranslation tran in set.Translation)
                        {
                            tran.Value = tran.Value.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");
                        }
                    }
                }
                return _translationsList;
            }
            set { 
                _translationsList = value;
                CurrentSet = null;
            }
        }

        public static string GetString(string identifier)
        {
            string lang = MainForm.Conf.Language;
            if (lang == "NotSet")
            {
                lang = CultureInfo.CurrentCulture.Name.ToLower();
                if (TranslationSets.FirstOrDefault(p => p.CultureCode == lang) != null)
                    MainForm.Conf.Language = lang;
                else
                {
                    lang = lang.Split('-')[0];
                    if (TranslationSets.FirstOrDefault(p => p.CultureCode == lang) != null)
                        MainForm.Conf.Language = lang;
                    else
                        MainForm.Conf.Language = lang = "en";
                }
            }

            
            if (CurrentSet == null)
            {
                CurrentSet = TranslationSets.FirstOrDefault(p => p.CultureCode == lang);
            }
            try
            {
                TranslationsTranslationSetTranslation t =
                    CurrentSet.Translation.FirstOrDefault(p => p.Token == identifier);
                if (t != null)
                {
                    return t.Value;
                    //.Replace("&amp;", "&").Replace(@"\n", Environment.NewLine).Replace("&lt;", "<").Replace("&gt;", ">");
                }
            }
            catch
            {
                //possible threading error where language is reset
            }
            return "!" + identifier + "!";
        }


    }
}