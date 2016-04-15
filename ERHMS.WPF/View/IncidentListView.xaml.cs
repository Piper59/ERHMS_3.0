﻿using ERHMS.WPF.ViewModel;
using System;
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

namespace ERHMS.WPF.View
{
    /// <summary>
    /// Interaction logic for IncidentListView.xaml
    /// </summary>
    public partial class IncidentListView : UserControl
    {
        public IncidentListView()
        {
            InitializeComponent();

            DataContext = new IncidentListViewModel();
        }
    }
}
