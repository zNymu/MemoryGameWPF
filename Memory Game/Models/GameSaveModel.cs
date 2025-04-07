using System;
using System.Collections.Generic;

namespace MemoryGame.Models
{
    public class GameSaveModel
    {
        public string Username { get; set; }
        public string Category { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int TotalGameTimeInSeconds { get; set; }
        public int RemainingTimeInSeconds { get; set; }
        public DateTime SaveDateTime { get; set; }
        public List<SavedCardModel> Cards { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string FilePath { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string DisplayName => $"{Category} - {Rows}x{Columns} - {SaveDateTime:yyyy-MM-dd HH:mm:ss}";
    }

    public class SavedCardModel
    {
        public int Id { get; set; }
        public string FrontImagePath { get; set; }
        public bool IsMatched { get; set; }
        public bool IsFlipped { get; set; }
        public int GridRow { get; set; }
        public int GridColumn { get; set; }
    }
}