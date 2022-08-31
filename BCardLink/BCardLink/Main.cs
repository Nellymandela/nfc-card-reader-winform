using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BCardLink
{
    public partial class Main : Form
    {
        private SmartCardReader reader;
        private MiFareCard card;

        public Main()
        {
            InitializeComponent();
            GetDevices();
        }

        private void CardRemoved(object sender, EventArgs e)
        {
            card?.Dispose();
            card = null;

            var ignored = this.BeginInvoke((Action)(() =>
            {
                lblReaderIndicator.BackColor = Color.Red;
                WriteMessage("Place card on the reader to scan.");
                groupBox1.Visible = false;
            }));
        }

        private async void CardAdded(object sender, CardEventArgs args)
        {
            try
            {
                card = args.SmartCard.CreateMiFareCard();

                var cardIdentification = await card.GetCardInfo();
                var uid = await card.GetUid();

                var ignored = this.BeginInvoke((Action)(() =>
                {
                    lblReaderIndicator.BackColor = Color.Green;
                    WriteMessage("Card detected. Do not remove while linking.");
                    groupBox1.Visible = true;
                    txtUID.Text = BitConverter.ToString(uid);
                }));
            }
            catch (Exception ex)
            {
                PopupMessage("CardAdded Exception: " + ex.Message);
            }
        }

        private void cboDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeactivateDevice();
            ConnectToDevice(cboDevices.Text);
        }

        private void GetDevices()
        {
            try
            {

                IReadOnlyList<string> readers = CardReader.GetReaderNames();

                cboDevices.Items.AddRange(readers.ToArray());
            }
            catch (Exception e)
            {
                PopupMessage("Exception: " + e.Message);
            }
        }

        private async void ConnectToDevice(string name)
        {
            try
            {
                reader = await CardReader.FindAsync(name);
                if (reader == null)
                {
                    PopupMessage("No Readers Found");
                    return;
                }

                reader.CardAdded += CardAdded;
                reader.CardRemoved += CardRemoved;
                WriteMessage("Place card on the reader to scan.");
            }
            catch (Exception e)
            {
                PopupMessage("Exception: " + e.Message);
            }
        }

        public void PopupMessage(string message)
        {
            var ignored = this.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(message);
            }));
        }

        public void WriteMessage(string message)
        {
            var ignored = this.BeginInvoke((Action)(() =>
            {
               lblMessage.Text =  message;
            }));
        }

        private void DeactivateDevice()
        {
            if (reader == null)
            {
                return;
            }
            reader.CardAdded -= CardAdded;
            reader.CardRemoved -= CardRemoved;
            reader.Dispose();
            reader = null;
        }
    }
}
