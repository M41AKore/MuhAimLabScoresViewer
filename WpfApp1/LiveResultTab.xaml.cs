using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for LiveTrackerTab.xaml
    /// </summary>
    public partial class LiveResultTab : Page
    {
        public LiveResultTab()
        {
            InitializeComponent();
            DataContext = MainWindow.viewModel;
        }

        
    }
}
