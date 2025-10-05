using System;
using System.Collections.Generic;
using System.IO;
using CentralAuth;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Components;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using SCPSLAudioApi.AudioCore;
using UnityEngine;
using VoiceChat;
using VoiceChat.Codec;

namespace SCP294.Classes
{
    public class SoundHandler
    {
        public OpusEncoder Encoder = new OpusEncoder(VoiceChat.Codec.Enums.OpusApplicationType.Voip);

        // Borrowed from AutoEvents <3
        public static List<ReferenceHub> AudioPlayers = new List<ReferenceHub>();
        public static void PlayAudio(string audioFile, byte volume, bool loop, string soundName, Vector3 position, float dur = 0)
        {
            try
            {
                //var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
                int id = 9999 + AudioPlayers.Count;
                //var fakeConnection = new FakeConnection(id++);
                //var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
                //AudioPlayers.Add(hubPlayer);
                //NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
                Npc audioNpc = Npc.Spawn(soundName, position != Vector3.zero ? RoleTypeId.Tutorial : RoleTypeId.Spectator);
                audioNpc.Health = 9999f;
                //audioNpc.IsGodModeEnabled = true;
                audioNpc.ReferenceHub.characterClassManager._godMode = true;
                audioNpc.ReferenceHub.playerStats.GetModule<AdminFlagsStat>().SetFlag(AdminFlags.GodMode, true);
                var hubPlayer = audioNpc.ReferenceHub;
                AudioPlayers.Add(hubPlayer);

                hubPlayer.authManager.InstanceMode = ClientInstanceMode.Unverified;

                try
                {
                    hubPlayer.nicknameSync.SetNick(soundName);
                }
                catch (Exception)
                {
                    // ignored
                }

                var audioPlayer = AudioPlayerBase.Get(hubPlayer);

                var path = Path.Combine(Path.Combine(Paths.Configs, "SCP294"), audioFile);

                audioPlayer.Enqueue(path, -1);
                audioPlayer.LogDebug = false;
                audioPlayer.BroadcastChannel = VoiceChatChannel.Intercom;
                if (position != Vector3.zero)
                {
                    audioPlayer.BroadcastChannel = VoiceChatChannel.Proximity;
                    try
                    {
                        hubPlayer.roleManager.ServerSetRole(RoleTypeId.Tutorial, RoleChangeReason.None, RoleSpawnFlags.None);
                        hubPlayer.gameObject.transform.position = position;

                        hubPlayer.gameObject.transform.localScale = Vector3.zero;
                        foreach (Player item in Player.List)
                        {
                            Server.SendSpawnMessage?.Invoke(null, [hubPlayer.networkIdentity, item.Connection]);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                audioPlayer.Volume = volume;
                audioPlayer.Loop = loop;
                audioPlayer.Play(0);

                if (dur != 0)
                {
                    Timing.CallDelayed(dur, delegate
                    {
                        hubPlayer.transform.position = new Vector3(-99999,-99999,-99999);
                        AudioPlayers.Remove(hubPlayer);
                        if (audioPlayer.CurrentPlay != null)
                        {
                            audioPlayer.Stoptrack(true);
                            audioPlayer.OnDestroy();
                        }

                        hubPlayer.gameObject.transform.position = new Vector3(-9999f, -9999f, -9999f);
                        Timing.CallDelayed(0.5f, () =>
                        {
                            NetworkServer.Destroy(hubPlayer.gameObject);
                        });
                        //NetworkConnectionToClient conn = hubPlayer.connectionToClient;
                        //hubPlayer.OnDestroy();
                        //CustomNetworkManager.TypedSingleton.OnServerDisconnect(conn);
                        //UnityEngine.Object.Destroy(hubPlayer.gameObject);
                    });
                }

                Log.Debug($"Playing sound {path}");
            }
            catch (Exception e)
            {
                Log.Error($"Error on: {e.Data} -- {e.StackTrace}");
            }
        }
        public static void StopAudio()
        {
            foreach (var player in AudioPlayers)
            {
                if (!player) continue;
                var audioPlayer = AudioPlayerBase.Get(player);
                if (!audioPlayer) continue;

                if (audioPlayer.CurrentPlay != null)
                {
                    audioPlayer.Stoptrack(true);
                    audioPlayer.OnDestroy();
                }

                player.gameObject.transform.position = new Vector3(-9999f, -9999f, -9999f);
                Timing.CallDelayed(0.5f, () =>
                {
                    NetworkServer.Destroy(player.gameObject);
                });
                //NetworkConnectionToClient conn = player.connectionToClient;
                //player.OnDestroy();
                //CustomNetworkManager.TypedSingleton.OnServerDisconnect(conn);
                //NetworkServer.Destroy(player.gameObject);
            }
            AudioPlayers.Clear();
        }
    }
}
