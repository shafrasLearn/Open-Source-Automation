﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace OSAE.UI.Controls
{
    /// <summary>
    /// Interaction logic for NavigationImage.xaml
    /// </summary>
    public partial class UserSelector : UserControl
    {
        public OSAEObject screenObject { get; set; }
        public Point Location;
        public string _CurrentUser = "";
        public int _CurrentUserTrust = 0;
        public string _AppName = "";
        public string _ScreenLocation = "";
        private OSAEImageManager imgMgr = new OSAEImageManager();
        private string _pwbuff = "";
        private string _usersPIN = "";
        private bool loadingFlag = true;
        public UserSelector(OSAEObject sObject, string appName)
        {
            InitializeComponent();
            userGrid.Height = 25;
            _AppName = appName;
            screenObject = sObject;
            cboUsers.Items.Add("Log In/Out");
            cboUsers.SelectedIndex = 1;

            string currentUser = OSAE.OSAEObjectPropertyManager.GetObjectPropertyValue(_AppName, "Current User").Value;

            cboUsers.Background = new SolidColorBrush(Colors.Yellow);

            OSAEObjectCollection userList = OSAEObjectManager.GetObjectsByType("PERSON");
            foreach (OSAE.OSAEObject obj in userList)
            {
                cboUsers.Items.Add (obj.Name);
                if (obj.Name == currentUser)
                {
                    cboUsers.SelectedIndex = cboUsers.Items.Count - 1;
                    _CurrentUser = currentUser;
                    _CurrentUserTrust = Convert.ToUInt16(OSAEObjectPropertyManager.GetObjectPropertyValue(_CurrentUser, "Trust Level").Value);
                    cboUsers.Background = new SolidColorBrush(Colors.Green);

                    // Add Remember me option
                    ContextMenu ctmRembMe = new ContextMenu();
                    MenuItem miRembMe = new MenuItem();
                    miRembMe.Name = "rememberMode";
                    miRembMe.IsCheckable = true;
                    miRembMe.Checked += rememberMode_Checked;
                    miRembMe.Unchecked += rememberMode_Unchecked;
                    miRembMe.Header = "LogOut on Close";
                    string remUser = OSAE.OSAEObjectPropertyManager.GetObjectPropertyValue(_AppName, "LogOut on Close").Value;
                    bool loUser = Convert.ToBoolean(remUser);
                    if (loUser == true)
                    {
                        miRembMe.IsChecked = true;
                    }
                    else
                    {
                        miRembMe.IsChecked = false;
                    }
                    ctmRembMe.Items.Add(miRembMe);
                    cboUsers.ContextMenu = ctmRembMe;
                }
            }
            
            

            // string imgName = screenObject.Property("Image").Value;
            // OSAEImage img = imgMgr.GetImage(imgName);

            //  if (img != null)
            // DataSet dataSet = OSAESql.RunSQL("SELECT state_name FROM osae_v_object_state where object_name = '" + cboObject.SelectedValue + "' order by state_name");
            //cboState1.ItemsSource = dataSet.Tables[0].DefaultView;

            loadingFlag = false;
        }

        private void cboUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loadingFlag) return;

            _CurrentUser = "";
            _CurrentUserTrust = 0;
            _pwbuff = "";
            _usersPIN = "";
            lblPIN.Content = "";
            if (cboUsers.SelectedIndex == 1)
            {
                userGrid.Height = 25;
                cboUsers.Background = new SolidColorBrush(Colors.Yellow);
                OSAE.OSAEObjectPropertyManager.ObjectPropertySet(_AppName, "Current User", "", "SYSTEM");
                cboUsers.ContextMenu = null;
                OSAE.OSAEObjectPropertyManager.ObjectPropertySet(_AppName, "LogOut on Close", "TRUE", _CurrentUser);
            }
            else
            {
                //Load user's PIN
                _usersPIN = OSAE.OSAEObjectPropertyManager.GetObjectPropertyValue(cboUsers.SelectedItem.ToString(), "PIN").Value;
                if (_usersPIN == "")
                {
                    userGrid.Height = 25;
                    cboUsers.Background = new SolidColorBrush(Colors.Green);
                    _CurrentUserTrust = Convert.ToUInt16(OSAEObjectPropertyManager.GetObjectPropertyValue(cboUsers.SelectedItem.ToString(), "Trust Level").Value);
                    _CurrentUser = cboUsers.SelectedItem.ToString();
                    OSAE.OSAEObjectPropertyManager.ObjectPropertySet(_AppName, "Current User", _CurrentUser, "SYSTEM");

                    // Add Remember me option
                    ContextMenu ctmRembMe = new ContextMenu();
                    MenuItem miRembMe = new MenuItem();
                    miRembMe.Name = "rememberMode";
                    miRembMe.IsCheckable = true;
                    miRembMe.Checked += rememberMode_Checked;
                    miRembMe.Unchecked += rememberMode_Unchecked;
                    miRembMe.Header = "LogOut on Close";
                    string remUser = OSAE.OSAEObjectPropertyManager.GetObjectPropertyValue(_AppName, "LogOut on Close").Value;
                    bool loUser = Convert.ToBoolean(remUser);
                    if (loUser == true)
                    {
                        miRembMe.IsChecked = true;
                    }
                    else
                    {
                        miRembMe.IsChecked = false;
                    }
                    ctmRembMe.Items.Add(miRembMe);
                    cboUsers.ContextMenu = ctmRembMe;

                }
                else
                    userGrid.Height = 184;
            }
        }

        private void Number_Pressed(string _number)
        {
            _pwbuff += _number;
            if (_pwbuff == _usersPIN)
            {
                userGrid.Height = 25;
                cboUsers.Background = new SolidColorBrush(Colors.Green);
                _CurrentUserTrust = Convert.ToUInt16(OSAEObjectPropertyManager.GetObjectPropertyValue(cboUsers.SelectedItem.ToString(), "Trust Level").Value);
                _CurrentUser = cboUsers.SelectedItem.ToString();
                //For a User to use a screen, they are oviously here and in the same room as the screen
                OSAEObject oUser = OSAE.OSAEObjectManager.GetObjectByName(_CurrentUser);
                if (oUser.Container != _ScreenLocation)
                    OSAE.OSAEObjectManager.ObjectUpdate(oUser.Name, oUser.Name, oUser.Alias, oUser.Description,oUser.Type, oUser.Address, _ScreenLocation,oUser.MinTrustLevel,oUser.Enabled);

                OSAE.OSAEObjectPropertyManager.ObjectPropertySet(_AppName, "Current User", _CurrentUser, _CurrentUser);

                // Add Remember me option
                ContextMenu ctmRembMe = new ContextMenu();
                MenuItem miRembMe = new MenuItem();
                miRembMe.Name = "rememberMode";
                miRembMe.IsCheckable = true;
                miRembMe.Checked += rememberMode_Checked;
                miRembMe.Unchecked += rememberMode_Unchecked;
                miRembMe.Header = "LogOut on Close";
                string remUser = OSAE.OSAEObjectPropertyManager.GetObjectPropertyValue(_AppName, "LogOut on Close").Value;
                bool loUser = Convert.ToBoolean(remUser);
                if (loUser == true)
                {
                    miRembMe.IsChecked = true;
                }
                else
                {
                    miRembMe.IsChecked = false;
                }
                ctmRembMe.Items.Add(miRembMe);
                cboUsers.ContextMenu = ctmRembMe;
            }
            lblPIN.Content  += "* ";
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("1");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("2");
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("3");
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("4");
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("5");
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("6");
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("7");
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("8");
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("9");
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            _pwbuff = "";
            lblPIN.Content = "";
        }

        private void button0_Click(object sender, RoutedEventArgs e)
        {
            Number_Pressed("0");
        }

        private void buttonGo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rememberMode_Checked(object sender, RoutedEventArgs e)
        {
            OSAE.OSAEObjectPropertyManager.ObjectPropertySet(_AppName, "LogOut on Close", "TRUE", _CurrentUser);
        }

        private void rememberMode_Unchecked(object sender, RoutedEventArgs e)
        {
            OSAE.OSAEObjectPropertyManager.ObjectPropertySet(_AppName, "LogOut on Close", "FALSE", _CurrentUser);
        }
    }
}