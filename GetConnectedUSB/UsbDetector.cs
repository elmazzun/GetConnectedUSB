using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace GetConnectedUSB
{
    public class UsbDetector // : IUsbDetector
    {
        private const string Query = "SELECT * FROM {0} WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'";
        private const string CreationEvent = "__InstanceCreationEvent";
        private const string DeletionEvent = "__InstanceDeletionEvent";
        private const int ReplayNumber = 1;

        public UsbDetector() {
            var bgwDriveDetector = new BackgroundWorker();
            bgwDriveDetector.DoWork += DoWork;
            bgwDriveDetector.RunWorkerAsync();
        }

        public struct USBDeviceInfo {
            public string DeviceId { get; }
            public string SerialNr { get; }
            public char DriveLetter { get; }
            public override string ToString() => $"DeviceId: {DeviceId}; SerialNr: {SerialNr}; DriveLetter: {DriveLetter}";
            public USBDeviceInfo(string deviceId, string SerialNr, char driveLetter) {
                this.DeviceId = deviceId;
                this.SerialNr = SerialNr;
                DriveLetter = driveLetter;
            }
        }

        private readonly Subject<USBDeviceInfo> adds = new Subject<USBDeviceInfo>();
        private readonly Subject<USBDeviceInfo> removes = new Subject<USBDeviceInfo>();

        public IObservable<USBDeviceInfo> Adds => adds.AsObservable();

        public IObservable<USBDeviceInfo> Removes => removes.AsObservable();

        private void DoWork(object sender, DoWorkEventArgs e) {
            SubscribeToEvent(CreationEvent, adds);
            SubscribeToEvent(DeletionEvent, removes);
        }

        private static void SubscribeToEvent(string eventType, IObserver<USBDeviceInfo> observer) {
            WqlEventQuery wqlEventQuery = new WqlEventQuery(string.Format(Query, eventType));
            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(wqlEventQuery);

            var observable = Observable.FromEventPattern<EventArrivedEventHandler, EventArrivedEventArgs>(
                h => insertWatcher.EventArrived += h,
                h => insertWatcher.EventArrived -= h).Replay(ReplayNumber);

            observable.Connect();
            observable.Select(a => a.EventArgs).Select(MapEventArgs).Subscribe(observer);
            insertWatcher.Start();
        }

        private static USBDeviceInfo MapEventArgs(EventArrivedEventArgs e) {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            //string appDataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string deviceId = (string)instance.GetPropertyValue("DeviceID");
            string serialNr = deviceId.Substring(deviceId.LastIndexOf('\\')).Replace("\\", "");
            char driveLetter = 'Z'; //GetDriveLetter(serialNr).First();
            // Console.WriteLine($"[EventArrivedEventArgs] Got new USB with serial ${serialNr}, drive letter ${driveLetter}");
            foreach (PropertyData prop in instance.Properties)
            {
                Console.WriteLine("{0}: {1}", prop.Name, prop.Value);
            }
            return new USBDeviceInfo(deviceId, serialNr, driveLetter);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/31938686/how-to-get-the-drive-letter-of-usb-device-using-wmi
        /// </summary>
        /// <returns></returns>
        static IEnumerable<(string deviceId, string pnpDeviceId, string driveLetter)> SelectDeviceInformation()
        {
            foreach (ManagementObject device in SelectDevices())
            {
                var deviceId = (string)device.GetPropertyValue("DeviceID");
                var pnpDeviceId = (string)device.GetPropertyValue("PNPDeviceID");
                var driveLetter = (string)SelectPartitions(device).SelectMany(SelectDisks).Select(disk => disk["Name"]).SingleOrDefault();
                if (driveLetter == null)
                {
                    driveLetter = "DRIVE NOT ASSIGNED";
                }

                yield return (deviceId, pnpDeviceId, driveLetter);
            }

            /*static */ IEnumerable<ManagementObject> SelectDevices() => new ManagementObjectSearcher(
                    @"SELECT * FROM Win32_DiskDrive WHERE InterfaceType LIKE 'USB%'").Get()
                .Cast<ManagementObject>();

            /*static */ IEnumerable<ManagementObject> SelectPartitions(ManagementObject device) => new ManagementObjectSearcher(
                    "ASSOCIATORS OF {Win32_DiskDrive.DeviceID=" +
                    "'" + device.Properties["DeviceID"].Value + "'} " +
                    "WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get()
                .Cast<ManagementObject>();

            /*static */ IEnumerable<ManagementObject> SelectDisks(ManagementObject partition) => new ManagementObjectSearcher(
                    "ASSOCIATORS OF {Win32_DiskPartition.DeviceID=" +
                    "'" + partition["DeviceID"] + "'" +
                    "} WHERE AssocClass = Win32_LogicalDiskToPartition").Get()
                .Cast<ManagementObject>();
        }

        private static string GetDriveLetter(string serialNr) {
            return SelectDeviceInformation()
               .Single(a =>
               a.pnpDeviceId.Remove(a.pnpDeviceId.Length - 2).Substring(a.pnpDeviceId.LastIndexOf('\\')).Replace("\\", "") == (serialNr)).driveLetter;
        }
    }
}
