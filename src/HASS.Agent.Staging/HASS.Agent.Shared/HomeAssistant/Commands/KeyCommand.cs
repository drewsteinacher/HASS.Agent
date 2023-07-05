﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HASS.Agent.Shared.Enums;
using HASS.Agent.Shared.Models.HomeAssistant;
using Serilog;
using static HASS.Agent.Shared.Functions.NativeMethods;

namespace HASS.Agent.Shared.HomeAssistant.Commands
{
    /// <summary>
    /// Command to simulate a keypress
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class KeyCommand : AbstractCommand
    {
        private const string DefaultName = "key";

        public const int VK_MEDIA_NEXT_TRACK = 0xB0; //todo: fix
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3; //todo: fix
        public const int VK_MEDIA_PREV_TRACK = 0xB1; //todo: fix
        public const int VK_VOLUME_MUTE = 0xAD; //todo: fix
        public const int VK_VOLUME_UP = 0xAF; //todo: fix
        public const int VK_VOLUME_DOWN = 0xAE; //todo: fix
        public const int KEY_UP = 38;  //todo: fix

        public string State { get; protected set; }
        public byte KeyCode { get; set; }

        public KeyCommand(byte keyCode, string name = DefaultName, string friendlyName = DefaultName, CommandEntityType entityType = CommandEntityType.Switch, string id = default) : base(name ?? DefaultName, friendlyName ?? null, entityType, id)
        {
            KeyCode = keyCode;
            State = "OFF";
        }

        public override DiscoveryConfigModel GetAutoDiscoveryConfig()
        {
            if (Variables.MqttManager == null) return null;

            var deviceConfig = Variables.MqttManager.GetDeviceConfigModel();
            if (deviceConfig == null) return null;

            return new CommandDiscoveryConfigModel
            {
                Name = Name,
                FriendlyName = FriendlyName,
                Unique_id = Id,
                Availability_topic = $"{Variables.MqttManager.MqttDiscoveryPrefix()}/sensor/{deviceConfig.Name}/availability",
                Command_topic = $"{Variables.MqttManager.MqttDiscoveryPrefix()}/{Domain}/{deviceConfig.Name}/{ObjectId}/set",
                Action_topic = $"{Variables.MqttManager.MqttDiscoveryPrefix()}/{Domain}/{deviceConfig.Name}/{ObjectId}/action",
                State_topic = $"{Variables.MqttManager.MqttDiscoveryPrefix()}/{Domain}/{deviceConfig.Name}/{ObjectId}/state",
                Device = deviceConfig
            };
        }

        public override string GetState() => State;

        public override void TurnOff()
        {
            //
        }

        public override void TurnOn()
        {
            State = "ON";

            var inputs = new INPUT[2];
            inputs[0].type = InputType.INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = KeyCode;

            inputs[1].type = InputType.INPUT_KEYBOARD;
            inputs[1].U.ki.wVk = KeyCode;
            inputs[1].U.ki.dwFlags = KEYEVENTF.KEYUP;

            var ret = SendInput((uint)inputs.Length, inputs, INPUT.Size);
            if (ret != inputs.Length)
            {
                var error = Marshal.GetLastWin32Error();
                Log.Error($"[{DefaultName}] Error simulating key press for {KeyCode}: {error}");
            }

            State = "OFF";
        }

        public override void TurnOnWithAction(string action)
        {
            //
        }
    }
}
