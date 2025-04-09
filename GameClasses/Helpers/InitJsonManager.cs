namespace BoardGameBackend.Helpers
{
    public static class InitJsonManager{
        
        private static Dictionary<string, string> jsonDictionary = new Dictionary<string, string>();
        
        private static List<string> ChangelogList = new List<string>(){
            "Karty ery: 20 (-3; +3).",
            "Karty wydarzeń: 52 (-3; +11).",
            "Czwarty poziom religii daje teraz 1 punkt za każde bóstwo na 3 poziomie.",
            "Pielgrzymka jest teraz jedyną akcją z klasy religii. Może celować w dowolne miasto (nawet neutralne).",
            "Żetony steli zostały usunięte (ich wymagania też). Rozbudowa (rzeźbiarstwo) wymaga 4 władców.",
            "Dodano nowe opcje (akcje specjalistyczne) (punktowanie za pierwsze kąty).",
            "Żetony wojny usunięte. Wojna o Trybut daje teraz dodatkowo 1 wybranego przez gracza specjalistę.",
            "Podbój jest teraz opcją Wojny Gwiezdnej (która kosztuje 4 wojowników i może celować w każde miasto przeciwnika). Nowe zasady podboju w instrukcji.",
            "Nowe zasady rozbudowy w instrukcji."
        };
        static InitJsonManager()
        {   
            string filePath = "Data/Deities.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("Deities.json", File.ReadAllText(filePath));
            filePath = "Data/Resources.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("Resources.json", File.ReadAllText(filePath)); 
            filePath = "Data/TileData.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("TileData.json", File.ReadAllText(filePath));
            filePath = "Data/TileTypes.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("TileTypes.json", File.ReadAllText(filePath));
            filePath = "Data/AngleBoard.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("AngleBoard.json", File.ReadAllText(filePath));    
            filePath = "Data/ResourceConverters.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("ResourceConverters.json", File.ReadAllText(filePath));     
            filePath = "Data/Rulers.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("Rulers.json", File.ReadAllText(filePath));        
            filePath = "Data/ActionCards.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("ActionCards.json", File.ReadAllText(filePath));      
            filePath = "Data/Actions.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("Actions.json", File.ReadAllText(filePath));      
            filePath = "Data/Konstelacje.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("Konstelacje.json", File.ReadAllText(filePath));        
            filePath = "Data/Luxuries.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("Luxuries.json", File.ReadAllText(filePath));  
            filePath = "Data/EraEffects.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("EraEffects.json", File.ReadAllText(filePath)); 
            filePath = "Data/Events.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("Events.json", File.ReadAllText(filePath));                                     
        }
        public static Dictionary<string, string> GetJSONDictionary()
        {
            return jsonDictionary;
        }

        public static List<string> GetChangelogList()
        {
            return ChangelogList;
        }
    }
}