using BepInEx;
using UnityEngine;
using UnityEditor;
using Buttplug;
using System;
using System.Threading.Tasks;

namespace DeathsVibes
{
    [BepInPlugin("Lilly.DeathsVibes", "Death's Vibes", "1.0.0")]
    [BepInProcess("DeathsDoor.exe")]

    public class DeathsVibes : BaseUnityPlugin
    {
        async void Awake()
        {
            await RunExample();
        }


        private async Task RunExample()
        {
            var connector = new ButtplugEmbeddedConnectorOptions();
            var client = new ButtplugClient("Death's Vibes Client");

            try
            {
                await client.ConnectAsync(connector);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Can't connect, exiting!");
                Logger.LogWarning($"Message: {ex.InnerException.Message}");
                return;
            }
            Logger.LogWarning("Connected!");

        devicelost:
            await client.StartScanningAsync();
            while (client.Devices.Length == 0)
                await Task.Delay(5000);
            await client.StopScanningAsync();
            Logger.LogWarning("Client currently knows about these devices:");
            foreach (var device in client.Devices)
            {
                Logger.LogWarning($"- {device.Name}");
            }

            foreach (var device in client.Devices)
            {
                Logger.LogWarning($"{device.Name} supports these messages:");
                foreach (var msgInfo in device.AllowedMessages)
                {
                    Logger.LogWarning($"- {msgInfo.Key.ToString()}");
                    if (msgInfo.Value.FeatureCount != 0)
                    {
                        Logger.LogWarning($" - Features: {msgInfo.Value.FeatureCount}");
                    }
                }
            }

            Logger.LogWarning("Sending commands");

            var testClientDevice = client.Devices;

            GameObject Player = null;
            float MaxHealth = -1;
            float Power;

            while (true)
            {
            playerlost:
                if (Player == null)
                {
                    await Task.Delay(1000);
                    Player = GameObject.Find("PLAYER");
                    if (Player != null)
                    {
                        if (Player.GetComponent<PlayerGlobal>().GetHealth() > MaxHealth)
                        {
                            MaxHealth = Player.GetComponent<PlayerGlobal>().GetHealth();
                            //Logger.LogWarning("Max Health = " + MaxHealth);
                        }
                        Logger.LogWarning("Player Found");
                    }
                }
                else
                {
                    try
                    {
                        await Task.Delay(500);
                        for (int i = 0; i < testClientDevice.Length; i++)
                        {
                            Power = (1f - (Player.GetComponent<PlayerGlobal>().GetHealth() / MaxHealth));
                            await testClientDevice[i].SendVibrateCmd(Power);
                            //Logger.LogWarning("Health = " + Player.GetComponent<PlayerGlobal>().GetHealth());
                            //Logger.LogWarning("Max Health = " + MaxHealth);
                            //Logger.LogWarning("Power = " + Power);
                        }
                    }
                    catch (ButtplugDeviceException)
                    {
                        Logger.LogWarning("device lost");
                        goto devicelost;
                    }
                    catch (Exception)
                    {
                        Logger.LogWarning("player lost");
                        Player = null;
                        goto playerlost;
                    }
                }
            }
        }
    }
}
