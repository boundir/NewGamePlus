using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class OnPawnJoin
    {
        public static void HostilityResponse(Pawn p, PopAdaptationEvent ev)
        {
            if (PopAdaptationEvent.GainedColonist == ev && p.RaceProps.Humanlike && !p.Dead && p.IsColonist)
            {
                if (IsPawnCompatibleWithResponseMode(p))
                {
                    p.playerSettings.hostilityResponse = NewGamePlus.settings.threatResponseMode;
                }
            }
        }
        private static bool IsPawnCompatibleWithResponseMode(Pawn pawn)
        {
            return !(pawn.WorkTagIsDisabled(WorkTags.Violent) && NewGamePlus.settings.threatResponseMode == HostilityResponseMode.Attack);
        }
    }

}