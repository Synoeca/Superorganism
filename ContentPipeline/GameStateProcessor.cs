using Microsoft.Xna.Framework.Content.Pipeline;

namespace ContentPipeline
{
    [ContentProcessor(DisplayName = "GameState Processor")]
    public class GameStateProcessor : ContentProcessor<GameStateContent, GameStateContent>
    {
        public override GameStateContent Process(GameStateContent gameState, ContentProcessorContext context)
        {
            return gameState;
        }
    }
}
