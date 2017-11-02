using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TLSharp.Core;
using Animator;

namespace Telesharp
{
    public partial class LoginWindow : Window
    {
        const double animationDuration = 200;
        bool codeSent;

        public LoginWindow()
        {
            InitializeComponent();
        }

        async void nextClick(object sender, RoutedEventArgs e)
        {
            nextButton.IsEnabled = false;

            if (Telegram.IsUserAuthorized())
            {
                new MainWindow().Show();
                Close();
                return;
            }
            if (codeSent)
            {
                inputCodePanel.IsEnabled = false;

                if (await Telegram.MakeAuth(telegramCode.Text))
                {
                    animate(inputPhonePanel, successPanel);
                    nextButton.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Wrong code. Please make sure you've input a valid code",
                        "Login failed", MessageBoxButton.OK, MessageBoxImage.Error);

                    inputCodePanel.IsEnabled = nextButton.IsEnabled = true;
                }
            }
            else
            {
                inputPhonePanel.IsEnabled = false;

                Telegram.Phone = phoneCode.Text + phoneNumber.Text;
                await Telegram.Connect();
                if (await Telegram.SendCodeRequest())
                {
                    animate(inputPhonePanel, inputCodePanel);
                    codeSent = true;
                }
                else
                {
                    MessageBox.Show("Could not login. Please make sure you've input the correct number",
                        "Login failed", MessageBoxButton.OK, MessageBoxImage.Error);

                    inputPhonePanel.IsEnabled = nextButton.IsEnabled = true;
                }
            }
        }

        void phoneTextChanged(object sender, TextChangedEventArgs e) => checkNextButton();
        void telegramCodeTextChanged(object sender, TextChangedEventArgs e) => checkNextButton();
        void checkNextButton()
        {
            if (codeSent)
            {
                nextButton.IsEnabled = telegramCode.Text.Length == 5;
            }
            else
            {
                nextButton.IsEnabled =
                    !string.IsNullOrEmpty(phoneCode.Text) &&
                    !string.IsNullOrEmpty(phoneNumber.Text);
            }
        }

        void animate(FrameworkElement hide, FrameworkElement show)
        {
            var oldHeight = hide.ActualHeight;
            hide.ResizeHeight(oldHeight, 0, animationDuration);
            show.ResizeHeight(0, oldHeight, animationDuration);
        }
    }
}
