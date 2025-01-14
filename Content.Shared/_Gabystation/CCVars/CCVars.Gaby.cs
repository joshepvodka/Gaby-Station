using Robust.Shared.Configuration;

namespace Content.Shared._Gabystation.CCVar;

[CVarDefs]
public sealed partial class GabyCVars
{
        /// <summary>
        /// Enables alternate job titles for players.
        /// </summary>
        public static readonly CVarDef<bool> ICAlternateJobTitlesEnable =
            CVarDef.Create("ic.alternate_job_titles_enable", true, CVar.SERVER | CVar.REPLICATED);

        /// <summary>
        /// If the message of the day lobby widget should be displayed.
        /// </summary>
        public static readonly CVarDef<bool> MOTDBuletinEnable =
            CVarDef.Create("chat.motd_buletin_enable", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE, "If the MOTD buletin appears in lobby");
}
