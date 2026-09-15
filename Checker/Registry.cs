using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Timers;


namespace SpoofingChecker
{
    public class Registry
    {
        //private const int ONE_SECOND = 1000;

        //private readonly ILogger logger;
        private readonly ConcurrentDictionary<(string key, string name), object> values;

        //private Timer timer;

        //public event RegistryValueChangedEventHandler ValueChanged;

        public Registry()
        {
            //this.logger = logger;
            //this.values = new ConcurrentDictionary<(string key, string name), object>();
        }

#if false
        public void StartMonitoring(string key, string name)
        {
            if (timer?.Enabled != true)
            {
                timer = new Timer(ONE_SECOND);
                timer.AutoReset = true;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }

            var success = TryRead(key, name, out var value);

            values.TryAdd((key, name), value);

            if (success)
            {
                logger.Debug($"Started monitoring value '{name}' from registry key '{key}'. Initial value: '{value}'.");
            }
            else
            {
                logger.Debug($"Started monitoring value '{name}' from registry key '{key}'. Value does currently not exist or initial read failed.");
            }
        }

        public void StopMonitoring()
        {
            values.Clear();

            if (timer != null)
            {
                timer.Stop();
                logger.Debug("Stopped monitoring the registry.");
            }
        }

        public void StopMonitoring(string key, string name)
        {
            values.TryRemove((key, name), out _);
        }
#endif

        public bool TryRead(string key, string name, out object value)
        {
            var defaultValue = new object();

            value = default;

            try
            {
                value = Microsoft.Win32.Registry.GetValue(key, name, defaultValue);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to read value '{name}' from registry key '{key}'!", e);
            }

            return value != default && value != defaultValue;
        }

        public bool TryGetNames(string keyName, out IEnumerable<string> names)
        {
            names = default;

            if (TryOpenKey(keyName, out var key))
            {
                using (key)
                {
                    try
                    {
                        names = key.GetValueNames();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to get registry value names for '{keyName}'!", e);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Failed to get names for '{keyName}'.");
            }

            return names != default;
        }

        public bool TryGetSubKeys(string keyName, out IEnumerable<string> subKeys)
        {
            subKeys = default;

            if (TryOpenKey(keyName, out var key))
            {
                using (key)
                {
                    try
                    {
                        subKeys = key.GetSubKeyNames();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to get registry sub key names for '{keyName}'!", e);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Failed to get sub keys for '{keyName}'.");
            }

            return subKeys != default;
        }

        private bool Exists(string key, string name)
        {
            var defaultValue = new object();
            var value = default(object);

            try
            {
                value = Microsoft.Win32.Registry.GetValue(key, name, defaultValue);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to read value '{name}' from registry key '{key}'!", e);
            }

            return value != default && value != defaultValue;
        }

#if false
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var item in values)
            {
                if (Exists(item.Key.key, item.Key.name))
                {
                    if (TryRead(item.Key.key, item.Key.name, out var value))
                    {
                        if (!Equals(item.Value, value))
                        {
                            logger.Debug($"Value '{item.Key.name}' from registry key '{item.Key.key}' has changed from '{item.Value}' to '{value}'!");
                            ValueChanged?.Invoke(item.Key.key, item.Key.name, item.Value, value);
                        }
                    }
                    else
                    {
                        logger.Error($"Failed to monitor value '{item.Key.name}' from registry key '{item.Key.key}'!");
                    }
                }
            }
        }
#endif
        private bool TryOpenKey(string keyName, out RegistryKey key)
        {
            key = default;

            try
            {
                if (TryGetHiveForKey(keyName, out var hive))
                {
                    if (keyName == hive.Name)
                    {
                        key = hive;
                    }
                    else
                    {
                        key = hive.OpenSubKey(keyName.Replace($@"{hive.Name}\", ""));
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to get hive for key '{keyName}'!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to open registry key '{keyName}'!", e);
            }

            return key != default;
        }

        private bool TryGetHiveForKey(string keyName, out RegistryKey hive)
        {
            var length = keyName.IndexOf('\\');
            var name = length != -1 ? keyName.Substring(0, length).ToUpperInvariant() : keyName.ToUpperInvariant();

            hive = default;

            switch (name)
            {
                case "HKEY_CLASSES_ROOT":
                    hive = Microsoft.Win32.Registry.ClassesRoot;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    hive = Microsoft.Win32.Registry.CurrentConfig;
                    break;
                case "HKEY_CURRENT_USER":
                    hive = Microsoft.Win32.Registry.CurrentUser;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    hive = Microsoft.Win32.Registry.LocalMachine;
                    break;
                case "HKEY_PERFORMANCE_DATA":
                    hive = Microsoft.Win32.Registry.PerformanceData;
                    break;
                case "HKEY_USERS":
                    hive = Microsoft.Win32.Registry.Users;
                    break;
            }

            return hive != default;
        }
    }

}
