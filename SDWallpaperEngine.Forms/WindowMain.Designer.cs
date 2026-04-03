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
            ((System.ComponentModel.ISupportInitialize)numericUpDown_interval).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_batch).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_steps).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_preview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_maxImages).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_denoise).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_gifDelay).BeginInit();
            SuspendLayout();
            // 
            // checkBox_enabled
            // 
            checkBox_enabled.AutoSize = true;
            checkBox_enabled.Location = new Point(304, 421);
            checkBox_enabled.Name = "checkBox_enabled";
            checkBox_enabled.Size = new Size(68, 19);
            checkBox_enabled.TabIndex = 0;
            checkBox_enabled.Text = "Enabled";
            checkBox_enabled.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_interval
            // 
            numericUpDown_interval.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericUpDown_interval.Location = new Point(297, 12);
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
            label_info_interval.Location = new Point(239, 14);
            label_info_interval.Name = "label_info_interval";
            label_info_interval.Size = new Size(52, 15);
            label_info_interval.TabIndex = 2;
            label_info_interval.Text = "Interval: ";
            // 
            // label_info_batch
            // 
            label_info_batch.AutoSize = true;
            label_info_batch.Location = new Point(251, 43);
            label_info_batch.Name = "label_info_batch";
            label_info_batch.Size = new Size(40, 15);
            label_info_batch.TabIndex = 4;
            label_info_batch.Text = "Batch:";
            // 
            // numericUpDown_batch
            // 
            numericUpDown_batch.Location = new Point(297, 41);
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
            label_info_steps.Location = new Point(253, 72);
            label_info_steps.Name = "label_info_steps";
            label_info_steps.Size = new Size(38, 15);
            label_info_steps.TabIndex = 6;
            label_info_steps.Text = "Steps:";
            // 
            // numericUpDown_steps
            // 
            numericUpDown_steps.Location = new Point(297, 70);
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
            pictureBox_preview.Location = new Point(12, 174);
            pictureBox_preview.Name = "pictureBox_preview";
            pictureBox_preview.Size = new Size(360, 240);
            pictureBox_preview.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_preview.TabIndex = 7;
            pictureBox_preview.TabStop = false;
            // 
            // label_info_maxImages
            // 
            label_info_maxImages.AutoSize = true;
            label_info_maxImages.Location = new Point(12, 14);
            label_info_maxImages.Name = "label_info_maxImages";
            label_info_maxImages.Size = new Size(79, 15);
            label_info_maxImages.TabIndex = 9;
            label_info_maxImages.Text = "Max. Images: ";
            // 
            // numericUpDown_maxImages
            // 
            numericUpDown_maxImages.Location = new Point(97, 12);
            numericUpDown_maxImages.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
            numericUpDown_maxImages.Name = "numericUpDown_maxImages";
            numericUpDown_maxImages.Size = new Size(75, 23);
            numericUpDown_maxImages.TabIndex = 8;
            numericUpDown_maxImages.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // button_openOutput
            // 
            button_openOutput.Location = new Point(178, 12);
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
            listBox_log.Location = new Point(12, 41);
            listBox_log.Name = "listBox_log";
            listBox_log.Size = new Size(221, 124);
            listBox_log.TabIndex = 11;
            listBox_log.TabStop = false;
            // 
            // label_info_denoise
            // 
            label_info_denoise.AutoSize = true;
            label_info_denoise.Location = new Point(236, 101);
            label_info_denoise.Name = "label_info_denoise";
            label_info_denoise.Size = new Size(55, 15);
            label_info_denoise.TabIndex = 13;
            label_info_denoise.Text = "Denoise: ";
            // 
            // numericUpDown_denoise
            // 
            numericUpDown_denoise.DecimalPlaces = 3;
            numericUpDown_denoise.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDown_denoise.Location = new Point(297, 99);
            numericUpDown_denoise.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown_denoise.Name = "numericUpDown_denoise";
            numericUpDown_denoise.Size = new Size(75, 23);
            numericUpDown_denoise.TabIndex = 12;
            numericUpDown_denoise.Value = new decimal(new int[] { 325, 0, 0, 196608 });
            // 
            // checkBox_record
            // 
            checkBox_record.AutoSize = true;
            checkBox_record.Location = new Point(12, 421);
            checkBox_record.Name = "checkBox_record";
            checkBox_record.Size = new Size(83, 19);
            checkBox_record.TabIndex = 14;
            checkBox_record.Text = "Record GIF";
            checkBox_record.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_gifDelay
            // 
            numericUpDown_gifDelay.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericUpDown_gifDelay.Location = new Point(97, 420);
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
            label_info_gifDelay.Location = new Point(157, 424);
            label_info_gifDelay.Name = "label_info_gifDelay";
            label_info_gifDelay.Size = new Size(59, 15);
            label_info_gifDelay.TabIndex = 16;
            label_info_gifDelay.Text = "ms/frame";
            // 
            // WindowMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 451);
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
            Name = "WindowMain";
            Text = "SDWall-E (Configuration)";
            ((System.ComponentModel.ISupportInitialize)numericUpDown_interval).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_batch).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_steps).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_preview).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_maxImages).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_denoise).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_gifDelay).EndInit();
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
    }
}
