using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Otimizador_de_vídeos
{
    public partial class Form1 : Form
    {

        Process process;

        public Form1() {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e) {

            textBox1.Clear();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {

                StatusTela(false);
                textBox1.AppendText("Iniciando processo de otimização\r\n");

                Task.Run(() => {

                    string inputFile = openFileDialog1.FileName;
                    string extension = Path.GetExtension(inputFile);
                    string tempFile = Path.GetTempFileName() + extension;
                    string outputFile = inputFile.Substring(0, inputFile.Length - extension.Length);
                    outputFile += "-otimizado-em-" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;

                    ProcessStartInfo statInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = Directory.GetCurrentDirectory() + @"\ffmpeg.exe",
                        Arguments = "-hide_banner -i " + EscapeFile(inputFile) + " " + EscapeFile(tempFile)
                    };

                    process = new Process
                    {
                        StartInfo = statInfo
                    };

                    process.ErrorDataReceived += process_OutputDataReceived;
                    process.OutputDataReceived += process_OutputDataReceived;
                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    process.CancelOutputRead();
                    process.CancelErrorRead();
                    int exitcode = process.ExitCode;
                    process = null;

                    StatusTela(true);

                    if (exitcode == 0) {
                        FileInfo infoInput = new FileInfo(inputFile);
                        FileInfo infoTemp = new FileInfo(tempFile);

                        if (infoInput.Length > infoTemp.Length) {
                            File.Move(tempFile, outputFile);
                            MessageBox.Show("Seu vídeo foi salvo na mesma pasta do vídeo original", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        } else {
                            MessageBox.Show("Desculpe, não conseguimos reduzir o tamanho do seu vídeo", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    } else {
                        MessageBox.Show("Não foi possível converter seu vídeo", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                });

            }
        }

        private string EscapeFile(string file) {
            return "\"" + ((string)file).Replace(@"\", @"\\").Replace("\"", @"\" + "\"") + "\"";
        }

        void StatusTela(bool ativo) {
            this.Invoke((MethodInvoker)delegate {
                button1.Enabled = ativo;
                button2.Enabled = !ativo;
                progressBar1.Style = ativo ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
            });
        }

        private void Form1_Load(object sender, EventArgs e) {
            StatusTela(true);
        }

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            this.Invoke((MethodInvoker)delegate {
                textBox1.AppendText(e.Data + "\r\n");
            });
        }

        private void button2_Click(object sender, EventArgs e) {
            process.Kill();
            button2.Enabled = false;
        }
    }
}