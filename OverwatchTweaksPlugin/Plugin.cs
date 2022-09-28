using Exiled.API.Features;
using System;
using HarmonyLib;
using PlayerEvent = Exiled.Events.Handlers.Player;
using ServerEvent = Exiled.Events.Handlers.Server;

namespace OverwatchTweaksPlugin
{
    public class OverwatchTweaksPlugin : Plugin<Config>
    {
        public Harmony harmony;
        public EventHandler EventHandler;
        public static OverwatchTweaksPlugin Singleton;

        public override string Name { get; } = "OverwatchTweaksPlugin";
        public override string Author { get; } = "Denty";
        public override string Prefix { get; } = "OTP"; //DisableStuffPlugin
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(5, 3, 0);

        public override void OnEnabled()
        {
            harmony = new Harmony($"{Author}.{Name}");
            harmony.PatchAll();

            Singleton = this;
            EventHandler = new EventHandler();

            //EnableEvents
            //PlayerEvent.TogglingOverwatch += EventHandler.OnToggleOverwatch;
        }

        public override void OnDisabled()
        {
            harmony.UnpatchAll();

            //DisableEvents
            //PlayerEvent.TogglingOverwatch -= EventHandler.OnToggleOverwatch;

            Singleton = null;
            base.OnDisabled();
        }
    }
}