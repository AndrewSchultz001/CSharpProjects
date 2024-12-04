using System;
using System.ComponentModel;

namespace ThermostatEventsApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press Any Key to Start the Device...");
            Console.ReadKey();

            IDevice device = new Device();
            device.RunDevice();
            Console.ReadKey();
        }
    }

    public class Device : IDevice
    {

        const double WARNINGLEVEL = 27;
        const double EMERGENCYLEVEL = 75;

        public double WarningTemperatureLevel => WARNINGLEVEL;
        public double EmergencyTemperatureLevel => EMERGENCYLEVEL;

        public void HandleEmergency()
        {
            Console.WriteLine();
            Console.WriteLine("Sending out notifications to emergency services personal...");
            ShutDownDevice();
            Console.WriteLine();
        }

        private void ShutDownDevice()
        {
            Console.WriteLine("Shutting down device...");
        }

        public void RunDevice()
        {
            Console.WriteLine("Device is running...");

            ICoolingMechanism coolingMechanism = new CoolingMechanism();
            IHeatSensor heatSensor = new HeatSensor(WARNINGLEVEL, EMERGENCYLEVEL);
            IThermostat thermostat = new Thermostat(coolingMechanism, heatSensor, this);

            thermostat.RunThermostat();
        }
    }

    public class Thermostat : IThermostat
    {
        private ICoolingMechanism _coolingMechanism = null;
        private IHeatSensor _heatSensor = null;
        private IDevice _device = null;

        public Thermostat(ICoolingMechanism coolingMechanism, IHeatSensor heatSensor, IDevice device)
        {
            _coolingMechanism = coolingMechanism;
            _heatSensor = heatSensor;
            _device = device;
        }

        private void WireUpEventsToEventHandlers()
        {
            _heatSensor.TemperatureReachesWarningLevelEventHandler += HeatSensor_TemperatureReachesWarningLevelEventHandler;
            _heatSensor.TemperatureFallsBelowWarningLevelEventHandler += HeatSensor_TemperatureFallsBelowWarningLevelEventHandler;
            _heatSensor.TemperatureReachesEmergencyLevelEventHandler += HeatSensor_TemperatureReachesEmergencyLevelEventHandler;
        }

        private void HeatSensor_TemperatureReachesWarningLevelEventHandler(object sender, TemperatureEventsArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine();
            Console.WriteLine($"Warning Alert!! (Warning level is between {_device.WarningTemperatureLevel} and {_device.EmergencyTemperatureLevel})");
            _coolingMechanism.On();
            Console.ResetColor();
        }

        private void HeatSensor_TemperatureFallsBelowWarningLevelEventHandler(object sender, TemperatureEventsArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine();
            Console.WriteLine($"Information Alert!! Temperature falls below warning level (Warning level is between {_device.WarningTemperatureLevel} and {_device.EmergencyTemperatureLevel})");
            _coolingMechanism.Off();
            Console.ResetColor();
        }

        private void HeatSensor_TemperatureReachesEmergencyLevelEventHandler(object sender, TemperatureEventsArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine($"Emergency Alert!! (Emergency level is {_device.EmergencyTemperatureLevel} and above)");
            _device.HandleEmergency();
            Console.ResetColor();
        }

        public void RunThermostat()
        {
            Console.WriteLine("Thermostat is running...");
            WireUpEventsToEventHandlers();
            _heatSensor.RunHeatSensor();
        }
    }

    public interface IThermostat
    {
        void RunThermostat();
    }

    public interface IDevice
    {
        double WarningTemperatureLevel { get; }
        double EmergencyTemperatureLevel { get; }
        void RunDevice();
        void HandleEmergency();
    }

    public class CoolingMechanism : ICoolingMechanism
    {
        public void On()
        {
            Console.WriteLine();
            Console.WriteLine("Turning cooling mechanism on...");
            Console.WriteLine();
        }

        public void Off()
        {
            Console.WriteLine();
            Console.WriteLine("Turning cooling mechanism off...");
            Console.WriteLine();
        }
    }

    public interface ICoolingMechanism
    {
        void On();
        void Off();
    }

    public class HeatSensor : IHeatSensor
    {

        double _warningLevel = 0;
        double _emergencyLevel = 0;
        bool _hasReachedWarningTemperature = false;

        protected EventHandlerList _listEventDelegates = new EventHandlerList();
        static readonly object _temperatureReachesWarningLevel = new object();
        static readonly object _temperatureReachesEmergencyLevel = new object();
        static readonly object _temperatureFallsBelowWarningLevel = new object();

        private double[] _temperatureData = null;

        public HeatSensor(double warningLevel, double emergencyLevel)
        {
            _warningLevel = warningLevel;
            _emergencyLevel = emergencyLevel;
            seed_data();
        }

        private void MonitorTemperature()
        {
            foreach (double temp in _temperatureData)
            {
                Console.ResetColor();
                Console.WriteLine($"DateTime: {DateTime.Now}, Temperature: {temp}");

                if (temp >= _emergencyLevel)
                {
                    TemperatureEventsArgs e = new TemperatureEventsArgs
                    {
                        Temperature = temp,
                        CurrentDateTime = DateTime.Now
                    };
                    OnTemperatureReachesEmergencyLevel(e);
                }
                else if (temp >= _warningLevel)
                {
                    _hasReachedWarningTemperature = true;
                    TemperatureEventsArgs e = new TemperatureEventsArgs
                    {
                        Temperature = temp,
                        CurrentDateTime = DateTime.Now
                    };
                    OnTemperatureReachesWarningLevel(e);
                }
                else if (temp < _warningLevel && _hasReachedWarningTemperature)
                {
                    _hasReachedWarningTemperature = false;
                    TemperatureEventsArgs e = new TemperatureEventsArgs
                    {
                        Temperature = temp,
                        CurrentDateTime = DateTime.Now
                    };
                    OnTemperatureFallsBelowWarningLevel(e);
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private void seed_data()
        {
            _temperatureData = new double[] { 16, 17, 16.5, 18, 19, 22, 24, 26.75, 28.7, 27.6, 26, 24, 22, 45, 68, 86.45 };
        }

        protected void OnTemperatureReachesWarningLevel(TemperatureEventsArgs e)
        {
            EventHandler<TemperatureEventsArgs> handler = (EventHandler<TemperatureEventsArgs>)_listEventDelegates[_temperatureReachesWarningLevel];

            if (handler != null)
            {
                handler(this, e);   
            }
        }

        protected void OnTemperatureReachesEmergencyLevel(TemperatureEventsArgs e)
        {
            EventHandler<TemperatureEventsArgs> handler = (EventHandler<TemperatureEventsArgs>)_listEventDelegates[_temperatureReachesEmergencyLevel];

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnTemperatureFallsBelowWarningLevel(TemperatureEventsArgs e)
        {
            EventHandler<TemperatureEventsArgs> handler = (EventHandler<TemperatureEventsArgs>)_listEventDelegates[_temperatureFallsBelowWarningLevel];

            if (handler != null)
            {
                handler(this, e);
            }
        }

        event EventHandler<TemperatureEventsArgs> IHeatSensor.TemperatureReachesEmergencyLevelEventHandler
        {
            add
            {
                _listEventDelegates.AddHandler(_temperatureReachesEmergencyLevel, value);
            }
            remove
            {
                _listEventDelegates.RemoveHandler(_temperatureReachesEmergencyLevel, value);
            }
        }

        event EventHandler<TemperatureEventsArgs> IHeatSensor.TemperatureReachesWarningLevelEventHandler
        {
            add
            {
                _listEventDelegates.AddHandler(_temperatureReachesWarningLevel, value);
            }
            remove
            {
                _listEventDelegates.RemoveHandler(_temperatureReachesWarningLevel, value);
            }
        }

        event EventHandler<TemperatureEventsArgs> IHeatSensor.TemperatureFallsBelowWarningLevelEventHandler
        {
            add
            {
                _listEventDelegates.AddHandler(_temperatureFallsBelowWarningLevel, value);
            }
            remove
            {
                _listEventDelegates.RemoveHandler(_temperatureFallsBelowWarningLevel, value);
            }
        }

        public void RunHeatSensor()
        {
            Console.WriteLine("Heat sensor is running...");
            MonitorTemperature();
        }
    }

    public interface IHeatSensor
    {
        event EventHandler<TemperatureEventsArgs> TemperatureReachesEmergencyLevelEventHandler;
        event EventHandler<TemperatureEventsArgs> TemperatureReachesWarningLevelEventHandler;
        event EventHandler<TemperatureEventsArgs> TemperatureFallsBelowWarningLevelEventHandler;

        void RunHeatSensor();
    }

    public class TemperatureEventsArgs : EventArgs
    {
        public double Temperature { get; set; }
        public DateTime CurrentDateTime { get; set; }

    }
}