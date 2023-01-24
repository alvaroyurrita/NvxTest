using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crestron.SimplSharp; // For Basic SIMPL# Classes
using Crestron.SimplSharpPro; // For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming; // For Generic Device Support

namespace DGINvxVideoTests
{
    public class ControlSystem : CrestronControlSystem
    {
        private List<DmNvxBaseClass> _nvxDevices;
        private DmNvxD30 _nvxDecoder1;
        private DmNvxD30 _nvxDecoder2;
        private DmNvxD30 _nvxDecoder3;
        private DmNvxE760C _nvxEncoder1;
        private DmNvxE760C _nvxEncoder2;
        private DmNvxE30 _nvxEncoder3;
        private DmNvxE30 _nvxEncoder4;
        private DmNvxE760C _nvxEncoder5;
        private DmNvxE760C _nvxEncoder6;
        private DmNvxE760C _nvxEncoder7;
        private DmNvxE30 _nvxEncoder8;
        private DmNvxE30 _singleEncoder;
        private Dictionary<int, string> _dmInputEvents;
        private Dictionary<int, string> _streamEvents;
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
            _dmInputEvents = GetConstants(typeof(DMInputEventIds));
            _streamEvents = GetConstants(typeof(DMInputEventIds));
            try
            {
                _nvxDecoder1 = new DmNvxD30(0x10, this);
                _nvxDecoder2 = new DmNvxD30(0x20, this);
                _nvxDecoder3 = new DmNvxD30(0x21, this);
                _nvxEncoder1 = new DmNvxE760C(0x11, this);
                _nvxEncoder2 = new DmNvxE760C(0x12, this);
                _nvxEncoder3 = new DmNvxE30(0x13, this);
                _nvxEncoder4 = new DmNvxE30(0x14, this);
                _nvxEncoder5 = new DmNvxE760C(0x22, this);
                _nvxEncoder6 = new DmNvxE760C(0x23, this);
                _nvxEncoder7 = new DmNvxE760C(0x24, this);
                _nvxEncoder8 = new DmNvxE30(0x25, this);
                _nvxDevices = new List<DmNvxBaseClass>
                {
                    (DmNvxBaseClass)_nvxDecoder1,
                    (DmNvxBaseClass)_nvxDecoder2,
                    (DmNvxBaseClass)_nvxDecoder3,
                    (DmNvxBaseClass)_nvxEncoder1,
                    (DmNvxBaseClass)_nvxEncoder2,
                    (DmNvxBaseClass)_nvxEncoder3,
                    (DmNvxBaseClass)_nvxEncoder4,
                    (DmNvxBaseClass)_nvxEncoder5,
                    (DmNvxBaseClass)_nvxEncoder6,
                    (DmNvxBaseClass)_nvxEncoder7,
                    (DmNvxBaseClass)_nvxEncoder8,
                };
                // var nvxEncoder = new DmNvx352(0x21, this);
                // var nvxEncoder = new DmNvx350(0x13, this);
                // nvxDecoder.HdmiIn[2].StreamChange += (device, args) =>
                // {
                //     CrestronConsole.PrintLine($"HdmiIn 2 Stream Change: {args.EventId}: {streamEvents[args.EventId] }");
                // };
                _singleEncoder = _nvxEncoder8;
                // SetIndividualTest(); 
                SetMultipleTest();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }
        private void SetIndividualTest()
        { 
            _singleEncoder.BaseEvent += (device, args) =>
            {
                CrestronConsole.PrintLine($"[{_singleEncoder.ID:X2}]-Base Event: {args.EventId}: {_dmInputEvents[args.EventId]}");
            };
            _singleEncoder.EndpointNameChange += (device, args) =>
            {
                CrestronConsole.PrintLine($"[{_singleEncoder.ID:X2}]-Endpoint Name Change Event {args.EventId}");
            };
            _singleEncoder.IpInformationChange += (device, args) =>
            {
                CrestronConsole.PrintLine(
                    $"[{_singleEncoder.ID:X2}]-IP Information Change Event {args.DeviceIpAddress} {args.Connected}");
            };
            _singleEncoder.OnlineStatusChange += (device, args) =>
            {
                CrestronConsole.PrintLine($"[{_singleEncoder.ID:X2}]-Online Status Event {args.DeviceOnLine}");
                CrestronConsole.PrintLine($"[{_singleEncoder.ID:X2}]-Name {_singleEncoder.Control.NameFeedback.StringValue}");
                _singleEncoder.Control.Name.StringValue = _singleEncoder.Control.NameFeedback.StringValue;
                CrestronConsole.PrintLine($"[{_singleEncoder.ID:X2}]-Multicast {_singleEncoder.Control.MulticastAddressFeedback.StringValue}");
                _singleEncoder.Control.MulticastAddress.StringValue =
                    _singleEncoder.Control.MulticastAddressFeedback.StringValue;
            };  
            _singleEncoder.HdmiIn[1].StreamChange += (device, args) =>
            {
                CrestronConsole.PrintLine(
                    $"[{_singleEncoder.ID:X2}]-HdmiIn 1 Stream Change: {args.EventId}: {_streamEvents[args.EventId]}");
            };
            CrestronConsole.AddNewConsoleCommand(GetSyncSingleStatus, "GetSyncStatus", "",
                ConsoleAccessLevelEnum.AccessAdministrator);
            if (_singleEncoder.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                CrestronConsole.PrintLine($"error registering {_singleEncoder.ID:X2}");
            } 
        }
        private void SetMultipleTest()
        {
            foreach (var nvxDevice in _nvxDevices)
            {
                nvxDevice.BaseEvent += (device, args) =>
                {
                    CrestronConsole.PrintLine($"[{nvxDevice.ID:X2}]-Base Event: {args.EventId}: {_dmInputEvents[args.EventId]}");
                };
                nvxDevice.EndpointNameChange += (device, args) =>
                {
                    CrestronConsole.PrintLine($"[{nvxDevice.ID:X2}]-Endpoint Name Change Event {args.EventId}");
                };
                nvxDevice.IpInformationChange += (device, args) =>
                {
                    CrestronConsole.PrintLine(
                        $"[{nvxDevice.ID:X2}]-IP Information Change Event {args.DeviceIpAddress} {args.Connected}");
                };
                nvxDevice.OnlineStatusChange += (device, args) =>
                {
                    CrestronConsole.PrintLine($"[{nvxDevice.ID:X2}]-Online Status Event {args.DeviceOnLine}");
                };
                if (nvxDevice.HdmiIn == null) continue;
                nvxDevice.HdmiIn[1].StreamChange += (device, args) =>
                {
                    CrestronConsole.PrintLine(
                        $"[{nvxDevice.ID:X2}]-HdmiIn 1 Stream Change: {args.EventId}: {_streamEvents[args.EventId]}");
                };
            }
            CrestronConsole.AddNewConsoleCommand(GetSyncMultipleStatus, "GetSyncStatus", "",
                ConsoleAccessLevelEnum.AccessAdministrator);
            foreach (var encoder in _nvxDevices)
            {
                if (encoder.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    CrestronConsole.PrintLine($"error registering {encoder.ID}");
                }
            }
        }
        private void GetSyncMultipleStatus(string parameters)
        {
            if (parameters == "")
            {
                foreach (var encoder in _nvxDevices)
                {

                    if (encoder.HdmiIn == null) continue;
                    foreach (var input in encoder.HdmiIn)
                    {
                        var sync = input.SyncDetectedFeedback.BoolValue;
                        CrestronConsole.PrintLine($" [{encoder.ID:X2}]-SyncStatus: {sync}");
                    }
                }
                return;
            }
            _ = UInt16.TryParse(parameters, out ushort id);
            var singleEncoder = _nvxDevices.Find(e => e.ID == id);
            if (singleEncoder == null || singleEncoder.HdmiIn == null) return;
            CrestronConsole.PrintLine($"[{id:X2}]-SyncStatus: {singleEncoder.HdmiIn[1].SyncDetectedFeedback.BoolValue}");
        }
        private void GetSyncSingleStatus(string parameters)
        {
            if (_singleEncoder.HdmiIn == null) return;
            var sync = _singleEncoder.HdmiIn[1].SyncDetectedFeedback.BoolValue;
            CrestronConsole.PrintLine($"[{_singleEncoder.ID:X2}]-SyncStatus:  {sync}");
            CrestronConsole.PrintLine($"[{_singleEncoder.ID:X2}]-Name {_singleEncoder.Control.NameFeedback.StringValue}");
            CrestronConsole.PrintLine($"[{_singleEncoder.ID:X2}]-Multicast {_singleEncoder.Control.MulticastAddressFeedback.StringValue}");
        }
        private Dictionary<int, string> GetConstants(IReflect type)
        {
            var fieldInfos = type.GetFields(BindingFlags.Public |
                                            BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var constants = fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            return constants.ToDictionary(constant => (int)constant.GetValue(null), constant => constant.Name);
        }
    }
}