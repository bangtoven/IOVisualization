﻿#pragma checksum "..\..\EnvSetup.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6220A170D7D933966522CC55F481E0F2"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Framework.UI;
using Framework.UI.Commands;
using Framework.UI.Controls;
using Framework.UI.Converters;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ValtioClient {
    
    
    /// <summary>
    /// EnvSetup
    /// </summary>
    public partial class EnvSetup : Framework.UI.Controls.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\EnvSetup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox deviceList;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\EnvSetup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox traceLengthHour;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\EnvSetup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox traceLengthMin;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\EnvSetup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox timeWindow;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\EnvSetup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox blockUnit;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\EnvSetup.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button traceBtn;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ValtioClient;component/envsetup.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\EnvSetup.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.deviceList = ((System.Windows.Controls.ListBox)(target));
            
            #line 18 "..\..\EnvSetup.xaml"
            this.deviceList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.deviceList_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.traceLengthHour = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.traceLengthMin = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.timeWindow = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.blockUnit = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.traceBtn = ((System.Windows.Controls.Button)(target));
            
            #line 48 "..\..\EnvSetup.xaml"
            this.traceBtn.Click += new System.Windows.RoutedEventHandler(this.traceBtn_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

