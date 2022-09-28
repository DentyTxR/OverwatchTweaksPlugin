using System;
using HarmonyLib;
using RemoteAdmin;
using System.Text;
using CommandSystem;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using CommandSystem.Commands.RemoteAdmin;


namespace OverwatchTweaksPlugin.Patches
{
    [HarmonyPatch(typeof(OverwatchCommand), nameof(OverwatchCommand.Execute))]

    public static class OverwatchCommandPatch
    {
        public static string Command { get; } = "overwatch";
        public static string[] Aliases { get; } = new string[]
        {
            "ovr"
        };
        public static string Description { get; } = "Changes the status of overwatch mode for the specified player(s).";
        public static string[] Usage { get; } = new string[]
        {
            "%player%",
            "enable/disable (Leave blank for toggle)"
        };


        public static bool Prefix(OverwatchCommand instance, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(global::PlayerPermissions.Overwatch, out response))
            {
                return false;
            }
            string[] array = new string[0];
            PlayerCommandSender playerCommandSender;
            List<ReferenceHub> list;
            if ((playerCommandSender = (sender as PlayerCommandSender)) != null && (arguments.Count == 0 || (arguments.Count == 1 && !arguments.At(0).Contains(".") && !arguments.At(0).Contains("@"))))
            {
                list = new List<ReferenceHub>();
                list.Add(playerCommandSender.ReferenceHub);
                if (arguments.Count > 1)
                {
                    array[0] = arguments.At(1);
                }
                else
                {
                    array = null;
                }
            }
            else
            {
                list = Utils.RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out array, false);
            }
            Misc.CommandOperationMode commandOperationMode;
            if (!Misc.TryCommandModeFromArgs(ref array, out commandOperationMode))
            {
                response = "Invalid option " + array[0] + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.";
                return false;
            }
            StringBuilder stringBuilder = (commandOperationMode == Misc.CommandOperationMode.Toggle) ? null : StringBuilderPool.Shared.Rent();
            PlayerCommandSender playerCommandSender2 = (PlayerCommandSender)sender;
            bool flag = playerCommandSender2.ServerRoles.Staff || playerCommandSender2.CheckPermission(global::PlayerPermissions.PermissionsManagement);
            int num = 0;
            if (list != null)
            {
                foreach (ReferenceHub referenceHub in list)
                {
                    ServerRoles serverRoles = referenceHub.serverRoles;
                    if (!serverRoles.BypassStaff || flag)
                    {
                        switch (commandOperationMode)
                        {
                            case Misc.CommandOperationMode.Disable:
                                if (!serverRoles.enabled)
                                {
                                    continue;
                                }
                                serverRoles.UserCode_CmdSetOverwatchStatus(0);
                                if (num != 0)
                                {
                                    stringBuilder.Append(", ");
                                }
                                stringBuilder.Append(referenceHub.LoggedNameFromRefHub());
                                break;
                            case Misc.CommandOperationMode.Enable:
                                if (serverRoles.enabled)
                                {
                                    continue;
                                }
                                serverRoles.UserCode_CmdSetOverwatchStatus(1);
                                if (num != 0)
                                {
                                    stringBuilder.Append(", ");
                                }
                                stringBuilder.Append(referenceHub.LoggedNameFromRefHub());
                                break;
                            case Misc.CommandOperationMode.Toggle:
                                serverRoles.UserCode_CmdSetOverwatchStatus(2);
                                if (serverRoles.enabled)
                                {
                                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, sender.LogName + " enabled overwatch mode for player " + referenceHub.LoggedNameFromRefHub() + " using overwatch mode toggle command.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
                                }
                                else
                                {
                                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, sender.LogName + " disabled overwatch mode for player " + referenceHub.LoggedNameFromRefHub() + " using overwatch mode toggle command.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
                                }
                                break;
                        }
                        num++;
                    }
                }
            }
            if (num > 0)
            {
                if (commandOperationMode != Misc.CommandOperationMode.Disable)
                {
                    if (commandOperationMode == Misc.CommandOperationMode.Enable)
                    {
                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Format("{0} enabled overwatch mode for player{1}{2}.", sender.LogName, (num == 1) ? " " : "s ", stringBuilder), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
                        StringBuilderPool.Shared.Return(stringBuilder);
                    }
                }
                else
                {
                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Format("{0} disabled overwatch mode for player{1}{2}.", sender.LogName, (num == 1) ? " " : "s ", stringBuilder), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
                    StringBuilderPool.Shared.Return(stringBuilder);
                }
            }
            response = string.Format("Done! The request affected {0} player{1}", num, (num == 1) ? "!" : "s!");
            return true;
        }
    }
}