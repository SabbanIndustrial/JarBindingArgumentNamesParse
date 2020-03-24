using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace JarBindingArgumentNamesParse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public string ProjectDirectory { get; set; }

        public List<string> FileExtensions { get => ExtensionsTextBox.Text.Split('.').ToList(); }

        private void OpenDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            //C:\Users\Acer\Desktop\SmartPosLib.jar.src\com\centerm\smartpos\aidl
            string path = SaveFileDialog();
            if (string.IsNullOrWhiteSpace(path))
            {
                Title = "Error selecting directory";
                return;
            }
            ProjectDirectory = path;// System.IO.Path.GetDirectoryName(path);
            Title = $"Working directory: {ProjectDirectory}";
        }

        private string SaveFileDialog()
        {
            var dialog = new SaveFileDialog();
            //dialog.InitialDirectory = textbox.Text; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                // Our final value is in path
                return path;
            }
            return null;
        }
        


        private void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            //<attr path="/api/package[@name='com.centerm.smartpos.aidl.pinpad']/interface[@name='AidlPinPad']/method[@name='getCheckValue']/parameter[@name='p0']" name="managedName">retLen</attr>

            if (!string.IsNullOrWhiteSpace(ProjectDirectory))
            {
                int filesCount = 0;
                int methodsCount = 0;
                int argumentsCount = 0;

                foreach (var item in FileExtensions)
                {
                    string[] files = Directory.GetFiles(ProjectDirectory, $"*.{item}", SearchOption.AllDirectories);
                    filesCount = files.Length;
                    foreach (var item2 in files)
                    {

                        string text = File.ReadAllText(item2);

                        //remove all comments
                        text = Regex.Replace(text, "\\/\\*([\\S\\s]*?)\\*\\/", "");
                        //remove argument wraps
                        text = Regex.Replace(text, ",\\n", "");
                        //tabs
                        text = Regex.Replace(text, "\\t", "");
                        //random spaces
                        text = Regex.Replace(text, " {2,10}", "");



                        string package = Regex.Match(text, "(?<=package)(.*?)(?=;)").Value.Trim();


                        string inter = Regex.Match(text, "(?<=interface)(.*?)(?={)").Value.Trim();


                        //parse all method signatures
                        foreach (var signature in Regex.Matches(text, "(?<=\\S{1}) (.*?)\\);"))
                        {
                            methodsCount++;
                            string method = signature.ToString().Replace("\r", "").Replace("\n", "").Replace("\t", "");


                            string methodName = Regex.Match(method, "(?<= )(.*?)(?=\\()").Value;


                            //parse arguments
                            string args = Regex.Match(method, "(?<=\\()(.*?)(?=\\))").Value + ",";

                            args = args.Replace(", ", ",");
                            var argList = args.Split(',');

                            int pn = 0;
                            foreach (var arg in argList)
                            {
                                string clnarg = arg.Trim();
                                if (string.IsNullOrWhiteSpace(clnarg))
                                {
                                    continue;
                                }
                                argumentsCount++;
                                string argName = clnarg.Split(' ').Last();
                                //keyword in c#
                                if (argName == "params")
                                {
                                    argName = "args";
                                }
                                //compose final metadata.xml attribute
                                //in my case parameters were called p0, p1... by default
                                string result = $"<attr path=\"/api/package[@name='{package}']/interface[@name='{inter}']/method[@name='{methodName}']/parameter[@name='p{pn}']\" name=\"managedName\">{argName}</attr>";

                                if (string.IsNullOrWhiteSpace(argName))
                                {
                                    string stop = "";

                                }
                                TextPatternTextBox.Text += $"{result}\n";
                                pn++;
                            }



                        }
                    }
                }
                textBlock.Text = $"Files found: {filesCount}\n" +
                    $"Methods found: {methodsCount}\n" +
                    $"Arguments found: {argumentsCount}\n";
            }
        }
    }
}
