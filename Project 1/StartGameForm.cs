namespace Project_1;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

public class StartGameForm : Form
{
    private readonly Button _startButton;

    public StartGameForm()
    {
        Text = "Healer - Launcher";
        ClientSize = new Size(400, 200);
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = true;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        _startButton = new Button
        {
            Text = "Start Game",
            Anchor = AnchorStyles.None,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(20, 10, 20, 10)
        };
        _startButton.Click += StartButton_Click;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.Controls.Add(_startButton, 0, 0);

        Controls.Add(layout);
    }

    private void StartButton_Click(object? sender, EventArgs e)
    {
        _startButton.Enabled = false;

        try
        {
            var executablePath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                executablePath = Application.ExecutablePath;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = "--run-game",
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory
            };

            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Unable to start the game process.");
            }

            Close();
        }
        catch (Exception ex)
        {
            _startButton.Enabled = true;
            MessageBox.Show(this, $"Failed to start the game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
