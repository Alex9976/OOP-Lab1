﻿using OOP.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OOP
{
    public static class FunctionalPluginsLoader
    {
        public static void LoadPlugins(Dictionary<string, IFuncPlugin> FuncPluginsList, Dictionary<string, bool> FuncPluginsListActivartors)
        {
            if (!Directory.Exists(@"Functional Plugins"))
            {
                Directory.CreateDirectory(@"Functional Plugins");
            }
            var files = Directory.GetFiles("Functional Plugins", "*.dll");

            foreach (var file in files)
            {
                Assembly assembly = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), file));

                Type[] pluginTypes;
                pluginTypes = assembly.GetTypes();

                foreach (var pluginType in pluginTypes)
                {
                    if (pluginType.GetInterface("OOP.Sdk.IFuncPlugin") != null)
                    {
                        var creatorInstance = Activator.CreateInstance(pluginType);
                        FuncPluginsList.Add(((IFuncPlugin)creatorInstance).ShortName, (IFuncPlugin)creatorInstance);
                        FuncPluginsListActivartors.Add(((IFuncPlugin)creatorInstance).ShortName, false);
                    }
                }
            }

            Assembly mainAssembly = Assembly.Load("OOP");

            Type[] mainPluginTypes;
            mainPluginTypes = mainAssembly.GetTypes();

            foreach (var pluginType in mainPluginTypes)
            {
                if (pluginType.GetInterface("OOP.Sdk.IFuncPlugin") != null)
                {
                    var creatorInstance = Activator.CreateInstance(pluginType);
                    FuncPluginsList.Add(((IFuncPlugin)creatorInstance).ShortName, (IFuncPlugin)creatorInstance);
                    FuncPluginsListActivartors.Add(((IFuncPlugin)creatorInstance).ShortName, false);
                }
            }
        }
    }
}
