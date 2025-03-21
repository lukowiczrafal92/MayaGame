namespace BoardGameBackend.Helpers
{
    public static class InitJsonManager{
        
        private static Dictionary<string, string> jsonDictionary = new Dictionary<string, string>();
        
        private static List<string> ChangelogList = new List<string>(){
            "03.2025",
            "Karty ery: 12.",
            "Karty wydarzeń: 28.",
            "16.03 POPRAWKI PO INSTRUKCJI:",
            "Karty konstelacji dają 2/3 punkty (zamiast 3/4).",
            "Punktacja końcowa: 2 punkty zamiast 1 punktu za każde 'zwycięstwo' na torze bóstw.",
            "TO PONIŻEJ NIE JEST POKAZANE W INTERFEJSIE:",
            "'4 poziom' bóstw daje 1 punkt + (1 punkt za każdego władcę z tym patronem)."
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