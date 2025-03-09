
using BoardGameBackend.Mappers;
using Newtonsoft.Json;
using BoardGameBackend.Models;
using BoardGameBackend.Helpers;

namespace BoardGameBackend.GameData
{
    public static class GameDataManager
    {

        private static List<DeityGameData> lDeities = new List<DeityGameData>();
        private static List<ResourceGameData> lResources = new List<ResourceGameData>();
        private static List<TileTypeGameData> lTileTypes = new List<TileTypeGameData>();
        private static List<TileGameData> lTiles = new List<TileGameData>();
        private static List<AngleBoardTile> lAngleBoardTiles = new List<AngleBoardTile>();
        private static List<ResourceConverterGameData> lResourceConverters = new List<ResourceConverterGameData>();
        private static List<RulerGameData> lRulers = new List<RulerGameData>();
        private static List<ActionCardGameData> lActionCards = new List<ActionCardGameData>();
        private static List<ActionGameData> lActions = new List<ActionGameData>();
        private static List<KonstelacjaGameData> lKonstelacje = new List<KonstelacjaGameData>();
        private static List<LuxuryGameData> lLuxuries = new List<LuxuryGameData>();
        private static List<EraEffectGameData> lEraEffects = new List<EraEffectGameData>();
        private static List<EventGameData> lEvents = new List<EventGameData>();

        static GameDataManager()
        {            
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Deities.json");
            var jsonData = File.ReadAllText(fullPath);
            lDeities = JsonConvert.DeserializeObject<List<DeityGameData>>(jsonData);           
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Resources.json");
            jsonData = File.ReadAllText(fullPath);
            lResources = JsonConvert.DeserializeObject<List<ResourceGameData>>(jsonData);
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/TileTypes.json");
            jsonData = File.ReadAllText(fullPath);
            lTileTypes = JsonConvert.DeserializeObject<List<TileTypeGameData>>(jsonData);     
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/TileData.json");
            jsonData = File.ReadAllText(fullPath);
            lTiles = JsonConvert.DeserializeObject<List<TileGameData>>(jsonData);     
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/AngleBoard.json");
            jsonData = File.ReadAllText(fullPath);
            lAngleBoardTiles = JsonConvert.DeserializeObject<List<AngleBoardTile>>(jsonData);    
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/ResourceConverters.json");
            jsonData = File.ReadAllText(fullPath);
            lResourceConverters = JsonConvert.DeserializeObject<List<ResourceConverterGameData>>(jsonData);   
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Rulers.json");
            jsonData = File.ReadAllText(fullPath);
            lRulers = JsonConvert.DeserializeObject<List<RulerGameData>>(jsonData);     
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/ActionCards.json");
            jsonData = File.ReadAllText(fullPath);
            lActionCards = JsonConvert.DeserializeObject<List<ActionCardGameData>>(jsonData);
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Actions.json");
            jsonData = File.ReadAllText(fullPath);
            lActions = JsonConvert.DeserializeObject<List<ActionGameData>>(jsonData);
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Konstelacje.json");
            jsonData = File.ReadAllText(fullPath);
            lKonstelacje = JsonConvert.DeserializeObject<List<KonstelacjaGameData>>(jsonData);
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Luxuries.json");
            jsonData = File.ReadAllText(fullPath);
            lLuxuries = JsonConvert.DeserializeObject<List<LuxuryGameData>>(jsonData);
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/EraEffects.json");
            jsonData = File.ReadAllText(fullPath);
            lEraEffects = JsonConvert.DeserializeObject<List<EraEffectGameData>>(jsonData);
            fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Events.json");
            jsonData = File.ReadAllText(fullPath);
            lEvents = JsonConvert.DeserializeObject<List<EventGameData>>(jsonData);
        }
        
        public static DeityGameData GetDeityById(int id){
            return lDeities.FirstOrDefault(tile => tile.Id == id)!;
        }
        public static TileGameData GetTileById(int id){
            return lTiles.FirstOrDefault(tile => tile.Id == id)!;
        }
        public static List<LuxuryGameData> GetLuxuries()
        {
            return lLuxuries;
        }
        public static ActionGameData GetActionById(int id){
            return lActions.FirstOrDefault(action => action.Id == id)!;
        }
        public static KonstelacjaGameData GetKonstelacjaById(int id){
            return lKonstelacje.FirstOrDefault(action => action.Id == id)!;
        }
        public static ResourceConverterGameData GetResourceConverterById(int id){
            return lResourceConverters.FirstOrDefault(tile => tile.Id == id)!;
        }
        public static EventGameData GetEventById(int id){
            return lEvents.FirstOrDefault(action => action.Id == id)!;
        }
        public static EraEffectGameData GetEraEffectById(int id){
            return lEraEffects.FirstOrDefault(action => action.Id == id)!;
        }
        public static List<KonstelacjaGameData> GetKonstelacje(){
            return lKonstelacje;
        }
        public static List<EraEffectGameData> GetEraEffects(){
            return lEraEffects;
        }
        public static List<EventGameData> GetEvents(){
            return lEvents;
        }
        public static List<ActionCardGameData> GetActionCards(){
            return lActionCards;
        }
        public static List<TileGameData> GetTiles(){
            return lTiles;
        }
        public static RulerGameData GetRulerById(int id){
            return lRulers.FirstOrDefault(tile => tile.Id == id)!;
        }
        public static List<RulerGameData> GetRulers(){
            return lRulers;
        }
        public static List<AngleBoardTile> GetAngleBoardTiles(){
            return lAngleBoardTiles;
        }
        public static List<DeityGameData> GetDeities()
        {
            return lDeities;
        }
        public static List<ResourceConverterGameData> GetResourceConverters()
        {
            return lResourceConverters;
        }
        public static List<ResourceGameData> GetResources()
        {
            return lResources;
        }
    }
}