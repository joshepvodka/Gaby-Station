using Robust.Shared.Configuration;

namespace Content.Shared._Gabystation.CCVar;

[CVarDefs]
public sealed partial class GabyCVars
{
    /// <summary>
    /// Discord Webhooks
    /// </summary>
    public static readonly CVarDef<string> BanDiscordWebhook =
        CVarDef.Create("discord.ban_webhook", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Enables alternate job titles for players.
    /// </summary>
    public static readonly CVarDef<bool> ICAlternateJobTitlesEnable =
        CVarDef.Create("ic.alternate_job_titles_enable", true, CVar.SERVER | CVar.REPLICATED);
}
