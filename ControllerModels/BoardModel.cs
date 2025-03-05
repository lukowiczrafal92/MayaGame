
namespace BoardGameBackend.Models
{
    public class MoveToTileDto
    {
        public required int TileId { get; set; }   
        public required bool FullMovement {get; set;}   
        public bool? AdjacentMovement {get; set;}  
        public int? TeleportationPlace {get; set;}
    }

    public class TeleportToTile
    {
        public required int TileId { get; set; }   
    }

    public class SwapTokensData
    {
        public required int TileIdOne { get; set; }  
        public required int TileIdTwo { get; set; } 
    }

    public class RotatePawnData
    {
        public required int TileId { get; set; }  
    }
}