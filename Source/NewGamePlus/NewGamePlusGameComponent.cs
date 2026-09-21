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
                PolicyTransfer.ImportAll(game);
                AreaTransfer.ImportAll(game);
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Failed to apply stored policies and areas to the new game: " + e);
            }
        }
    }
}
