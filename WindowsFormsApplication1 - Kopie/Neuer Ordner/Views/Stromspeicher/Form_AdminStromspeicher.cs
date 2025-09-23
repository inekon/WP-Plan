﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form_AdminStromspeicher : Form
    {
        OdbcCommand DBCommand;
        StromspeicherModel model = new StromspeicherModel();
        public List<WErzeugerModel> list_spmodel = new List<WErzeugerModel>();
        public bool m_bItemBearbeiten = false;
        private bool m_Neu = false;

        public Form_AdminStromspeicher()
        {
            InitializeComponent();
            DBCommand = Program.DBConnection.CreateCommand();
        }
        
        public void SetControls(string projekt)
        {
            listBox_Stromspeicher.Items.Clear();
            for (int i = 0; i < list_spmodel.Count; i++)
            {
                listBox_Stromspeicher.Items.Add(list_spmodel[i].Bezeichner);
            }
            if (listBox_Stromspeicher.Items.Count > 0) listBox_Stromspeicher.SelectedIndex = 0;
        }

        private void btn_Beenden_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_Abbruch_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form_Stromspeicher_Load(object sender, EventArgs e)
        {
            if (m_bItemBearbeiten) return;

            RecordSet rs = new RecordSet();
            rs.Open("SELECT * FROM Tab_Stromspeicher");   
            
            if(!rs.EOF())
            {
                while (rs.Next())
                {
                    string bezeichner = rs.Read("Bezeichner").ToString();
                    Console.WriteLine("Bezeichner: {bezeichner}");
                    listBox_Stromspeicher.Items.Add(bezeichner);
                }
                listBox_Stromspeicher.SelectedIndex = 0;
            }
        }

        private TextBox GetTextBox_Energie()
        {
            return textBox_Energie;
        }
        
        private void btn_Speichern_Click(object sender, EventArgs e)
        {
            if (textBox_Degradation.Text == "" || textBox_Energie.Text == "" || textBox_Ladezustand.Text == "" ||
                textBox_Leistung.Text == "" || textBox_Typ.Text == "")
            {
                MessageBox.Show("Eingaben überprüfen!");
                return;
            }

            try
            {
                if (m_Neu)
                {
                    DBCommand.CommandText = "INSERT INTO TAB_Stromspeicher ( Bezeichner, Typ, Leistung, Energie, Degradation, Ladezustand ) SELECT '" + textBox_Bezeichner.Text +
                        "' AS Ausdr1, '" + textBox_Typ.Text + "' AS Ausdr2, " + textBox_Leistung.Text + " AS Ausdr3, " + textBox_Energie.Text + " AS Ausdr4, " + textBox_Degradation.Text + " AS Ausdr5, " +
                    textBox_Ladezustand.Text + " AS Ausdr6";

                    DBCommand.ExecuteNonQuery();
                    listBox_Stromspeicher.Items.Add(textBox_Bezeichner.Text);
                    listBox_Stromspeicher.SelectedIndex = listBox_Stromspeicher.Items.Count - 1;
                }
                else
                {
                    DBCommand.CommandText = "UPDATE Tab_Stromspeicher SET Bezeichner='" + textBox_Bezeichner.Text + "' , Typ='" + textBox_Typ.Text + "', Leistung=" + textBox_Leistung.Text + ", Energie=" + textBox_Energie.Text + ", Degradation=" + textBox_Degradation.Text + ", Ladezustand=" + textBox_Ladezustand.Text + " WHERE Bezeichner='" + listBox_Stromspeicher.Text + "'";
                    DBCommand.ExecuteNonQuery();
                }
            }
            catch (OdbcException sqlEx)
            {
                // Fehler beim Datenbankzugriff abfangen
                Console.WriteLine("SQL Fehler: " + sqlEx.Message);
                m_Neu = false;
                InitControls();
                return;
            }
            catch (Exception ex)
            {
                // Allgemeine Fehler abfangen
                Console.WriteLine("Allgemeiner Fehler: " + ex.Message);
                m_Neu = false;
                InitControls();
                return;
            }
            return;
        }

        private void listBox_Stromspeicher_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecordSet rs = new RecordSet();

            textBox_Bezeichner.Text = listBox_Stromspeicher.Text;
            model.m_szBezeichner = textBox_Bezeichner.Text;
            
            rs.Open("SELECT * FROM Tab_Stromspeicher where Bezeichner='" + listBox_Stromspeicher.Text + "'");

            if (!rs.EOF())
            {
                textBox_Energie.Text = rs.Read("Energie").ToString();
                model.m_Energie = Int32.Parse(textBox_Energie.Text);
                textBox_Leistung.Text = rs.Read("Leistung").ToString();
                model.m_Energie = Int32.Parse(textBox_Energie.Text);

                textBox_Typ.Text = (string)rs.Read("Typ");
                model.m_szTyp = textBox_Typ.Text;
                textBox_Degradation.Text = rs.Read("Degradation").ToString();
                model.m_Degradation = Double.Parse(textBox_Degradation.Text);
                textBox_Ladezustand.Text = rs.Read("Ladezustand").ToString();
                model.m_Ladezustand = Int32.Parse(textBox_Ladezustand.Text);
                textBox_Bezeichner.Text = rs.Read("Bezeichner").ToString();
                model.m_szBezeichner = textBox_Bezeichner.Text;
            }

        }

        private void btn_Neu_Click(object sender, EventArgs e)
        {
            InitControls();
            Form_Sp_ItemNeu frm = new Form_Sp_ItemNeu();
            frm.ShowDialog();
            if (frm.result == DialogResult.OK)
            {
                m_Neu = true;
                textBox_Bezeichner.Text = frm.m_szName;
            }
            return;
        }

        private void InitControls()
        {
            m_Neu = false;
            textBox_Bezeichner.Text = "";
            textBox_Typ.Text = "";
            textBox_Ladezustand.Text = "";
            textBox_Degradation.Text = "";
            textBox_Energie.Text = "";
            textBox_Leistung.Text = "";
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close(); 
        }

        private void btn_Loeschen_Click(object sender, EventArgs e)
        {
            if (listBox_Stromspeicher.SelectedIndex == -1)
            {
                MessageBox.Show("Stromspeicher in Liste auswählen!");
                return;
            }
            try
            {
                DBCommand.CommandText = "DELETE * from  Tab_Stromspeicher where Bezeichner='" + textBox_Bezeichner.Text + "'";
                DBCommand.ExecuteNonQuery();
                listBox_Stromspeicher.Items.Remove(textBox_Bezeichner.Text);
                listBox_Stromspeicher.SelectedIndex = listBox_Stromspeicher.Items.Count - 1;
            }
            catch (OdbcException sqlEx)
            {
                // Fehler beim Datenbankzugriff abfangen
                MessageBox.Show("Stromspeicher kann nicht gelöscht werden.\nEs besteht eine Projektzordnung!");  
                Console.WriteLine("SQL Fehler: " + sqlEx.Message);
                return;
            }
            catch (Exception ex)
            {
                // Allgemeine Fehler abfangen
                Console.WriteLine("Allgemeiner Fehler: " + ex.Message);
                return;
            }

        }

        private void textBox_Typ_Validating(object sender, CancelEventArgs e)
        {
            if (textBox_Typ.Text == "") { MessageBox.Show("Eingabe überprüfen!"); }
        }

        private void textBox_Leistung_Validating(object sender, CancelEventArgs e)
        {
            if (!Program.checkInt(textBox_Leistung, textBox_Leistung.Text)) { textBox_Leistung.Undo(); }
        }

        private void textBox_Energie_Validating(object sender, CancelEventArgs e)
        {
            if (!Program.checkInt(textBox_Energie, textBox_Energie.Text)) { textBox_Energie.Undo(); }
        }

        private void textBox_Ladezustand_Validating(object sender, CancelEventArgs e)
        {
            if (!Program.checkInt(textBox_Ladezustand, textBox_Ladezustand.Text)) { textBox_Ladezustand.Undo(); }
        }

        private void textBox_Degradation_Validating(object sender, CancelEventArgs e)
        {
            if (!Program.checkInt(textBox_Degradation, textBox_Degradation.Text)) { textBox_Degradation.Undo(); }
        }

    }
}
