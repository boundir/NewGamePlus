using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class OnPawnJoin
    {
        public static void HostilityResponse(Pawn p, PopAdaptationEvent ev)
        {
            if (PopAdaptationEvent.GainedColonist == ev && p.IsColonist)
            {
                Apply(p);
            }
        }

        public static void Apply(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || !pawn.RaceProps.Humanlike || pawn.playerSettings == null)
            {
                return;
            }

            if (IsPawnCompatibleWithResponseMode(pawn))
            {
                pawn.playerSettings.hostilityResponse = NewGamePlus.settings.threatResponseMode;
            }
        }

        private static bool IsPawnCompatibleWithResponseMode(Pawn pawn)
        {
            return !(pawn.WorkTagIsDisabled(WorkTags.Violent) && NewGamePlus.settings.threatResponseMode == HostilityResponseMode.Attack);
        }
    }

}