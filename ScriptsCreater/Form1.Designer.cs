namespace ScriptsCreater
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnScript = new System.Windows.Forms.Button();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.txSalida = new System.Windows.Forms.TextBox();
            this.txFile = new System.Windows.Forms.TextBox();
            this.lbl_file = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbLectorCSV = new System.Windows.Forms.RadioButton();
            this.rbHist = new System.Windows.Forms.RadioButton();
            this.rbIntegridad = new System.Windows.Forms.RadioButton();
            this.rbDSDM = new System.Windows.Forms.RadioButton();
            this.rbMaestro = new System.Windows.Forms.RadioButton();
            this.btRuta = new System.Windows.Forms.Button();
            this.gbDSDM = new System.Windows.Forms.GroupBox();
            this.cbIncremental = new System.Windows.Forms.CheckBox();
            this.rb_DSDM_DM = new System.Windows.Forms.RadioButton();
            this.rb_DSDM_DS = new System.Windows.Forms.RadioButton();
            this.rb_DSDM_All = new System.Windows.Forms.RadioButton();
            this.rb_DSDM_T = new System.Windows.Forms.RadioButton();
            this.gbHist = new System.Windows.Forms.GroupBox();
            this.cb_ClaveAuto = new System.Windows.Forms.CheckBox();
            this.rb_Directorio = new System.Windows.Forms.RadioButton();
            this.rb_Archivo = new System.Windows.Forms.RadioButton();
            this.gbAcciones = new System.Windows.Forms.GroupBox();
            this.cb_IndexCS = new System.Windows.Forms.CheckBox();
            this.cb_ChangeTrack = new System.Windows.Forms.CheckBox();
            this.cb_CreateTable = new System.Windows.Forms.CheckBox();
            this.rb_tabMF_STG = new System.Windows.Forms.RadioButton();
            this.txBBDD = new System.Windows.Forms.TextBox();
            this.rb_CSV_MF_STG = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.gbDSDM.SuspendLayout();
            this.gbHist.SuspendLayout();
            this.gbAcciones.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnScript
            // 
            this.btnScript.Location = new System.Drawing.Point(852, 266);
            this.btnScript.Name = "btnScript";
            this.btnScript.Size = new System.Drawing.Size(86, 48);
            this.btnScript.TabIndex = 9;
            this.btnScript.Text = "Generar Script";
            this.btnScript.UseVisualStyleBackColor = true;
            this.btnScript.Click += new System.EventHandler(this.btnScript_Click);
            // 
            // btnBuscar
            // 
            this.btnBuscar.Location = new System.Drawing.Point(863, 11);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(75, 23);
            this.btnBuscar.TabIndex = 8;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.button1_Click);
            // 
            // txSalida
            // 
            this.txSalida.Location = new System.Drawing.Point(123, 52);
            this.txSalida.Name = "txSalida";
            this.txSalida.Size = new System.Drawing.Size(720, 22);
            this.txSalida.TabIndex = 7;
            // 
            // txFile
            // 
            this.txFile.Location = new System.Drawing.Point(123, 12);
            this.txFile.Name = "txFile";
            this.txFile.ReadOnly = true;
            this.txFile.Size = new System.Drawing.Size(495, 22);
            this.txFile.TabIndex = 6;
            // 
            // lbl_file
            // 
            this.lbl_file.AutoSize = true;
            this.lbl_file.Location = new System.Drawing.Point(9, 15);
            this.lbl_file.Name = "lbl_file";
            this.lbl_file.Size = new System.Drawing.Size(101, 17);
            this.lbl_file.TabIndex = 10;
            this.lbl_file.Text = "CSV a generar";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Ruta almacenar";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rb_CSV_MF_STG);
            this.groupBox1.Controls.Add(this.rb_tabMF_STG);
            this.groupBox1.Controls.Add(this.rbLectorCSV);
            this.groupBox1.Controls.Add(this.rbHist);
            this.groupBox1.Controls.Add(this.rbIntegridad);
            this.groupBox1.Controls.Add(this.rbDSDM);
            this.groupBox1.Controls.Add(this.rbMaestro);
            this.groupBox1.Location = new System.Drawing.Point(12, 96);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(221, 218);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tipo Script";
            // 
            // rbLectorCSV
            // 
            this.rbLectorCSV.AutoSize = true;
            this.rbLectorCSV.Location = new System.Drawing.Point(24, 134);
            this.rbLectorCSV.Name = "rbLectorCSV";
            this.rbLectorCSV.Size = new System.Drawing.Size(148, 21);
            this.rbLectorCSV.TabIndex = 4;
            this.rbLectorCSV.TabStop = true;
            this.rbLectorCSV.Text = "Lector CSV UNION";
            this.rbLectorCSV.UseVisualStyleBackColor = true;
            this.rbLectorCSV.CheckedChanged += new System.EventHandler(this.rbLectorCSV_CheckedChanged);
            // 
            // rbHist
            // 
            this.rbHist.AutoSize = true;
            this.rbHist.Location = new System.Drawing.Point(25, 79);
            this.rbHist.Name = "rbHist";
            this.rbHist.Size = new System.Drawing.Size(117, 21);
            this.rbHist.TabIndex = 3;
            this.rbHist.TabStop = true;
            this.rbHist.Text = "Historificación";
            this.rbHist.UseVisualStyleBackColor = true;
            this.rbHist.CheckedChanged += new System.EventHandler(this.rbHist_CheckedChanged);
            // 
            // rbIntegridad
            // 
            this.rbIntegridad.AutoSize = true;
            this.rbIntegridad.Location = new System.Drawing.Point(25, 52);
            this.rbIntegridad.Name = "rbIntegridad";
            this.rbIntegridad.Size = new System.Drawing.Size(92, 21);
            this.rbIntegridad.TabIndex = 2;
            this.rbIntegridad.TabStop = true;
            this.rbIntegridad.Text = "Integridad";
            this.rbIntegridad.UseVisualStyleBackColor = true;
            this.rbIntegridad.CheckedChanged += new System.EventHandler(this.rbIntegridad_CheckedChanged);
            // 
            // rbDSDM
            // 
            this.rbDSDM.AutoSize = true;
            this.rbDSDM.Location = new System.Drawing.Point(25, 107);
            this.rbDSDM.Name = "rbDSDM";
            this.rbDSDM.Size = new System.Drawing.Size(74, 21);
            this.rbDSDM.TabIndex = 1;
            this.rbDSDM.TabStop = true;
            this.rbDSDM.Text = "DS-DM";
            this.rbDSDM.UseVisualStyleBackColor = true;
            this.rbDSDM.CheckedChanged += new System.EventHandler(this.rbDSDM_CheckedChanged);
            // 
            // rbMaestro
            // 
            this.rbMaestro.AutoSize = true;
            this.rbMaestro.Location = new System.Drawing.Point(25, 25);
            this.rbMaestro.Name = "rbMaestro";
            this.rbMaestro.Size = new System.Drawing.Size(87, 21);
            this.rbMaestro.TabIndex = 0;
            this.rbMaestro.TabStop = true;
            this.rbMaestro.Text = "Maestros";
            this.rbMaestro.UseVisualStyleBackColor = true;
            this.rbMaestro.CheckedChanged += new System.EventHandler(this.rbMaestro_CheckedChanged);
            // 
            // btRuta
            // 
            this.btRuta.Location = new System.Drawing.Point(863, 51);
            this.btRuta.Name = "btRuta";
            this.btRuta.Size = new System.Drawing.Size(75, 23);
            this.btRuta.TabIndex = 13;
            this.btRuta.Text = "Ruta";
            this.btRuta.UseVisualStyleBackColor = true;
            this.btRuta.Click += new System.EventHandler(this.btRuta_Click);
            // 
            // gbDSDM
            // 
            this.gbDSDM.Controls.Add(this.cbIncremental);
            this.gbDSDM.Controls.Add(this.rb_DSDM_DM);
            this.gbDSDM.Controls.Add(this.rb_DSDM_DS);
            this.gbDSDM.Controls.Add(this.rb_DSDM_All);
            this.gbDSDM.Controls.Add(this.rb_DSDM_T);
            this.gbDSDM.Location = new System.Drawing.Point(472, 96);
            this.gbDSDM.Name = "gbDSDM";
            this.gbDSDM.Size = new System.Drawing.Size(221, 218);
            this.gbDSDM.TabIndex = 14;
            this.gbDSDM.TabStop = false;
            this.gbDSDM.Text = "DS - DM";
            this.gbDSDM.Visible = false;
            // 
            // cbIncremental
            // 
            this.cbIncremental.AutoSize = true;
            this.cbIncremental.Location = new System.Drawing.Point(19, 185);
            this.cbIncremental.Name = "cbIncremental";
            this.cbIncremental.Size = new System.Drawing.Size(103, 21);
            this.cbIncremental.TabIndex = 4;
            this.cbIncremental.Text = "Incremental";
            this.cbIncremental.UseVisualStyleBackColor = true;
            // 
            // rb_DSDM_DM
            // 
            this.rb_DSDM_DM.AutoSize = true;
            this.rb_DSDM_DM.Location = new System.Drawing.Point(19, 79);
            this.rb_DSDM_DM.Name = "rb_DSDM_DM";
            this.rb_DSDM_DM.Size = new System.Drawing.Size(50, 21);
            this.rb_DSDM_DM.TabIndex = 3;
            this.rb_DSDM_DM.TabStop = true;
            this.rb_DSDM_DM.Text = "DM";
            this.rb_DSDM_DM.UseVisualStyleBackColor = true;
            this.rb_DSDM_DM.CheckedChanged += new System.EventHandler(this.rb_DSDM_DM_CheckedChanged);
            // 
            // rb_DSDM_DS
            // 
            this.rb_DSDM_DS.AutoSize = true;
            this.rb_DSDM_DS.Location = new System.Drawing.Point(19, 52);
            this.rb_DSDM_DS.Name = "rb_DSDM_DS";
            this.rb_DSDM_DS.Size = new System.Drawing.Size(48, 21);
            this.rb_DSDM_DS.TabIndex = 2;
            this.rb_DSDM_DS.TabStop = true;
            this.rb_DSDM_DS.Text = "DS";
            this.rb_DSDM_DS.UseVisualStyleBackColor = true;
            this.rb_DSDM_DS.CheckedChanged += new System.EventHandler(this.rb_DSDM_DS_CheckedChanged);
            // 
            // rb_DSDM_All
            // 
            this.rb_DSDM_All.AutoSize = true;
            this.rb_DSDM_All.Location = new System.Drawing.Point(19, 106);
            this.rb_DSDM_All.Name = "rb_DSDM_All";
            this.rb_DSDM_All.Size = new System.Drawing.Size(74, 21);
            this.rb_DSDM_All.TabIndex = 1;
            this.rb_DSDM_All.TabStop = true;
            this.rb_DSDM_All.Text = "DS-DM";
            this.rb_DSDM_All.UseVisualStyleBackColor = true;
            this.rb_DSDM_All.CheckedChanged += new System.EventHandler(this.rb_DSDM_All_CheckedChanged);
            // 
            // rb_DSDM_T
            // 
            this.rb_DSDM_T.AutoSize = true;
            this.rb_DSDM_T.Location = new System.Drawing.Point(19, 25);
            this.rb_DSDM_T.Name = "rb_DSDM_T";
            this.rb_DSDM_T.Size = new System.Drawing.Size(38, 21);
            this.rb_DSDM_T.TabIndex = 0;
            this.rb_DSDM_T.TabStop = true;
            this.rb_DSDM_T.Text = "T";
            this.rb_DSDM_T.UseVisualStyleBackColor = true;
            this.rb_DSDM_T.CheckedChanged += new System.EventHandler(this.rb_DSDM_T_CheckedChanged);
            // 
            // gbHist
            // 
            this.gbHist.Controls.Add(this.cb_ClaveAuto);
            this.gbHist.Controls.Add(this.rb_Directorio);
            this.gbHist.Controls.Add(this.rb_Archivo);
            this.gbHist.Location = new System.Drawing.Point(245, 96);
            this.gbHist.Name = "gbHist";
            this.gbHist.Size = new System.Drawing.Size(221, 218);
            this.gbHist.TabIndex = 15;
            this.gbHist.TabStop = false;
            this.gbHist.Text = "Historificación";
            this.gbHist.Visible = false;
            // 
            // cb_ClaveAuto
            // 
            this.cb_ClaveAuto.AutoSize = true;
            this.cb_ClaveAuto.Checked = true;
            this.cb_ClaveAuto.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_ClaveAuto.Location = new System.Drawing.Point(21, 185);
            this.cb_ClaveAuto.Name = "cb_ClaveAuto";
            this.cb_ClaveAuto.Size = new System.Drawing.Size(156, 21);
            this.cb_ClaveAuto.TabIndex = 4;
            this.cb_ClaveAuto.Text = "Clave Autonumerico";
            this.cb_ClaveAuto.UseVisualStyleBackColor = true;
            // 
            // rb_Directorio
            // 
            this.rb_Directorio.AutoSize = true;
            this.rb_Directorio.Location = new System.Drawing.Point(19, 52);
            this.rb_Directorio.Name = "rb_Directorio";
            this.rb_Directorio.Size = new System.Drawing.Size(190, 21);
            this.rb_Directorio.TabIndex = 2;
            this.rb_Directorio.TabStop = true;
            this.rb_Directorio.Text = "Todos Archivos directorio";
            this.rb_Directorio.UseVisualStyleBackColor = true;
            // 
            // rb_Archivo
            // 
            this.rb_Archivo.AutoSize = true;
            this.rb_Archivo.Location = new System.Drawing.Point(19, 25);
            this.rb_Archivo.Name = "rb_Archivo";
            this.rb_Archivo.Size = new System.Drawing.Size(163, 21);
            this.rb_Archivo.TabIndex = 0;
            this.rb_Archivo.TabStop = true;
            this.rb_Archivo.Text = "Archivo seleccionado";
            this.rb_Archivo.UseVisualStyleBackColor = true;
            // 
            // gbAcciones
            // 
            this.gbAcciones.Controls.Add(this.cb_IndexCS);
            this.gbAcciones.Controls.Add(this.cb_ChangeTrack);
            this.gbAcciones.Controls.Add(this.cb_CreateTable);
            this.gbAcciones.Location = new System.Drawing.Point(699, 96);
            this.gbAcciones.Name = "gbAcciones";
            this.gbAcciones.Size = new System.Drawing.Size(239, 111);
            this.gbAcciones.TabIndex = 16;
            this.gbAcciones.TabStop = false;
            this.gbAcciones.Text = "Acciones Comunes";
            this.gbAcciones.Visible = false;
            // 
            // cb_IndexCS
            // 
            this.cb_IndexCS.AutoSize = true;
            this.cb_IndexCS.Checked = true;
            this.cb_IndexCS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_IndexCS.Location = new System.Drawing.Point(19, 81);
            this.cb_IndexCS.Name = "cb_IndexCS";
            this.cb_IndexCS.Size = new System.Drawing.Size(209, 21);
            this.cb_IndexCS.TabIndex = 6;
            this.cb_IndexCS.Text = "Generar Indice ColumnStore";
            this.cb_IndexCS.UseVisualStyleBackColor = true;
            // 
            // cb_ChangeTrack
            // 
            this.cb_ChangeTrack.AutoSize = true;
            this.cb_ChangeTrack.Checked = true;
            this.cb_ChangeTrack.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_ChangeTrack.Location = new System.Drawing.Point(19, 53);
            this.cb_ChangeTrack.Name = "cb_ChangeTrack";
            this.cb_ChangeTrack.Size = new System.Drawing.Size(214, 21);
            this.cb_ChangeTrack.TabIndex = 5;
            this.cb_ChangeTrack.Text = "Change Tracking Comentado";
            this.cb_ChangeTrack.UseVisualStyleBackColor = true;
            // 
            // cb_CreateTable
            // 
            this.cb_CreateTable.AutoSize = true;
            this.cb_CreateTable.Location = new System.Drawing.Point(19, 26);
            this.cb_CreateTable.Name = "cb_CreateTable";
            this.cb_CreateTable.Size = new System.Drawing.Size(188, 21);
            this.cb_CreateTable.TabIndex = 4;
            this.cb_CreateTable.Text = "Create Table Comentado";
            this.cb_CreateTable.UseVisualStyleBackColor = true;
            // 
            // rb_tabMF_STG
            // 
            this.rb_tabMF_STG.AutoSize = true;
            this.rb_tabMF_STG.Location = new System.Drawing.Point(24, 161);
            this.rb_tabMF_STG.Name = "rb_tabMF_STG";
            this.rb_tabMF_STG.Size = new System.Drawing.Size(133, 21);
            this.rb_tabMF_STG.TabIndex = 5;
            this.rb_tabMF_STG.TabStop = true;
            this.rb_tabMF_STG.Text = "Table MF a STG";
            this.rb_tabMF_STG.UseVisualStyleBackColor = true;
            this.rb_tabMF_STG.CheckedChanged += new System.EventHandler(this.rb_tabMF_STG_CheckedChanged);
            // 
            // txBBDD
            // 
            this.txBBDD.Location = new System.Drawing.Point(624, 12);
            this.txBBDD.Name = "txBBDD";
            this.txBBDD.Size = new System.Drawing.Size(219, 22);
            this.txBBDD.TabIndex = 17;
            this.txBBDD.Visible = false;
            // 
            // rb_CSV_MF_STG
            // 
            this.rb_CSV_MF_STG.AutoSize = true;
            this.rb_CSV_MF_STG.Location = new System.Drawing.Point(24, 188);
            this.rb_CSV_MF_STG.Name = "rb_CSV_MF_STG";
            this.rb_CSV_MF_STG.Size = new System.Drawing.Size(124, 21);
            this.rb_CSV_MF_STG.TabIndex = 6;
            this.rb_CSV_MF_STG.TabStop = true;
            this.rb_CSV_MF_STG.Text = "CSV MF a STG";
            this.rb_CSV_MF_STG.UseVisualStyleBackColor = true;
            this.rb_CSV_MF_STG.CheckedChanged += new System.EventHandler(this.rb_CSV_MF_STG_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 326);
            this.Controls.Add(this.txBBDD);
            this.Controls.Add(this.gbDSDM);
            this.Controls.Add(this.gbHist);
            this.Controls.Add(this.gbAcciones);
            this.Controls.Add(this.btRuta);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbl_file);
            this.Controls.Add(this.btnScript);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.txSalida);
            this.Controls.Add(this.txFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Generador Script";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbDSDM.ResumeLayout(false);
            this.gbDSDM.PerformLayout();
            this.gbHist.ResumeLayout(false);
            this.gbHist.PerformLayout();
            this.gbAcciones.ResumeLayout(false);
            this.gbAcciones.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnScript;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.TextBox txSalida;
        private System.Windows.Forms.TextBox txFile;
        private System.Windows.Forms.Label lbl_file;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbDSDM;
        private System.Windows.Forms.RadioButton rbMaestro;
        private System.Windows.Forms.Button btRuta;
        private System.Windows.Forms.RadioButton rbIntegridad;
        private System.Windows.Forms.RadioButton rbHist;
        private System.Windows.Forms.GroupBox gbDSDM;
        private System.Windows.Forms.CheckBox cbIncremental;
        private System.Windows.Forms.RadioButton rb_DSDM_DM;
        private System.Windows.Forms.RadioButton rb_DSDM_DS;
        private System.Windows.Forms.RadioButton rb_DSDM_All;
        private System.Windows.Forms.RadioButton rb_DSDM_T;
        private System.Windows.Forms.GroupBox gbHist;
        private System.Windows.Forms.CheckBox cb_ClaveAuto;
        private System.Windows.Forms.RadioButton rb_Directorio;
        private System.Windows.Forms.RadioButton rb_Archivo;
        private System.Windows.Forms.GroupBox gbAcciones;
        private System.Windows.Forms.CheckBox cb_ChangeTrack;
        private System.Windows.Forms.CheckBox cb_CreateTable;
        private System.Windows.Forms.CheckBox cb_IndexCS;
        private System.Windows.Forms.RadioButton rbLectorCSV;
        private System.Windows.Forms.RadioButton rb_tabMF_STG;
        private System.Windows.Forms.TextBox txBBDD;
        private System.Windows.Forms.RadioButton rb_CSV_MF_STG;
    }
}

