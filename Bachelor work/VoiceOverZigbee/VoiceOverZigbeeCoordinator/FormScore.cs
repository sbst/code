using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
//пространство имен сервера по передаче голоса
namespace VoiceOverZigbeeCoordinator
{
    //форма оценки качества
    public partial class FormScore : Form
    {
        //конструктор по умолчанию
        public FormScore()
        {
            InitializeComponent();  //инициализируем интерфейс
            comboBoxScore.SelectedIndex = 0;    //значение в combobox по умолчанию
        }
        //метод возвращающий значение combobox
        public string Data
        {
            get
            {
                return comboBoxScore.Text;
            }
        }
        //событие закрытия главной формы
        private void FormScore_FormClosing(object sender, FormClosingEventArgs e)
        {
            //если попытка закрытия формы не выбрав оценку    
            if ((e.CloseReason == CloseReason.UserClosing)&&(comboBoxScore.SelectedIndex == 0))
                    e.Cancel = true;
        }
        //щелчок на кнопку при выборе, если выбрана оценка то закрываем форму
        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (comboBoxScore.SelectedIndex != 0)
                this.Close();
        }
    }
}
