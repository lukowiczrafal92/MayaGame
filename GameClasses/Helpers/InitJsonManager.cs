namespace BoardGameBackend.Helpers
{
    public static class InitJsonManager{
        
        private static Dictionary<string, string> jsonDictionary = new Dictionary<string, string>();
        
        private static List<string> ChangelogList = new List<string>(){
            "Karty ery: 25 (-4; +2).",
            "Karty wydarzeń: 30 (-27, +3).",
            "Karty celów: 24 (+24).",
            "Karty wydarzeń rozdzielone na wydarzenia oraz karty celów (opcjonalne).",
            "Nowa mechanika punktacji za surowce luksusowe.",
            "Nowa mechanika stolicy i jej punktacji.",
            "Zmieniony koszt akcji Gwiazda Władcy (z 4 na (1 + ERA)).",
            "Akcja Wojny rozdzielona na akcje: Wojna Gwiezdna oraz Podbój. Nowe zasady podboju.",
            "Interfejs: uproszczony wybór karty władcy."
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
            filePath = "Data/Stolice.json";
            if (File.Exists(filePath))
                jsonDictionary.Add("Stolice.json", File.ReadAllText(filePath));      
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