using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;

namespace DS
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string GetFloder()
        {
            string folderPath = Path.Combine(Environment.CurrentDirectory, "Documentos Assinado");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }


        private void OpenPdfButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Arquivos PDF (*.pdf)|*.pdf";
        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        openFileDialog.Title = "Selecione um documento PDF";

        if (openFileDialog.ShowDialog() == true)
        {
                string selectedFilePath = openFileDialog.FileName;              
                string documentoAssinado = $"{GetFloder()}-Assinado.pdf";               
                SingletonCertificate certificate = SingletonCertificate.InstanceCertificate;
                var chosenCertificate =  certificate.GetCertificate();
                certificate.SignPDF(selectedFilePath, documentoAssinado, chosenCertificate);

                Process.Start(documentoAssinado);
        }
            
    }

}
}
