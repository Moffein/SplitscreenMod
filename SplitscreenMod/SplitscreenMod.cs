using BepInEx;
using R2API.Utils;
using Rewired;
using RoR2;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Moffein
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.SplitscreenMod", "SplitscreenMod", "1.0.0")]
    public class SplitscreenMod : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };
        }
        [ConCommand(commandName = "splitscreenmod", flags = ConVarFlags.None, helpText = "Logs in the specified number of users, or two by default.")]
        private static void CCSplitscreenMod(ConCommandArgs args)
        {
            int num = 2;
            int value;
            if (args.Count >= 1 && TextSerialization.TryParseInvariant(args[0], out value))
            {
                num = Mathf.Clamp(value, 1, 4);
            }
            if (!NetworkClient.active)
            {
                UserProfile mainProfile = LocalUserManager.GetFirstLocalUser().userProfile;
                LocalUserManager.ClearUsers();
                LocalUserManager.LocalUserInitializationInfo[] array = new LocalUserManager.LocalUserInitializationInfo[num];
                array[0].player = ReInput.players.GetPlayer(1);
                array[0].profile = mainProfile;
                if (array[0].profile == null)
                {
                    array[0].profile = UserProfile.CreateGuestProfile();
                    print("SplitscreenMod: Profile could not be loaded. Using guest profile.");
                }
                for (int i = 1; i < num; i++)
                {
                    array[i] = new LocalUserManager.LocalUserInitializationInfo
                    {
                        player = ReInput.players.GetPlayer(2+(i-1)),
                        profile = UserProfile.CreateGuestProfile()
                    };
                }
                LocalUserManager.SetLocalUsers(array);
            }
        }
        [ConCommand(commandName = "splitscreenmodnokeyboard", flags = ConVarFlags.None, helpText = "Logs in the specified number of users, or two by default. Disables keyboard input.")]
        private static void CCSplitscreenModNoKeyboard(ConCommandArgs args)
        {
            int num = 2;
            int value;
            if (args.Count >= 1 && TextSerialization.TryParseInvariant(args[0], out value))
            {
                num = Mathf.Clamp(value, 1, 4);
            }
            if (!NetworkClient.active)
            {
                UserProfile mainProfile = LocalUserManager.GetFirstLocalUser().userProfile;
                LocalUserManager.ClearUsers();
                LocalUserManager.LocalUserInitializationInfo[] array = new LocalUserManager.LocalUserInitializationInfo[num];
                array[0].player = ReInput.players.GetPlayer(2);
                array[0].profile = mainProfile;
                if (array[0].profile == null)
                {
                    array[0].profile = UserProfile.CreateGuestProfile();
                    print("SplitscreenMod: Profile could not be loaded. Using guest profile.");
                }
                for (int i = 1; i < num; i++)
                {
                    array[i] = new LocalUserManager.LocalUserInitializationInfo
                    {
                        player = ReInput.players.GetPlayer(2 + i),
                        profile = UserProfile.CreateGuestProfile()
                    };
                }
                LocalUserManager.SetLocalUsers(array);
            }
        }
    }

    public class CommandHelper
    {
        public static void RegisterCommands(RoR2.Console self)
        {
            var types = typeof(CommandHelper).Assembly.GetTypes();
            var catalog = self.GetFieldValue<IDictionary>("concommandCatalog");

            foreach (var methodInfo in types.SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)))
            {
                var customAttributes = methodInfo.GetCustomAttributes(false);
                foreach (var attribute in customAttributes.OfType<ConCommandAttribute>())
                {
                    var conCommand = Reflection.GetNestedType<RoR2.Console>("ConCommand").Instantiate();

                    conCommand.SetFieldValue("flags", attribute.flags);
                    conCommand.SetFieldValue("helpText", attribute.helpText);
                    conCommand.SetFieldValue("action", (RoR2.Console.ConCommandDelegate)System.Delegate.CreateDelegate(typeof(RoR2.Console.ConCommandDelegate), methodInfo));

                    catalog[attribute.commandName.ToLower()] = conCommand;
                }
            }
        }
    }
}