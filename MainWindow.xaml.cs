﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using OOP.Sdk;

namespace OOP
{
    public partial class MainWindow : Window
    {
        public Dictionary<string, ITransportFactoryPlugin> TransportFactoryList = new Dictionary<string, ITransportFactoryPlugin>();
        public List<ITransportPlugin> TransportList = new List<ITransportPlugin>();
        XmlSerializer XMLFormatter;
        BinaryFormatter BinFormatter = new BinaryFormatter();
        List<Type> objectTypesList = new List<Type>();
       
        string ProgrammPath = Directory.GetCurrentDirectory();
        bool IsComponentsInitialized = false;
        object[] Parameters = new object[3];

        Dictionary<string, ITransportFactoryPlugin> creators = null;

        public MainWindow()
        {
            InitializeComponent();

            BinFormatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            BinFormatter.Binder  = new CustomSerializationBinder();

            creators = ReadExtensions(objectTypesList);

            Assembly assembly = Assembly.Load("OOP");

            Type[] type = assembly.GetTypes();
            foreach (Type item in type)
            {
                if (item.IsSubclassOf(typeof(TransportFactory)))
                {
                    comboMain.Items.Add((item.Name).Substring(0, Math.Abs((item.Name).IndexOf("Creator"))));
                    TransportFactoryList.Add((item.Name).Substring(0, Math.Abs((item.Name).IndexOf("Creator"))), (ITransportFactoryPlugin)Activator.CreateInstance(item));
                }
                if (item.IsSubclassOf(typeof(Transport)))
                {
                    objectTypesList.Add(item);
                }
            }
      

            foreach (var creator in creators)
            {
                comboMain.Items.Add(creator.Key);
                TransportFactoryList.Add(creator.Key, creator.Value);
            }

            Type[] objectTypes = new Type[objectTypesList.Count];
            for (int i = 0; i < objectTypes.Length; i++)
            {
                objectTypes[i] = objectTypesList[i];
            }
            XMLFormatter = new XmlSerializer(typeof(List<object>), objectTypes);

            IsComponentsInitialized = true;
            comboMain.SelectedIndex = 0;


        }

        static Dictionary<string, ITransportFactoryPlugin> ReadExtensions(List<Type> objectTypesList)
        {
            var files = Directory.GetFiles("Extensions", "*.dll");
            Dictionary<string, ITransportFactoryPlugin> creatorList = new Dictionary<string, ITransportFactoryPlugin>();

            foreach (var file in files)
            {
                Assembly assembly = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), file));

                Type[] pluginTypes;
                pluginTypes = assembly.GetTypes();

                foreach (var pluginType in pluginTypes)
                {
                    if (pluginType.GetInterface("OOP.Sdk.ITransportFactoryPlugin") != null)
                    {
                        var creatorInstance = Activator.CreateInstance(pluginType);
                        creatorList.Add((pluginType.Name).Substring(0, Math.Abs((pluginType.Name).IndexOf("Creator"))), (ITransportFactoryPlugin)creatorInstance);
                    }
                    if (pluginType.GetInterface("OOP.Sdk.ITransportPlugin") != null)
                    {
                        var creatorInstance = Activator.CreateInstance(pluginType);
                        objectTypesList.Add(creatorInstance.GetType());
                    }
                }
            }
            return creatorList;
        }


        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            
            Parameters[0] = textBox.Text;
            Parameters[1] = comboBox.SelectedIndex + 1;
            Parameters[2] = checkBox.IsChecked;          

            foreach (string Name in TransportFactoryList.Keys)
            {
                if (Name == comboMain.SelectedItem.ToString())
                {
                    TransportList.Add(TransportFactoryList[Name].Create(Parameters));
                    AddObjectToList(TransportFactoryList[Name].ImgPath, TransportList.Count - 1);
                }   
            }
        }

        public void AddObjectToList(string Img, int Element)
        {
            StackPanel stackPanel = new StackPanel { Width = 550, Height = 79 };
            stackPanel.Orientation = Orientation.Horizontal;
            try
            {
                stackPanel.Children.Add(new Image
                {
                    Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Images/", Img))),
                    Stretch = Stretch.Fill,
                    Width = 124,
                    Height = 74
                });
            }
            catch
            {
                MessageBox.Show("Loading image error");
            }            
            
            Label label = new Label { Width = 420, Height = 31 };
            label.Content = TransportList[Element].PrintInfo();
            stackPanel.Children.Add(label);

            listBox.Items.Add(stackPanel);
        }

        private void comboMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsComponentsInitialized)
            {
                comboLabel.Content = TransportFactoryList[comboMain.SelectedItem.ToString()].Question1();
                checkBox.Content = TransportFactoryList[comboMain.SelectedItem.ToString()].Question2();

                string[] elements = TransportFactoryList[comboMain.SelectedItem.ToString()].Answer();
                for (int i = 0; i < comboBox.Items.Count;)
                {
                    comboBox.Items.RemoveAt(i);
                }
                for (int i = 0; i < elements.Length; i++)
                {
                    comboBox.Items.Add(elements[i]);
                }
                comboBox.SelectedIndex = 0;
            }
            
        }
    
        private void btnXMLsave_Click(object sender, RoutedEventArgs e)
        {
            using (FileStream file = new FileStream("Transport.xml", FileMode.Create))
            {
                List<object> transports = new List<object>();
                for (int i = 0; i < TransportList.Count; i++)
                {
                    transports.Add(TransportList[i]);
                }
                XMLFormatter.Serialize(file, transports);
            }
        }

        private void btnXMLload_Click(object sender, RoutedEventArgs e)
        {
            using (FileStream file = new FileStream("Transport.xml", FileMode.OpenOrCreate))
            {
                TransportList.Clear();
                List<object> transports = (List<object>)XMLFormatter.Deserialize(file);
                for (int i = 0; i < transports.Count; i++)
                {
                    try
                    {
                        TransportList.Add((ITransportPlugin)transports[i]);
                    }
                    catch (InvalidCastException ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }

            listBox.Items.Clear();
            
            for (int i = 0; i < TransportList.Count; i++)
            {
                    AddObjectToList(TransportFactoryList[TransportList[i].Name].ImgPath, i);
                
            }

        }

        private void btnBinsave_Click(object sender, RoutedEventArgs e)
        {
            using (FileStream file = new FileStream("Transport.dat", FileMode.Create))
            {
                BinFormatter.Serialize(file, TransportList);
            }
        }

        private void btnBinload_Click(object sender, RoutedEventArgs e)
        {
            using (FileStream file = new FileStream("Transport.dat", FileMode.OpenOrCreate))
            {
                try
                {
                    TransportList = (List<ITransportPlugin>)BinFormatter.Deserialize(file);
                }
                catch (SerializationException)
                {
                    MessageBox.Show("Serialization error");
                }

                listBox.Items.Clear();

                for (int i = 0; i < TransportList.Count; i++)
                {
                    try
                    {
                        AddObjectToList(TransportFactoryList[TransportList[i].Name].ImgPath, i);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        string errorMessage = ex.Message.ToString();
                        MessageBox.Show(errorMessage.Substring(0, errorMessage.IndexOf(".")) + ": " + TransportList[i].Name);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                TransportList.RemoveAt(listBox.SelectedIndex);

                listBox.Items.Clear();

                for (int i = 0; i < TransportList.Count; i++)
                {
                    AddObjectToList(TransportFactoryList[TransportList[i].Name].ImgPath, i);
                }

            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                EditForm editForm = new EditForm(this, listBox.SelectedIndex);
                editForm.Show();
            }
            
        }
    }
}
