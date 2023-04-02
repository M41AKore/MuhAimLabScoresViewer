using System;
using System.Windows;
using System.Windows.Forms;
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
            if(sender is Button btn)
            {
                if (btn.Content.ToString() == "OK") this.Close();
            }                     
        }

        public void setText(string s)
        {
            TxtOutput.Text = s;
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
