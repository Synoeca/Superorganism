using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContentPipeline
{
    [ContentImporter(".sav", DisplayName = "GameState Importer", DefaultProcessor = "GameStateProcessor")]
    public class GameStateImporter : ContentImporter<GameStateContent>
    {
        public override GameStateContent Import(string filename, ContentImporterContext context)
        {
            // Read the save file
            string jsonContent = File.ReadAllText(filename);

            // Deserialize into our content object
            GameStateContent? gameState = JsonSerializer.Deserialize<GameStateContent>(jsonContent);
            gameState.SaveFilename = filename;

            return gameState;
        }
    }
}
