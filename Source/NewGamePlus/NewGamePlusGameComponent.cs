using System;
using Verse;

namespace Boundir.NewGamePlus
{
    public class NewGamePlusGameComponent : GameComponent
    {
        private readonly Game game;

        public NewGamePlusGameComponent(Game game)
        {
            this.game = game;
        }

        public override void StartedNewGame()
        {
            try
            {
                AreaTransfer.ImportAll(game);
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Failed to apply stored areas to the new game: " + e);
            }
        }
    }
}
