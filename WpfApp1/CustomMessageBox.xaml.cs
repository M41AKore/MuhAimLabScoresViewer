using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        MessageBoxButtons MessageBoxButtons = MessageBoxButtons.OK;

        public CustomMessageBox()
        {
            InitializeComponent();
        }

        private void btn_right_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn.Content.ToString() == "OK") this.Close();
        }

        public void setLayout(MessageBoxButtons layout)
        {
            //set buttons 
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBoxResult.None;

            /* switch(MessageBoxButtons)
             {
                 case MessageBoxButtons.OK:
                     return MessageBoxResult.OK;
             }*/
            //return msgbox result

            MainWindow.currentMsgBox = null;
        }
    }
}
