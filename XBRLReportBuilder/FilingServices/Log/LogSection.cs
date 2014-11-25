/*****************************************************************************
 * LogSection (class)
 * Copyright © 2006-2007 Rivet Software, Inc. All rights reserved.
 * Defines a new section that can be added to a .NET application config file.
 * This configuration section can then be used to define how the LogManager 
 * interacts with the Windows Event Log.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Aucent.FilingServices.Log
{
    /// <summary>
    /// Class to read custom configuration section in the config file.
    /// Reads <LogSection logName="XBRLServiceLog" sourceName="myservice" exclude="Info|Warning"></LogSection> from the config file.
    /// The custom config section is used to log to event log and also allows to exclude certain types of logging.
    /// </summary>
    public class LogSection:ConfigurationSection
    {
        // The collection (property bag) that contains 
        // the section properties.
        private static ConfigurationPropertyCollection _Properties;

        // The logName property.
        private static readonly ConfigurationProperty logName =
            new ConfigurationProperty("logName",
            typeof(string), "ReportBuilder",
            ConfigurationPropertyOptions.IsRequired);

        // The sourceName property.
        private static readonly ConfigurationProperty sourceName =
            new ConfigurationProperty("sourceName",
            typeof(string), "FilingServices",
            ConfigurationPropertyOptions.IsRequired);

        // Exclude flags - All,None,Info,Warning,Error or any combination (Info|Warning)
        private static readonly ConfigurationProperty exclude =
            new ConfigurationProperty("exclude",
            typeof(string), "None", ConfigurationPropertyOptions.None);

        public LogSection()
        {
            _Properties = new ConfigurationPropertyCollection();

            _Properties.Add(logName);
            _Properties.Add(sourceName);
            _Properties.Add(exclude);
        }

        // This is a key customization. 
        // It returns the initialized property bag.
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _Properties;
            }
        }

        public string LogName
        {
            get
            {
                return (string)this["logName"];
            }
            //set
            //{
            //    this["logName"] = value;
            //}
        }

        
        public string SourceName
        {
            get
            {
                return (string)this["sourceName"];
            }
            //set
            //{
            //    this["sourceName"] = value;
            //}
        }

        public ExcludeFlag Exclude
        {
            get
            {
                ExcludeFlag retVal = ExcludeFlag.None;
                //return (ExcludeFlag)this["exclude"];
                string excludeFlags = (string) this["exclude"];

                if (!string.IsNullOrEmpty(excludeFlags))
                {
                    excludeFlags = excludeFlags.TrimEnd("|".ToCharArray());
                    string[] flags = excludeFlags.Split("|".ToCharArray());
                    
                    foreach (string flag in flags)
                    {
                        switch (flag.ToLower())
                        {
                            case "all":
                                retVal = ExcludeFlag.All;
                                break;
                            case "info":
                                retVal |= ExcludeFlag.Info;
                                break;
                            case "warning":
                                retVal |= ExcludeFlag.Warning;
                                break;
                            case "error":
                                retVal |= ExcludeFlag.Error;
                                break;
                            case "none":
                                retVal = ExcludeFlag.None;
                                break;
                            default:
                                retVal = ExcludeFlag.All;
                                break;
                        }
                        if (retVal == ExcludeFlag.None || retVal == ExcludeFlag.All)
                            break;
                    }
                }
                return retVal;
            }
        }
    }

    [Flags]
    public enum ExcludeFlag
    {
        None = 0,
        All =1,
        Info = 2,
        Warning =4,
        Error =8
    }
}
