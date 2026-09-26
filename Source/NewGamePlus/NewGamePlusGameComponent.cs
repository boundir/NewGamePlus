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

                foreach (Map map in game.Maps)
                {
                    if (!map.IsPlayerHome)
                    {
                        continue;
                    }

                    foreach (Pawn pawn in map.mapPawns.FreeColonists)
                    {
                        OnPawnJoin.Apply(pawn);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Failed to apply stored policies and areas to the new game: " + e);
            }
        }
    }
}
