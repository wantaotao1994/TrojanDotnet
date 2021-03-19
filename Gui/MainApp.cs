using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using NStack;
using Terminal.Gui;
using Winter.Model;
using Winter.Utility;

namespace Winter.Gui
{
    public class MainApp
    {
        private MySetting _options;
        private TrojanContext _trojanContext;
        private IProxySetting _proxySetting;
        private Window _window;
        
        
        public MainApp(IOptions<MySetting> options, IProxySetting proxySetting, TrojanContext trojanContext)
        {
            _proxySetting = proxySetting;
            _trojanContext = trojanContext;
            _options = options.Value;
        }
        
        
        public void Run()
        {
             Application.Init ();
             
            var top = Application.Top;
            // Creates the top-level window to show
            _window = new Window ("FAY PROXY") {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };
            _window.ColorScheme = Colors.TopLevel;
         
            
            top.Add (_window);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar (new MenuBarItem [] {
                new("_Theme", new MenuItem [] {
                    new("Base","", () => { _window.ColorScheme = Colors.Base;Application.Refresh();}),
                    new("Dialog","", () => { _window.ColorScheme = Colors.Dialog;Application.Refresh(); }),
                    new("Error","", () =>
                    {
                        _window.ColorScheme = Colors.Error;Application.Refresh();
                    }),
                    new("TopLevel","", () => { _window.ColorScheme = Colors.TopLevel; Application.Refresh();}),

                }),
                new("_About","...",action:About)
            });
            top.Add (menu);
            
            var optionList = new ustring[_options.Trojan.Count];
            
            for (int i = 0; i < optionList.Length; i++)
            {
                optionList[i] =$"{ _options.Trojan[i].Remark}({_options.Trojan[i].Host})";
            }
            
            var proxyMode = new RadioGroup(3, 5, new ustring[] {"PAC", "GLOBAL"});
            var nodeList = new RadioGroup(3, 11, optionList);

            nodeList.SelectedItemChanged+= NodeListOnSelectedItemChanged;
            proxyMode.SelectedItemChanged += ProxyModeOnSelectedItemChanged;
            
            // Add some controls, 
            _window.Add (
                // The ones with my favorite layout system, Computed
                new Label (3, 3, "Node List:"),
                nodeList,
                new Label (3, 9, "Proxy Mode:"),
                // The ones laid out like an australopithecus, with Absolute positions:
                proxyMode
                );

            Application.Run ();
        }

        private void NodeListOnSelectedItemChanged(RadioGroup.SelectedItemChangedArgs obj)
        {
            _trojanContext.SetUseIndex(obj.SelectedItem);
        }

        private void ProxyModeOnSelectedItemChanged(RadioGroup.SelectedItemChangedArgs obj)
        {
            if (obj.SelectedItem==0)
            {
                _proxySetting.SetPacProxy(Helper.GetPacAddress(_options.PacServerPort));
            }
            else if (obj.SelectedItem==1)
            {
                _proxySetting.SetGlobalProxy("127.0.0.1:" + _options.HttpProxyPort);
            }
        }

        
        private void About()
        {
            var button = new Button("OK");
   
            var about =  new Dialog("ABOUT", 60, 20, button);
            var label = new Label("FAY PROXY") {X = Pos.Center(), Y = 2};
            about.Add(label);
            
            var label2 = new Label("Just for learning,and compliance with your local laws") {X = 2, Y = 6};
            about.Add(label2);
            
            var label3 = new Label("Thanks @jang and projects:") {X = 2, Y = 8};
            about.Add(label3);
            var label4 = new TextField("https://github.com/migueldeicaza/gui.cs") {X = 2, Y = 10};
            about.Add(label4);

            
            /*var label3 = new Label("Project Url") {X = 3, Y = 6};
            about.Add(label3);*/

            button.Clicked += () =>
            {
                _window.Remove(about);
            };
            _window.Add(about);
        }

        private void SetTheme()
        {
            
        }
    }
}