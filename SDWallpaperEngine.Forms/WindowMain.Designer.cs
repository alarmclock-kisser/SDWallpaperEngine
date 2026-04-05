namespace SDWallpaperEngine.Forms
{
    partial class WindowMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            checkBox_enabled = new CheckBox();
            numericUpDown_interval = new NumericUpDown();
            label_info_interval = new Label();
            label_info_batch = new Label();
            numericUpDown_batch = new NumericUpDown();
            label_info_steps = new Label();
            numericUpDown_steps = new NumericUpDown();
            pictureBox_preview = new PictureBox();
            label_info_maxImages = new Label();
            numericUpDown_maxImages = new NumericUpDown();
            button_openOutput = new Button();
            listBox_log = new ListBox();
            label_info_denoise = new Label();
            numericUpDown_denoise = new NumericUpDown();
            checkBox_record = new CheckBox();
            numericUpDown_gifDelay = new NumericUpDown();
            label_info_gifDelay = new Label();
            button_reset = new Button();
            button_comfyUi = new Button();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openOutputFolderToolStripMenuItem = new ToolStripMenuItem();
            cloarOutputFolderToolStripMenuItem = new ToolStripMenuItem();
            optionsToolStripMenuItem = new ToolStripMenuItem();
            saveFramesToolStripMenuItem = new ToolStripMenuItem();
            setOutputFolderToolStripMenuItem = new ToolStripMenuItem();
            toolStripTextBox_outputFolder = new ToolStripTextBox();
            editAppsettingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            comfyUIAPIUrlToolStripMenuItem = new ToolStripMenuItem();
            comfyUIAPIKeyToolStripMenuItem = new ToolStripMenuItem();
            comfyUIEXEPathToolStripMenuItem = new ToolStripMenuItem();
            comfyUIWorkflowPathToolStripMenuItem = new ToolStripMenuItem();
            toolStripTextBox_comfyUiApiUrl = new ToolStripTextBox();
            toolStripTextBox_comfyUiApiKey = new ToolStripTextBox();
            toolStripTextBox_comfyUiExePath = new ToolStripTextBox();
            toolStripTextBox_comfyUiWorkflowPath = new ToolStripTextBox();
            comfyUiLaunchArgumentsToolStripMenuItem = new ToolStripMenuItem();
            toolStripTextBox_comfyUiLaunchArguments = new ToolStripTextBox();
            positivePromptToolStripMenuItem = new ToolStripMenuItem();
            negativePromptToolStripMenuItem = new ToolStripMenuItem();
            outputDirectoryPathToolStripMenuItem = new ToolStripMenuItem();
            maxKeepAmountToolStripMenuItem = new ToolStripMenuItem();
            maxTimeoutsecToolStripMenuItem = new ToolStripMenuItem();
            maxConnectRetriesToolStripMenuItem = new ToolStripMenuItem();
            toolStripTextBox_positivePrompt = new ToolStripTextBox();
            toolStripTextBox_negativePrompt = new ToolStripTextBox();
            toolStripTextBox_maxKeepAmount = new ToolStripTextBox();
            toolStripTextBox_maxTimeout = new ToolStripTextBox();
            toolStripTextBox_maxRetries = new ToolStripTextBox();
            toolStripTextBox_outputDirectory = new ToolStripTextBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_interval).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_batch).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_steps).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_preview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_maxImages).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_denoise).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_gifDelay).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // checkBox_enabled
            // 
            checkBox_enabled.AutoSize = true;
            checkBox_enabled.Location = new Point(304, 441);
            checkBox_enabled.Name = "checkBox_enabled";
            checkBox_enabled.Size = new Size(68, 19);
            checkBox_enabled.TabIndex = 0;
            checkBox_enabled.Text = "Enabled";
            checkBox_enabled.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_interval
            // 
            numericUpDown_interval.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericUpDown_interval.Location = new Point(297, 32);
            numericUpDown_interval.Maximum = new decimal(new int[] { 30000, 0, 0, 0 });
            numericUpDown_interval.Minimum = new decimal(new int[] { 25, 0, 0, 0 });
            numericUpDown_interval.Name = "numericUpDown_interval";
            numericUpDown_interval.Size = new Size(75, 23);
            numericUpDown_interval.TabIndex = 1;
            numericUpDown_interval.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // label_info_interval
            // 
            label_info_interval.AutoSize = true;
            label_info_interval.Location = new Point(239, 34);
            label_info_interval.Name = "label_info_interval";
            label_info_interval.Size = new Size(52, 15);
            label_info_interval.TabIndex = 2;
            label_info_interval.Text = "Interval: ";
            // 
            // label_info_batch
            // 
            label_info_batch.AutoSize = true;
            label_info_batch.Location = new Point(251, 63);
            label_info_batch.Name = "label_info_batch";
            label_info_batch.Size = new Size(40, 15);
            label_info_batch.TabIndex = 4;
            label_info_batch.Text = "Batch:";
            // 
            // numericUpDown_batch
            // 
            numericUpDown_batch.Location = new Point(297, 61);
            numericUpDown_batch.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numericUpDown_batch.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown_batch.Name = "numericUpDown_batch";
            numericUpDown_batch.Size = new Size(75, 23);
            numericUpDown_batch.TabIndex = 3;
            numericUpDown_batch.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label_info_steps
            // 
            label_info_steps.AutoSize = true;
            label_info_steps.Location = new Point(253, 92);
            label_info_steps.Name = "label_info_steps";
            label_info_steps.Size = new Size(38, 15);
            label_info_steps.TabIndex = 6;
            label_info_steps.Text = "Steps:";
            // 
            // numericUpDown_steps
            // 
            numericUpDown_steps.Location = new Point(297, 90);
            numericUpDown_steps.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numericUpDown_steps.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown_steps.Name = "numericUpDown_steps";
            numericUpDown_steps.Size = new Size(75, 23);
            numericUpDown_steps.TabIndex = 5;
            numericUpDown_steps.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // pictureBox_preview
            // 
            pictureBox_preview.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_preview.Location = new Point(12, 194);
            pictureBox_preview.Name = "pictureBox_preview";
            pictureBox_preview.Size = new Size(360, 240);
            pictureBox_preview.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_preview.TabIndex = 7;
            pictureBox_preview.TabStop = false;
            // 
            // label_info_maxImages
            // 
            label_info_maxImages.AutoSize = true;
            label_info_maxImages.Location = new Point(12, 34);
            label_info_maxImages.Name = "label_info_maxImages";
            label_info_maxImages.Size = new Size(79, 15);
            label_info_maxImages.TabIndex = 9;
            label_info_maxImages.Text = "Max. Images: ";
            // 
            // numericUpDown_maxImages
            // 
            numericUpDown_maxImages.Location = new Point(97, 32);
            numericUpDown_maxImages.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
            numericUpDown_maxImages.Name = "numericUpDown_maxImages";
            numericUpDown_maxImages.Size = new Size(75, 23);
            numericUpDown_maxImages.TabIndex = 8;
            numericUpDown_maxImages.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // button_openOutput
            // 
            button_openOutput.Location = new Point(178, 32);
            button_openOutput.Name = "button_openOutput";
            button_openOutput.Size = new Size(55, 23);
            button_openOutput.TabIndex = 10;
            button_openOutput.Text = "Output";
            button_openOutput.UseVisualStyleBackColor = true;
            button_openOutput.Click += button_openOutput_Click;
            // 
            // listBox_log
            // 
            listBox_log.FormattingEnabled = true;
            listBox_log.HorizontalScrollbar = true;
            listBox_log.Location = new Point(12, 61);
            listBox_log.Name = "listBox_log";
            listBox_log.Size = new Size(221, 124);
            listBox_log.TabIndex = 11;
            listBox_log.TabStop = false;
            // 
            // label_info_denoise
            // 
            label_info_denoise.AutoSize = true;
            label_info_denoise.Location = new Point(236, 121);
            label_info_denoise.Name = "label_info_denoise";
            label_info_denoise.Size = new Size(55, 15);
            label_info_denoise.TabIndex = 13;
            label_info_denoise.Text = "Denoise: ";
            // 
            // numericUpDown_denoise
            // 
            numericUpDown_denoise.DecimalPlaces = 3;
            numericUpDown_denoise.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDown_denoise.Location = new Point(297, 119);
            numericUpDown_denoise.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown_denoise.Name = "numericUpDown_denoise";
            numericUpDown_denoise.Size = new Size(75, 23);
            numericUpDown_denoise.TabIndex = 12;
            numericUpDown_denoise.Value = new decimal(new int[] { 325, 0, 0, 196608 });
            // 
            // checkBox_record
            // 
            checkBox_record.AutoSize = true;
            checkBox_record.Location = new Point(12, 441);
            checkBox_record.Name = "checkBox_record";
            checkBox_record.Size = new Size(83, 19);
            checkBox_record.TabIndex = 14;
            checkBox_record.Text = "Record GIF";
            checkBox_record.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_gifDelay
            // 
            numericUpDown_gifDelay.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericUpDown_gifDelay.Location = new Point(97, 440);
            numericUpDown_gifDelay.Maximum = new decimal(new int[] { 4000, 0, 0, 0 });
            numericUpDown_gifDelay.Minimum = new decimal(new int[] { 25, 0, 0, 0 });
            numericUpDown_gifDelay.Name = "numericUpDown_gifDelay";
            numericUpDown_gifDelay.Size = new Size(54, 23);
            numericUpDown_gifDelay.TabIndex = 15;
            numericUpDown_gifDelay.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // label_info_gifDelay
            // 
            label_info_gifDelay.AutoSize = true;
            label_info_gifDelay.Location = new Point(157, 444);
            label_info_gifDelay.Name = "label_info_gifDelay";
            label_info_gifDelay.Size = new Size(59, 15);
            label_info_gifDelay.TabIndex = 16;
            label_info_gifDelay.Text = "ms/frame";
            // 
            // button_reset
            // 
            button_reset.Location = new Point(239, 165);
            button_reset.Name = "button_reset";
            button_reset.Size = new Size(50, 23);
            button_reset.TabIndex = 17;
            button_reset.Text = "Reset";
            button_reset.UseVisualStyleBackColor = true;
            button_reset.Click += button_reset_Click;
            // 
            // button_comfyUi
            // 
            button_comfyUi.Location = new Point(297, 165);
            button_comfyUi.Name = "button_comfyUi";
            button_comfyUi.Size = new Size(75, 23);
            button_comfyUi.TabIndex = 18;
            button_comfyUi.Text = "ComfyUI";
            button_comfyUi.UseVisualStyleBackColor = true;
            button_comfyUi.Click += button_comfyUi_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, optionsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(384, 24);
            menuStrip1.TabIndex = 19;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openOutputFolderToolStripMenuItem, cloarOutputFolderToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openOutputFolderToolStripMenuItem
            // 
            openOutputFolderToolStripMenuItem.Name = "openOutputFolderToolStripMenuItem";
            openOutputFolderToolStripMenuItem.Size = new Size(176, 22);
            openOutputFolderToolStripMenuItem.Text = "Open output folder";
            openOutputFolderToolStripMenuItem.Click += openOutputFolderToolStripMenuItem_Click;
            // 
            // cloarOutputFolderToolStripMenuItem
            // 
            cloarOutputFolderToolStripMenuItem.Name = "cloarOutputFolderToolStripMenuItem";
            cloarOutputFolderToolStripMenuItem.Size = new Size(176, 22);
            cloarOutputFolderToolStripMenuItem.Text = "Cloar output folder";
            cloarOutputFolderToolStripMenuItem.Click += cloarOutputFolderToolStripMenuItem_Click;
            // 
            // optionsToolStripMenuItem
            // 
            optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { saveFramesToolStripMenuItem, setOutputFolderToolStripMenuItem, editAppsettingsToolStripMenuItem });
            optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            optionsToolStripMenuItem.Size = new Size(61, 20);
            optionsToolStripMenuItem.Text = "Options";
            // 
            // saveFramesToolStripMenuItem
            // 
            saveFramesToolStripMenuItem.CheckOnClick = true;
            saveFramesToolStripMenuItem.Name = "saveFramesToolStripMenuItem";
            saveFramesToolStripMenuItem.Size = new Size(180, 22);
            saveFramesToolStripMenuItem.Text = "Save frames";
            saveFramesToolStripMenuItem.CheckedChanged += saveFramesToolStripMenuItem_CheckedChanged;
            // 
            // setOutputFolderToolStripMenuItem
            // 
            setOutputFolderToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_outputFolder });
            setOutputFolderToolStripMenuItem.Name = "setOutputFolderToolStripMenuItem";
            setOutputFolderToolStripMenuItem.Size = new Size(180, 22);
            setOutputFolderToolStripMenuItem.Text = "Set output folder";
            // 
            // toolStripTextBox_outputFolder
            // 
            toolStripTextBox_outputFolder.Name = "toolStripTextBox_outputFolder";
            toolStripTextBox_outputFolder.Size = new Size(300, 23);
            toolStripTextBox_outputFolder.Text = "Output";
            toolStripTextBox_outputFolder.TextChanged += toolStripTextBox_outputFolder_TextChanged;
            // 
            // editAppsettingsToolStripMenuItem
            // 
            editAppsettingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { comfyUIAPIUrlToolStripMenuItem, comfyUIAPIKeyToolStripMenuItem, comfyUIEXEPathToolStripMenuItem, comfyUiLaunchArgumentsToolStripMenuItem, comfyUIWorkflowPathToolStripMenuItem, positivePromptToolStripMenuItem, negativePromptToolStripMenuItem, outputDirectoryPathToolStripMenuItem, maxKeepAmountToolStripMenuItem, maxTimeoutsecToolStripMenuItem, maxConnectRetriesToolStripMenuItem });
            editAppsettingsToolStripMenuItem.Name = "editAppsettingsToolStripMenuItem";
            editAppsettingsToolStripMenuItem.Size = new Size(180, 22);
            editAppsettingsToolStripMenuItem.Text = "Edit appsettings";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            helpToolStripMenuItem.Click += helpToolStripMenuItem_Click;
            // 
            // comfyUIAPIUrlToolStripMenuItem
            // 
            comfyUIAPIUrlToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_comfyUiApiUrl });
            comfyUIAPIUrlToolStripMenuItem.Name = "comfyUIAPIUrlToolStripMenuItem";
            comfyUIAPIUrlToolStripMenuItem.Size = new Size(219, 22);
            comfyUIAPIUrlToolStripMenuItem.Text = "ComfyUI API url";
            // 
            // comfyUIAPIKeyToolStripMenuItem
            // 
            comfyUIAPIKeyToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_comfyUiApiKey });
            comfyUIAPIKeyToolStripMenuItem.Name = "comfyUIAPIKeyToolStripMenuItem";
            comfyUIAPIKeyToolStripMenuItem.Size = new Size(219, 22);
            comfyUIAPIKeyToolStripMenuItem.Text = "ComfyUI API key";
            // 
            // comfyUIEXEPathToolStripMenuItem
            // 
            comfyUIEXEPathToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_comfyUiExePath });
            comfyUIEXEPathToolStripMenuItem.Name = "comfyUIEXEPathToolStripMenuItem";
            comfyUIEXEPathToolStripMenuItem.Size = new Size(219, 22);
            comfyUIEXEPathToolStripMenuItem.Text = "ComfyUI EXE path";
            // 
            // comfyUIWorkflowPathToolStripMenuItem
            // 
            comfyUIWorkflowPathToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_comfyUiWorkflowPath });
            comfyUIWorkflowPathToolStripMenuItem.Name = "comfyUIWorkflowPathToolStripMenuItem";
            comfyUIWorkflowPathToolStripMenuItem.Size = new Size(219, 22);
            comfyUIWorkflowPathToolStripMenuItem.Text = "ComfyUI Workflow path";
            // 
            // toolStripTextBox_comfyUiApiUrl
            // 
            toolStripTextBox_comfyUiApiUrl.Name = "toolStripTextBox_comfyUiApiUrl";
            toolStripTextBox_comfyUiApiUrl.Size = new Size(300, 23);
            toolStripTextBox_comfyUiApiUrl.Text = "http://localhost:8000";
            toolStripTextBox_comfyUiApiUrl.TextChanged += toolStripTextBox_comfyUiApiUrl_TextChanged;
            // 
            // toolStripTextBox_comfyUiApiKey
            // 
            toolStripTextBox_comfyUiApiKey.Name = "toolStripTextBox_comfyUiApiKey";
            toolStripTextBox_comfyUiApiKey.Size = new Size(300, 23);
            toolStripTextBox_comfyUiApiKey.TextChanged += toolStripTextBox_comfyUiApiKey_TextChanged;
            // 
            // toolStripTextBox_comfyUiExePath
            // 
            toolStripTextBox_comfyUiExePath.Name = "toolStripTextBox_comfyUiExePath";
            toolStripTextBox_comfyUiExePath.Size = new Size(300, 23);
            toolStripTextBox_comfyUiExePath.Text = "%appdata%\\Local\\Programs\\ComfyUI\\ComfyUI.exe";
            toolStripTextBox_comfyUiExePath.TextChanged += toolStripTextBox_comfyUiExePath_TextChanged;
            // 
            // toolStripTextBox_comfyUiWorkflowPath
            // 
            toolStripTextBox_comfyUiWorkflowPath.Name = "toolStripTextBox_comfyUiWorkflowPath";
            toolStripTextBox_comfyUiWorkflowPath.Size = new Size(300, 23);
            toolStripTextBox_comfyUiWorkflowPath.Text = "Ressources\\\\fast_wallpaper_gen.json";
            toolStripTextBox_comfyUiWorkflowPath.TextChanged += toolStripTextBox_comfyUiWorkflowPath_TextChanged;
            // 
            // comfyUiLaunchArgumentsToolStripMenuItem
            // 
            comfyUiLaunchArgumentsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_comfyUiLaunchArguments });
            comfyUiLaunchArgumentsToolStripMenuItem.Name = "comfyUiLaunchArgumentsToolStripMenuItem";
            comfyUiLaunchArgumentsToolStripMenuItem.Size = new Size(219, 22);
            comfyUiLaunchArgumentsToolStripMenuItem.Text = "ComfyUiLaunchArguments";
            // 
            // toolStripTextBox_comfyUiLaunchArguments
            // 
            toolStripTextBox_comfyUiLaunchArguments.Name = "toolStripTextBox_comfyUiLaunchArguments";
            toolStripTextBox_comfyUiLaunchArguments.Size = new Size(300, 23);
            toolStripTextBox_comfyUiLaunchArguments.TextChanged += toolStripTextBox_comfyUiLaunchArguments_TextChanged;
            // 
            // positivePromptToolStripMenuItem
            // 
            positivePromptToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_positivePrompt });
            positivePromptToolStripMenuItem.Name = "positivePromptToolStripMenuItem";
            positivePromptToolStripMenuItem.Size = new Size(219, 22);
            positivePromptToolStripMenuItem.Text = "Positive Prompt";
            // 
            // negativePromptToolStripMenuItem
            // 
            negativePromptToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_negativePrompt });
            negativePromptToolStripMenuItem.Name = "negativePromptToolStripMenuItem";
            negativePromptToolStripMenuItem.Size = new Size(219, 22);
            negativePromptToolStripMenuItem.Text = "Negative Prompt";
            // 
            // outputDirectoryPathToolStripMenuItem
            // 
            outputDirectoryPathToolStripMenuItem.Name = "outputDirectoryPathToolStripMenuItem";
            outputDirectoryPathToolStripMenuItem.Size = new Size(219, 22);
            outputDirectoryPathToolStripMenuItem.Text = "Output directory path";
            outputDirectoryPathToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_outputDirectory });
            // 
            // maxKeepAmountToolStripMenuItem
            // 
            maxKeepAmountToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_maxKeepAmount });
            maxKeepAmountToolStripMenuItem.Name = "maxKeepAmountToolStripMenuItem";
            maxKeepAmountToolStripMenuItem.Size = new Size(219, 22);
            maxKeepAmountToolStripMenuItem.Text = "Max. keep amount";
            // 
            // maxTimeoutsecToolStripMenuItem
            // 
            maxTimeoutsecToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_maxTimeout });
            maxTimeoutsecToolStripMenuItem.Name = "maxTimeoutsecToolStripMenuItem";
            maxTimeoutsecToolStripMenuItem.Size = new Size(219, 22);
            maxTimeoutsecToolStripMenuItem.Text = "Max. timeout (seconds)";
            // 
            // maxConnectRetriesToolStripMenuItem
            // 
            maxConnectRetriesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_maxRetries });
            maxConnectRetriesToolStripMenuItem.Name = "maxConnectRetriesToolStripMenuItem";
            maxConnectRetriesToolStripMenuItem.Size = new Size(219, 22);
            maxConnectRetriesToolStripMenuItem.Text = "Max. connect retries";
            // 
            // toolStripTextBox_positivePrompt
            // 
            toolStripTextBox_positivePrompt.Name = "toolStripTextBox_positivePrompt";
            toolStripTextBox_positivePrompt.Size = new Size(400, 23);
            toolStripTextBox_positivePrompt.Text = "evolve and derive the given image through iteration, keep the same color palette and try to rotate or morph visible features or structures to reshape them slightly or apply another artstyle";
            toolStripTextBox_positivePrompt.TextChanged += toolStripTextBox_positivePrompt_TextChanged;
            // 
            // toolStripTextBox_negativePrompt
            // 
            toolStripTextBox_negativePrompt.Name = "toolStripTextBox_negativePrompt";
            toolStripTextBox_negativePrompt.Size = new Size(300, 23);
            toolStripTextBox_negativePrompt.Text = "blurry, jpeg artifacts";
            toolStripTextBox_negativePrompt.TextChanged += toolStripTextBox_negativePrompt_TextChanged;
            // 
            // toolStripTextBox_maxKeepAmount
            // 
            toolStripTextBox_maxKeepAmount.Name = "toolStripTextBox_maxKeepAmount";
            toolStripTextBox_maxKeepAmount.Size = new Size(80, 23);
            toolStripTextBox_maxKeepAmount.Text = "16";
            toolStripTextBox_maxKeepAmount.TextChanged += toolStripTextBox_maxKeepAmount_TextChanged;
            // 
            // toolStripTextBox_maxTimeout
            // 
            toolStripTextBox_maxTimeout.Name = "toolStripTextBox_maxTimeout";
            toolStripTextBox_maxTimeout.Size = new Size(80, 23);
            toolStripTextBox_maxTimeout.Text = "180";
            toolStripTextBox_maxTimeout.TextChanged += toolStripTextBox_maxTimeout_TextChanged;
            // 
            // toolStripTextBox_maxRetries
            // 
            toolStripTextBox_maxRetries.Name = "toolStripTextBox_maxRetries";
            toolStripTextBox_maxRetries.Size = new Size(80, 23);
            toolStripTextBox_maxRetries.Text = "5";
            toolStripTextBox_maxRetries.TextChanged += toolStripTextBox_maxRetries_TextChanged;
            // 
            // toolStripTextBox_outputDirectory
            // 
            toolStripTextBox_outputDirectory.Name = "toolStripTextBox_outputDirectory";
            toolStripTextBox_outputDirectory.Size = new Size(300, 23);
            toolStripTextBox_outputDirectory.Text = "Output";
            toolStripTextBox_outputDirectory.TextChanged += toolStripTextBox_outputDirectory_TextChanged;
            // 
            // WindowMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 472);
            Controls.Add(button_comfyUi);
            Controls.Add(button_reset);
            Controls.Add(label_info_gifDelay);
            Controls.Add(numericUpDown_gifDelay);
            Controls.Add(checkBox_record);
            Controls.Add(label_info_denoise);
            Controls.Add(numericUpDown_denoise);
            Controls.Add(listBox_log);
            Controls.Add(button_openOutput);
            Controls.Add(label_info_maxImages);
            Controls.Add(numericUpDown_maxImages);
            Controls.Add(pictureBox_preview);
            Controls.Add(label_info_steps);
            Controls.Add(numericUpDown_steps);
            Controls.Add(label_info_batch);
            Controls.Add(numericUpDown_batch);
            Controls.Add(label_info_interval);
            Controls.Add(numericUpDown_interval);
            Controls.Add(checkBox_enabled);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            checkBox_enabled.CheckedChanged += CheckBoxEnabled_CheckedChanged;
            checkBox_record.CheckedChanged += CheckBoxRecord_CheckedChanged;
            listBox_log.MouseDoubleClick += ListBoxLog_MouseDoubleClick;
            Name = "WindowMain";
            Shown += WindowMain_Shown;
            FormClosing += WindowMain_FormClosing;
            Text = "SDWall-E (Configuration)";
            Load += WindowMain_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown_interval).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_batch).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_steps).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_preview).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_maxImages).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_denoise).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_gifDelay).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox checkBox_enabled;
        private NumericUpDown numericUpDown_interval;
        private Label label_info_interval;
        private Label label_info_batch;
        private NumericUpDown numericUpDown_batch;
        private Label label_info_steps;
        private NumericUpDown numericUpDown_steps;
        private PictureBox pictureBox_preview;
        private Label label_info_maxImages;
        private NumericUpDown numericUpDown_maxImages;
        private Button button_openOutput;
        private ListBox listBox_log;
        private Label label_info_denoise;
        private NumericUpDown numericUpDown_denoise;
        private CheckBox checkBox_record;
        private NumericUpDown numericUpDown_gifDelay;
        private Label label_info_gifDelay;
        private Button button_reset;
        private Button button_comfyUi;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openOutputFolderToolStripMenuItem;
        private ToolStripMenuItem cloarOutputFolderToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem editAppsettingsToolStripMenuItem;
        private ToolStripMenuItem setOutputFolderToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem saveFramesToolStripMenuItem;
        private ToolStripTextBox toolStripTextBox_outputFolder;
        private ToolStripMenuItem comfyUIAPIUrlToolStripMenuItem;
        private ToolStripMenuItem comfyUIAPIKeyToolStripMenuItem;
        private ToolStripMenuItem comfyUIEXEPathToolStripMenuItem;
        private ToolStripMenuItem comfyUIWorkflowPathToolStripMenuItem;
        private ToolStripTextBox toolStripTextBox_comfyUiApiUrl;
        private ToolStripTextBox toolStripTextBox_comfyUiApiKey;
        private ToolStripTextBox toolStripTextBox_comfyUiExePath;
        private ToolStripTextBox toolStripTextBox_comfyUiWorkflowPath;
        private ToolStripMenuItem comfyUiLaunchArgumentsToolStripMenuItem;
        private ToolStripTextBox toolStripTextBox_comfyUiLaunchArguments;
        private ToolStripMenuItem positivePromptToolStripMenuItem;
        private ToolStripTextBox toolStripTextBox_positivePrompt;
        private ToolStripMenuItem negativePromptToolStripMenuItem;
        private ToolStripMenuItem outputDirectoryPathToolStripMenuItem;
        private ToolStripMenuItem maxKeepAmountToolStripMenuItem;
        private ToolStripMenuItem maxTimeoutsecToolStripMenuItem;
        private ToolStripMenuItem maxConnectRetriesToolStripMenuItem;
        private ToolStripTextBox toolStripTextBox_negativePrompt;
        private ToolStripTextBox toolStripTextBox_maxKeepAmount;
        private ToolStripTextBox toolStripTextBox_maxTimeout;
        private ToolStripTextBox toolStripTextBox_maxRetries;
        private ToolStripTextBox toolStripTextBox_outputDirectory;
    }
}
