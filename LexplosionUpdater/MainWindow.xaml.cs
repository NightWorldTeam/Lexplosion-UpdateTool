using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Linq;

namespace NWUpdater
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] _args;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { DragMove(); };

            try
            {
                _args = Environment.GetCommandLineArgs();

                if (_args.Length > 6) // эта хуйня нужна потому что в лаунчере была ошибка с передачей аргументов
                {
                    string[] newArgs = new string[6];

                    string path = "";
                    int i;
                    for (i = 1; i < _args.Length; i++)
                    {
                        if (!_args[i].StartsWith("https://"))
                        {
                            path += _args[i] + " ";
                        }
                        else
                        {
                            break;
                        }
                    }

                    newArgs[1] = path.Remove(path.Length - 1, 1);
                    newArgs[2] = _args[i];

                    string processName = "";
                    for (int j = i + 1; j < _args.Length - 2; j++)
                    {
                        processName += _args[j] + " ";
                    }

                    newArgs[3] = processName.Remove(processName.Length - 1, 1);
                    newArgs[4] = _args[_args.Length - 2];
                    newArgs[5] = _args[_args.Length - 1];

                    _args = newArgs;
                }

                Left = Int32.Parse(_args[4]);
                Top = Int32.Parse(_args[5]);

                new Thread(Update).Start();
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void Update()
        {
            try
            {

                if (int.TryParse(_args[3], out int id)) // новая версия лаунчера передает pid
                {
                    Process.GetProcessById(id).Kill();
                }
                else
                {
                    foreach (var process in Process.GetProcessesByName(_args[3])) //старая передает имя процесса
                    {
                        process.Kill();
                    }
                }

                WebClient wc = new WebClient();
                byte[] fileBytes = wc.DownloadData(_args[2]);
                using (FileStream fstream = new FileStream(_args[1], FileMode.Truncate))
                {
                    fstream.Write(fileBytes, 0, fileBytes.Length);
                    fstream.Close();
                }

                Process proc = new Process();
                proc.StartInfo.FileName = _args[1];
                proc.Start();

            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        private void ErrorLog(Exception ex)
        {
            MessageBox.Show("Не удалось обновить лаунчер!");
            try
            {
                string name = Environment.ExpandEnvironmentVariables("%appdata%") + "/lexplosion-data/updater-crash-report_" + DateTime.Now.ToString("dd.MM.yyyy-h.mm.ss") + ".log";
                string dirName = Path.GetDirectoryName(name);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                using (FileStream fstream = new FileStream(name, FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(ex.ToString() + "\r\n\r\nArguments: '" + string.Join(" ", _args ?? new string[0]) + "'");
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }
            }
            catch
            {
            }

            Environment.Exit(0);
        }
    }
}
