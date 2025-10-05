using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Usables;
using Mirror;
using PlayerRoles.Voice;
using SCP294.Classes;
using SCP294.Types;
using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features.Pools;
using UnityEngine;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Networking;

namespace SCP294.Patches
{

    [HarmonyPatch(typeof(Scp207), nameof(Scp207.OnEffectsActivated))]
    internal static class SCP207OnEffectsActivated
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label skip = generator.DefineLabel();

            // Insert instructions to skip when NPC to the skip label
            newInstructions.Add(new CodeInstruction(OpCodes.Ret));
            newInstructions[newInstructions.Count - 1].labels.Add(skip);

            newInstructions.InsertRange(0, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrinkInfo), nameof(DrinkInfo.IsCustomDrink), new[] { typeof(ItemBase) })),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }

    [HarmonyPatch(typeof(AntiScp207), nameof(AntiScp207.OnEffectsActivated))]
    internal static class AntiSCP207OnEffectsActivated
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label skip = generator.DefineLabel();

            // Insert instructions to skip when NPC to the skip label
            newInstructions.Add(new CodeInstruction(OpCodes.Ret));
            newInstructions[newInstructions.Count - 1].labels.Add(skip);

            newInstructions.InsertRange(0, new List<CodeInstruction>()
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.Method(typeof(DrinkInfo), nameof(DrinkInfo.IsCustomDrink), new[] { typeof(ItemBase) })),
                new(OpCodes.Brtrue_S, skip),
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }

    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    internal static class ServerReceiveMessage
    {
        [HarmonyPrefix]
        private static bool Prefix(NetworkConnection conn, VoiceMessage msg)
        {
            if (!SCP294.Instance.Config.EnableVoiceEffects) return true;

            if (msg.SpeakerNull || msg.Speaker.netId != conn.identity.netId)
            {
                return false;
            }
            IVoiceRole voiceRole = msg.Speaker.roleManager.CurrentRole as IVoiceRole;
            if (voiceRole == null)
            {
                return false;
            }
            if (!voiceRole.VoiceModule.CheckRateLimit())
            {
                return false;
            }
            if (VoiceChatMutes.IsMuted(msg.Speaker))
            {
                return false;
            }
            VoiceChatChannel voiceChatChannel = voiceRole.VoiceModule.ValidateSend(msg.Channel);
            if (voiceChatChannel == VoiceChatChannel.None)
            {
                return false;
            }
            voiceRole.VoiceModule.CurrentChannel = voiceChatChannel;

            Player plr = Player.Get(msg.Speaker);
            if (SCP294.Instance.PlayerVoicePitch.TryGetValue(plr.UserId, out float pitchShift) && !Mathf.Approximately(pitchShift, 1f))
            {
                float[] message = new float[48000];
                OpusComponent comp = OpusComponent.Get(plr.ReferenceHub);
                comp.Decoder.Decode(msg.Data, msg.DataLength, message);

                comp.PitchShift(pitchShift, 480, 48000, message);

                msg.DataLength = comp.Encoder.Encode(message, msg.Data, 480);
            }

            foreach (ReferenceHub referenceHub in ReferenceHub.AllHubs)
            {
                if (referenceHub.roleManager.CurrentRole is IVoiceRole voiceRole2)
                {
                    VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                    if (voiceChatChannel2 != VoiceChatChannel.None)
                    {
                        msg.Channel = voiceChatChannel2;
                        referenceHub.connectionToClient.Send<VoiceMessage>(msg, 0);
                    }
                }
            }
            return false;
        }
    }
}
