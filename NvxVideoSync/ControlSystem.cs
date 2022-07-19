using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming; // For Generic Device Support

namespace DGINvxVideoTests
{
    public class ControlSystem : CrestronControlSystem
    {
        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {

        } 

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            var dmInputEvents = GetConstants(typeof(DMInputEventIds));
            var streamEvents = GetConstants(typeof(DMInputEventIds));
            try
            {
                var nvxEncoder = new DmNvxE30(0x13, this);
                // var nvxEncoder = new DmNvx352(0x21, this);
                // var nvxEncoder = new DmNvx350(0x13, this);

                nvxEncoder.BaseEvent += (device, args) =>
                {
                    CrestronConsole.PrintLine($"Base Event: {args.EventId}: {dmInputEvents[args.EventId]}");
                };
                nvxEncoder.EndpointNameChange += (device, args) =>
                {
                    CrestronConsole.PrintLine($"Endpoint Name Change Event {args.EventId}");
                };
                nvxEncoder.IpInformationChange += (device, args) =>
                {
                    CrestronConsole.PrintLine($"IP Information Change Event {args.DeviceIpAddress} {args.Connected}");
                };
                nvxEncoder.OnlineStatusChange += (device, args) =>
                {
                    CrestronConsole.PrintLine($"Online Status Event {args.DeviceOnLine}");
                };
                nvxEncoder.HdmiIn[1].StreamChange += (device, args) =>
                {
                    CrestronConsole.PrintLine($"HdmiIn 1 Stream Change: {args.EventId}: {streamEvents[args.EventId] }");
                };
                // nvxDecoder.HdmiIn[2].StreamChange += (device, args) =>
                // {
                //     CrestronConsole.PrintLine($"HdmiIn 2 Stream Change: {args.EventId}: {streamEvents[args.EventId] }");
                // };
                CrestronConsole.AddNewConsoleCommand(parameters =>
                {
                    CrestronConsole.PrintLine(
                        $"Status HDMI 1 sync {nvxEncoder.HdmiIn[1].SyncDetectedFeedback.BoolValue}");
                    // CrestronConsole.PrintLine(
                    //     $"Status HDMI 1 sync {nvxDecoder.HdmiIn[2].SyncDetectedFeedback.BoolValue}");
                }, "GetStatus", "Gets sync status", ConsoleAccessLevelEnum.AccessAdministrator);
                if (nvxEncoder.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    CrestronConsole.PrintLine("error registering decoder");
                } 
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }
       private Dictionary<int, string> GetConstants(IReflect type) 
        {
            var fieldInfos = type.GetFields(BindingFlags.Public |
                                            BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var constants =  fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            return constants.ToDictionary(constant => (int)constant.GetValue(null), constant => constant.Name);
        }
    }
}