namespace ScriptsCreator
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
            this.rbHistCsv = new System.Windows.Forms.RadioButton();
            this.rb_CSV_MF_STG = new System.Windows.Forms.RadioButton();
            this.rb_tabMF_STG = new System.Windows.Forms.RadioButton();
            this.rbLectorCSV = new System.Windows.Forms.RadioButton();
            this.rbHist = new System.Windows.Forms.RadioButton();
            this.rbMaestro = new System.Windows.Forms.RadioButton();
            this.rbIntegridad = new System.Windows.Forms.RadioButton();
            this.rbDSDM = new System.Windows.Forms.RadioButton();
            this.btRuta = new System.Windows.Forms.Button();
            this.gbDSDM = new System.Windows.Forms.GroupBox();
            this.cbIncremental = new System.Windows.Forms.CheckBox();
            this.rb_DSDM_DM = new System.Windows.Forms.RadioButton();
            this.rb_DSDM_DS = new System.Windows.Forms.RadioButton();
            this.rb_DSDM_All = new System.Windows.Forms.RadioButton();
            this.rb_DSDM_T = new System.Windows.Forms.RadioButton();
            this.gbHist = new System.Windows.Forms.GroupBox();
            this.pnl_Hist_CSV = new System.Windows.Forms.Panel();
            this.rb_Archivo = new System.Windows.Forms.RadioButton();
            this.rb_Directorio = new System.Windows.Forms.RadioButton();
            this.cb_ClaveAuto = new System.Windows.Forms.CheckBox();
            this.rb_Hist_CSV = new System.Windows.Forms.RadioButton();
            this.rb_Hist_Tabla = new System.Windows.Forms.RadioButton();
            this.gbAcciones = new System.Windows.Forms.GroupBox();
            this.cb_IndexCS = new System.Windows.Forms.CheckBox();
            this.cb_ChangeTrack = new System.Windows.Forms.CheckBox();
            this.cb_CreateTable = new System.Windows.Forms.CheckBox();
            this.txBBDD = new System.Windows.Forms.TextBox();
            this.lbl_bd_microfocus = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.gbDSDM.SuspendLayout();
            this.gbHist.SuspendLayout();
            this.pnl_Hist_CSV.SuspendLayout();
            this.gbAcciones.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnScript
            // 
            this.btnScript.Location = new System.Drawing.Point(528, 199);
            this.btnScript.Margin = new System.Windows.Forms.Padding(2);
            this.btnScript.Name = "btnScript";
            this.btnScript.Size = new System.Drawing.Size(156, 36);
            this.btnScript.TabIndex = 10;
            this.btnScript.Text = "Generar Script";
            this.btnScript.UseVisualStyleBackColor = true;
            this.btnScript.Visible = false;
            this.btnScript.Click += new System.EventHandler(this.btnScript_Click);
            // 
            // btnBuscar
            // 
            this.btnBuscar.Location = new System.Drawing.Point(637, 9);
            this.btnBuscar.Margin = new System.Windows.Forms.Padding(2);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(75, 19);
            this.btnBuscar.TabIndex = 9;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.button1_Click);
            // 
            // txSalida
            // 
            this.txSalida.Location = new System.Drawing.Point(104, 42);
            this.txSalida.Margin = new System.Windows.Forms.Padding(2);
            this.txSalida.Name = "txSalida";
            this.txSalida.Size = new System.Drawing.Size(529, 20);
            this.txSalida.TabIndex = 8;
            // 
            // txFile
            // 
            this.txFile.Location = new System.Drawing.Point(104, 10);
            this.txFile.Margin = new System.Windows.Forms.Padding(2);
            this.txFile.Name = "txFile";
            this.txFile.ReadOnly = true;
            this.txFile.Size = new System.Drawing.Size(389, 20);
            this.txFile.TabIndex = 6;
            this.txFile.TextChanged += new System.EventHandler(this.txFile_TextChanged);
            // 
            // lbl_file
            // 
            this.lbl_file.AutoSize = true;
            this.lbl_file.Location = new System.Drawing.Point(7, 12);
            this.lbl_file.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_file.Name = "lbl_file";
            this.lbl_file.Size = new System.Drawing.Size(41, 13);
            this.lbl_file.TabIndex = 11;
            this.lbl_file.Text = "Origen:";
            this.lbl_file.Click += new System.EventHandler(this.lbl_file_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Ruta destino:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbHistCsv);
            this.groupBox1.Controls.Add(this.rb_CSV_MF_STG);
            this.groupBox1.Controls.Add(this.rb_tabMF_STG);
            this.groupBox1.Controls.Add(this.rbLectorCSV);
            this.groupBox1.Controls.Add(this.rbHist);
            this.groupBox1.Controls.Add(this.rbMaestro);
            this.groupBox1.Controls.Add(this.rbIntegridad);
            this.groupBox1.Controls.Add(this.rbDSDM);
            this.groupBox1.Location = new System.Drawing.Point(9, 78);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(171, 225);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tipo Script";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // rbHistCsv
            // 
            this.rbHistCsv.AutoSize = true;
            this.rbHistCsv.Location = new System.Drawing.Point(14, 41);
            this.rbHistCsv.Margin = new System.Windows.Forms.Padding(2);
            this.rbHistCsv.Name = "rbHistCsv";
            this.rbHistCsv.Size = new System.Drawing.Size(149, 17);
            this.rbHistCsv.TabIndex = 19;
            this.rbHistCsv.TabStop = true;
            this.rbHistCsv.Text = "Historificación (desde csv)";
            this.rbHistCsv.UseVisualStyleBackColor = true;
            this.rbHistCsv.CheckedChanged += new System.EventHandler(this.rbHistCsv_CheckedChanged);
            // 
            // rb_CSV_MF_STG
            // 
            this.rb_CSV_MF_STG.AutoSize = true;
            this.rb_CSV_MF_STG.Location = new System.Drawing.Point(14, 83);
            this.rb_CSV_MF_STG.Margin = new System.Windows.Forms.Padding(2);
            this.rb_CSV_MF_STG.Name = "rb_CSV_MF_STG";
            this.rb_CSV_MF_STG.Size = new System.Drawing.Size(133, 17);
            this.rb_CSV_MF_STG.TabIndex = 6;
            this.rb_CSV_MF_STG.TabStop = true;
            this.rb_CSV_MF_STG.Text = "Extracción (desde csv)";
            this.rb_CSV_MF_STG.UseVisualStyleBackColor = true;
            this.rb_CSV_MF_STG.CheckedChanged += new System.EventHandler(this.rb_CSV_MF_STG_CheckedChanged);
            // 
            // rb_tabMF_STG
            // 
            this.rb_tabMF_STG.AutoSize = true;
            this.rb_tabMF_STG.Location = new System.Drawing.Point(14, 62);
            this.rb_tabMF_STG.Margin = new System.Windows.Forms.Padding(2);
            this.rb_tabMF_STG.Name = "rb_tabMF_STG";
            this.rb_tabMF_STG.Size = new System.Drawing.Size(139, 17);
            this.rb_tabMF_STG.TabIndex = 5;
            this.rb_tabMF_STG.TabStop = true;
            this.rb_tabMF_STG.Text = "Extracción (desde tabla)";
            this.rb_tabMF_STG.UseVisualStyleBackColor = true;
            this.rb_tabMF_STG.CheckedChanged += new System.EventHandler(this.rb_tabMF_STG_CheckedChanged);
            // 
            // rbLectorCSV
            // 
            this.rbLectorCSV.AutoSize = true;
            this.rbLectorCSV.Enabled = false;
            this.rbLectorCSV.Location = new System.Drawing.Point(14, 161);
            this.rbLectorCSV.Margin = new System.Windows.Forms.Padding(2);
            this.rbLectorCSV.Name = "rbLectorCSV";
            this.rbLectorCSV.Size = new System.Drawing.Size(117, 17);
            this.rbLectorCSV.TabIndex = 4;
            this.rbLectorCSV.TabStop = true;
            this.rbLectorCSV.Text = "Lector CSV UNION";
            this.rbLectorCSV.UseVisualStyleBackColor = true;
            this.rbLectorCSV.Visible = false;
            this.rbLectorCSV.CheckedChanged += new System.EventHandler(this.rbLectorCSV_CheckedChanged);
            // 
            // rbHist
            // 
            this.rbHist.AutoSize = true;
            this.rbHist.Location = new System.Drawing.Point(14, 20);
            this.rbHist.Margin = new System.Windows.Forms.Padding(2);
            this.rbHist.Name = "rbHist";
            this.rbHist.Size = new System.Drawing.Size(155, 17);
            this.rbHist.TabIndex = 3;
            this.rbHist.TabStop = true;
            this.rbHist.Text = "Historificación (desde tabla)";
            this.rbHist.UseVisualStyleBackColor = true;
            this.rbHist.CheckedChanged += new System.EventHandler(this.rbHist_CheckedChanged);
            // 
            // rbMaestro
            // 
            this.rbMaestro.AutoSize = true;
            this.rbMaestro.Enabled = false;
            this.rbMaestro.Location = new System.Drawing.Point(14, 204);
            this.rbMaestro.Margin = new System.Windows.Forms.Padding(2);
            this.rbMaestro.Name = "rbMaestro";
            this.rbMaestro.Size = new System.Drawing.Size(68, 17);
            this.rbMaestro.TabIndex = 0;
            this.rbMaestro.TabStop = true;
            this.rbMaestro.Text = "Maestros";
            this.rbMaestro.UseVisualStyleBackColor = true;
            this.rbMaestro.Visible = false;
            this.rbMaestro.CheckedChanged += new System.EventHandler(this.rbMaestro_CheckedChanged);
            // 
            // rbIntegridad
            // 
            this.rbIntegridad.AutoSize = true;
            this.rbIntegridad.Enabled = false;
            this.rbIntegridad.Location = new System.Drawing.Point(14, 182);
            this.rbIntegridad.Margin = new System.Windows.Forms.Padding(2);
            this.rbIntegridad.Name = "rbIntegridad";
            this.rbIntegridad.Size = new System.Drawing.Size(72, 17);
            this.rbIntegridad.TabIndex = 2;
            this.rbIntegridad.TabStop = true;
            this.rbIntegridad.Text = "Integridad";
            this.rbIntegridad.UseVisualStyleBackColor = true;
            this.rbIntegridad.Visible = false;
            this.rbIntegridad.CheckedChanged += new System.EventHandler(this.rbIntegridad_CheckedChanged);
            // 
            // rbDSDM
            // 
            this.rbDSDM.AutoSize = true;
            this.rbDSDM.Enabled = false;
            this.rbDSDM.Location = new System.Drawing.Point(14, 140);
            this.rbDSDM.Margin = new System.Windows.Forms.Padding(2);
            this.rbDSDM.Name = "rbDSDM";
            this.rbDSDM.Size = new System.Drawing.Size(60, 17);
            this.rbDSDM.TabIndex = 1;
            this.rbDSDM.TabStop = true;
            this.rbDSDM.Text = "DS-DM";
            this.rbDSDM.UseVisualStyleBackColor = true;
            this.rbDSDM.Visible = false;
            this.rbDSDM.CheckedChanged += new System.EventHandler(this.rbDSDM_CheckedChanged);
            // 
            // btRuta
            // 
            this.btRuta.Location = new System.Drawing.Point(637, 41);
            this.btRuta.Margin = new System.Windows.Forms.Padding(2);
            this.btRuta.Name = "btRuta";
            this.btRuta.Size = new System.Drawing.Size(75, 19);
            this.btRuta.TabIndex = 14;
            this.btRuta.Text = "Seleccionar";
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
            this.gbDSDM.Location = new System.Drawing.Point(354, 78);
            this.gbDSDM.Margin = new System.Windows.Forms.Padding(2);
            this.gbDSDM.Name = "gbDSDM";
            this.gbDSDM.Padding = new System.Windows.Forms.Padding(2);
            this.gbDSDM.Size = new System.Drawing.Size(166, 225);
            this.gbDSDM.TabIndex = 15;
            this.gbDSDM.TabStop = false;
            this.gbDSDM.Text = "DS - DM";
            this.gbDSDM.Visible = false;
            // 
            // cbIncremental
            // 
            this.cbIncremental.AutoSize = true;
            this.cbIncremental.Enabled = false;
            this.cbIncremental.Location = new System.Drawing.Point(14, 111);
            this.cbIncremental.Margin = new System.Windows.Forms.Padding(2);
            this.cbIncremental.Name = "cbIncremental";
            this.cbIncremental.Size = new System.Drawing.Size(81, 17);
            this.cbIncremental.TabIndex = 4;
            this.cbIncremental.Text = "Incremental";
            this.cbIncremental.UseVisualStyleBackColor = true;
            this.cbIncremental.Visible = false;
            // 
            // rb_DSDM_DM
            // 
            this.rb_DSDM_DM.AutoSize = true;
            this.rb_DSDM_DM.Enabled = false;
            this.rb_DSDM_DM.Location = new System.Drawing.Point(14, 64);
            this.rb_DSDM_DM.Margin = new System.Windows.Forms.Padding(2);
            this.rb_DSDM_DM.Name = "rb_DSDM_DM";
            this.rb_DSDM_DM.Size = new System.Drawing.Size(42, 17);
            this.rb_DSDM_DM.TabIndex = 3;
            this.rb_DSDM_DM.TabStop = true;
            this.rb_DSDM_DM.Text = "DM";
            this.rb_DSDM_DM.UseVisualStyleBackColor = true;
            this.rb_DSDM_DM.Visible = false;
            this.rb_DSDM_DM.CheckedChanged += new System.EventHandler(this.rb_DSDM_DM_CheckedChanged);
            // 
            // rb_DSDM_DS
            // 
            this.rb_DSDM_DS.AutoSize = true;
            this.rb_DSDM_DS.Enabled = false;
            this.rb_DSDM_DS.Location = new System.Drawing.Point(14, 42);
            this.rb_DSDM_DS.Margin = new System.Windows.Forms.Padding(2);
            this.rb_DSDM_DS.Name = "rb_DSDM_DS";
            this.rb_DSDM_DS.Size = new System.Drawing.Size(40, 17);
            this.rb_DSDM_DS.TabIndex = 2;
            this.rb_DSDM_DS.TabStop = true;
            this.rb_DSDM_DS.Text = "DS";
            this.rb_DSDM_DS.UseVisualStyleBackColor = true;
            this.rb_DSDM_DS.Visible = false;
            this.rb_DSDM_DS.CheckedChanged += new System.EventHandler(this.rb_DSDM_DS_CheckedChanged);
            // 
            // rb_DSDM_All
            // 
            this.rb_DSDM_All.AutoSize = true;
            this.rb_DSDM_All.Enabled = false;
            this.rb_DSDM_All.Location = new System.Drawing.Point(14, 86);
            this.rb_DSDM_All.Margin = new System.Windows.Forms.Padding(2);
            this.rb_DSDM_All.Name = "rb_DSDM_All";
            this.rb_DSDM_All.Size = new System.Drawing.Size(60, 17);
            this.rb_DSDM_All.TabIndex = 1;
            this.rb_DSDM_All.TabStop = true;
            this.rb_DSDM_All.Text = "DS-DM";
            this.rb_DSDM_All.UseVisualStyleBackColor = true;
            this.rb_DSDM_All.Visible = false;
            this.rb_DSDM_All.CheckedChanged += new System.EventHandler(this.rb_DSDM_All_CheckedChanged);
            // 
            // rb_DSDM_T
            // 
            this.rb_DSDM_T.AutoSize = true;
            this.rb_DSDM_T.Enabled = false;
            this.rb_DSDM_T.Location = new System.Drawing.Point(14, 20);
            this.rb_DSDM_T.Margin = new System.Windows.Forms.Padding(2);
            this.rb_DSDM_T.Name = "rb_DSDM_T";
            this.rb_DSDM_T.Size = new System.Drawing.Size(32, 17);
            this.rb_DSDM_T.TabIndex = 0;
            this.rb_DSDM_T.TabStop = true;
            this.rb_DSDM_T.Text = "T";
            this.rb_DSDM_T.UseVisualStyleBackColor = true;
            this.rb_DSDM_T.Visible = false;
            this.rb_DSDM_T.CheckedChanged += new System.EventHandler(this.rb_DSDM_T_CheckedChanged);
            // 
            // gbHist
            // 
            this.gbHist.Controls.Add(this.pnl_Hist_CSV);
            this.gbHist.Controls.Add(this.rb_Hist_CSV);
            this.gbHist.Controls.Add(this.rb_Hist_Tabla);
            this.gbHist.Location = new System.Drawing.Point(184, 78);
            this.gbHist.Margin = new System.Windows.Forms.Padding(2);
            this.gbHist.Name = "gbHist";
            this.gbHist.Padding = new System.Windows.Forms.Padding(2);
            this.gbHist.Size = new System.Drawing.Size(166, 225);
            this.gbHist.TabIndex = 16;
            this.gbHist.TabStop = false;
            this.gbHist.Text = "Origen de la historificación";
            this.gbHist.Visible = false;
            // 
            // pnl_Hist_CSV
            // 
            this.pnl_Hist_CSV.Controls.Add(this.rb_Archivo);
            this.pnl_Hist_CSV.Controls.Add(this.rb_Directorio);
            this.pnl_Hist_CSV.Controls.Add(this.cb_ClaveAuto);
            this.pnl_Hist_CSV.Location = new System.Drawing.Point(0, 66);
            this.pnl_Hist_CSV.Margin = new System.Windows.Forms.Padding(2);
            this.pnl_Hist_CSV.Name = "pnl_Hist_CSV";
            this.pnl_Hist_CSV.Size = new System.Drawing.Size(162, 106);
            this.pnl_Hist_CSV.TabIndex = 7;
            // 
            // rb_Archivo
            // 
            this.rb_Archivo.AutoSize = true;
            this.rb_Archivo.Location = new System.Drawing.Point(9, 13);
            this.rb_Archivo.Margin = new System.Windows.Forms.Padding(2);
            this.rb_Archivo.Name = "rb_Archivo";
            this.rb_Archivo.Size = new System.Drawing.Size(127, 17);
            this.rb_Archivo.TabIndex = 0;
            this.rb_Archivo.Text = "Archivo seleccionado";
            this.rb_Archivo.UseVisualStyleBackColor = true;
            this.rb_Archivo.Visible = false;
            // 
            // rb_Directorio
            // 
            this.rb_Directorio.AutoSize = true;
            this.rb_Directorio.Enabled = false;
            this.rb_Directorio.Location = new System.Drawing.Point(9, 35);
            this.rb_Directorio.Margin = new System.Windows.Forms.Padding(2);
            this.rb_Directorio.Name = "rb_Directorio";
            this.rb_Directorio.Size = new System.Drawing.Size(145, 17);
            this.rb_Directorio.TabIndex = 2;
            this.rb_Directorio.TabStop = true;
            this.rb_Directorio.Text = "Todos Archivos directorio";
            this.rb_Directorio.UseVisualStyleBackColor = true;
            this.rb_Directorio.Visible = false;
            // 
            // cb_ClaveAuto
            // 
            this.cb_ClaveAuto.AutoSize = true;
            this.cb_ClaveAuto.Checked = true;
            this.cb_ClaveAuto.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_ClaveAuto.Enabled = false;
            this.cb_ClaveAuto.Location = new System.Drawing.Point(16, 84);
            this.cb_ClaveAuto.Margin = new System.Windows.Forms.Padding(2);
            this.cb_ClaveAuto.Name = "cb_ClaveAuto";
            this.cb_ClaveAuto.Size = new System.Drawing.Size(121, 17);
            this.cb_ClaveAuto.TabIndex = 4;
            this.cb_ClaveAuto.Text = "Clave Autonumerico";
            this.cb_ClaveAuto.UseVisualStyleBackColor = true;
            this.cb_ClaveAuto.Visible = false;
            this.cb_ClaveAuto.CheckedChanged += new System.EventHandler(this.cb_ClaveAuto_CheckedChanged);
            // 
            // rb_Hist_CSV
            // 
            this.rb_Hist_CSV.AutoSize = true;
            this.rb_Hist_CSV.Location = new System.Drawing.Point(9, 42);
            this.rb_Hist_CSV.Margin = new System.Windows.Forms.Padding(2);
            this.rb_Hist_CSV.Name = "rb_Hist_CSV";
            this.rb_Hist_CSV.Size = new System.Drawing.Size(80, 17);
            this.rb_Hist_CSV.TabIndex = 6;
            this.rb_Hist_CSV.TabStop = true;
            this.rb_Hist_CSV.Text = "Fichero csv";
            this.rb_Hist_CSV.UseVisualStyleBackColor = true;
            this.rb_Hist_CSV.CheckedChanged += new System.EventHandler(this.rb_Hist_CSV_CheckedChanged);
            // 
            // rb_Hist_Tabla
            // 
            this.rb_Hist_Tabla.AutoSize = true;
            this.rb_Hist_Tabla.Location = new System.Drawing.Point(9, 20);
            this.rb_Hist_Tabla.Margin = new System.Windows.Forms.Padding(2);
            this.rb_Hist_Tabla.Name = "rb_Hist_Tabla";
            this.rb_Hist_Tabla.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.rb_Hist_Tabla.Size = new System.Drawing.Size(138, 17);
            this.rb_Hist_Tabla.TabIndex = 5;
            this.rb_Hist_Tabla.TabStop = true;
            this.rb_Hist_Tabla.Text = "Tabla existente en PRO";
            this.rb_Hist_Tabla.UseVisualStyleBackColor = true;
            this.rb_Hist_Tabla.CheckedChanged += new System.EventHandler(this.rb_Hist_Tabla_CheckedChanged);
            // 
            // gbAcciones
            // 
            this.gbAcciones.Controls.Add(this.cb_IndexCS);
            this.gbAcciones.Controls.Add(this.cb_ChangeTrack);
            this.gbAcciones.Controls.Add(this.cb_CreateTable);
            this.gbAcciones.Location = new System.Drawing.Point(524, 78);
            this.gbAcciones.Margin = new System.Windows.Forms.Padding(2);
            this.gbAcciones.Name = "gbAcciones";
            this.gbAcciones.Padding = new System.Windows.Forms.Padding(2);
            this.gbAcciones.Size = new System.Drawing.Size(179, 90);
            this.gbAcciones.TabIndex = 17;
            this.gbAcciones.TabStop = false;
            this.gbAcciones.Text = "Acciones Comunes";
            this.gbAcciones.Visible = false;
            // 
            // cb_IndexCS
            // 
            this.cb_IndexCS.AutoSize = true;
            this.cb_IndexCS.Checked = true;
            this.cb_IndexCS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_IndexCS.Enabled = false;
            this.cb_IndexCS.Location = new System.Drawing.Point(4, 64);
            this.cb_IndexCS.Margin = new System.Windows.Forms.Padding(2);
            this.cb_IndexCS.Name = "cb_IndexCS";
            this.cb_IndexCS.Size = new System.Drawing.Size(159, 17);
            this.cb_IndexCS.TabIndex = 6;
            this.cb_IndexCS.Text = "Generar Indice ColumnStore";
            this.cb_IndexCS.UseVisualStyleBackColor = true;
            this.cb_IndexCS.Visible = false;
            // 
            // cb_ChangeTrack
            // 
            this.cb_ChangeTrack.AutoSize = true;
            this.cb_ChangeTrack.Checked = true;
            this.cb_ChangeTrack.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_ChangeTrack.Location = new System.Drawing.Point(4, 43);
            this.cb_ChangeTrack.Margin = new System.Windows.Forms.Padding(2);
            this.cb_ChangeTrack.Name = "cb_ChangeTrack";
            this.cb_ChangeTrack.Size = new System.Drawing.Size(165, 17);
            this.cb_ChangeTrack.TabIndex = 5;
            this.cb_ChangeTrack.Text = "Change Tracking Comentado";
            this.cb_ChangeTrack.UseVisualStyleBackColor = true;
            // 
            // cb_CreateTable
            // 
            this.cb_CreateTable.AutoSize = true;
            this.cb_CreateTable.Location = new System.Drawing.Point(4, 22);
            this.cb_CreateTable.Margin = new System.Windows.Forms.Padding(2);
            this.cb_CreateTable.Name = "cb_CreateTable";
            this.cb_CreateTable.Size = new System.Drawing.Size(144, 17);
            this.cb_CreateTable.TabIndex = 4;
            this.cb_CreateTable.Text = "Create Table Comentado";
            this.cb_CreateTable.UseVisualStyleBackColor = true;
            // 
            // txBBDD
            // 
            this.txBBDD.Location = new System.Drawing.Point(557, 8);
            this.txBBDD.Margin = new System.Windows.Forms.Padding(2);
            this.txBBDD.Name = "txBBDD";
            this.txBBDD.Size = new System.Drawing.Size(76, 20);
            this.txBBDD.TabIndex = 7;
            this.txBBDD.Visible = false;
            this.txBBDD.TextChanged += new System.EventHandler(this.txBBDD_TextChanged);
            // 
            // lbl_bd_microfocus
            // 
            this.lbl_bd_microfocus.AutoSize = true;
            this.lbl_bd_microfocus.Location = new System.Drawing.Point(497, 12);
            this.lbl_bd_microfocus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_bd_microfocus.Name = "lbl_bd_microfocus";
            this.lbl_bd_microfocus.Size = new System.Drawing.Size(57, 13);
            this.lbl_bd_microfocus.TabIndex = 18;
            this.lbl_bd_microfocus.Text = "BD origen:";
            this.lbl_bd_microfocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_bd_microfocus.Visible = false;
            this.lbl_bd_microfocus.Click += new System.EventHandler(this.label1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 314);
            this.Controls.Add(this.lbl_bd_microfocus);
            this.Controls.Add(this.txBBDD);
            this.Controls.Add(this.gbDSDM);
            this.Controls.Add(this.btnScript);
            this.Controls.Add(this.gbHist);
            this.Controls.Add(this.gbAcciones);
            this.Controls.Add(this.btRuta);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbl_file);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.txSalida);
            this.Controls.Add(this.txFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Warren";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbDSDM.ResumeLayout(false);
            this.gbDSDM.PerformLayout();
            this.gbHist.ResumeLayout(false);
            this.gbHist.PerformLayout();
            this.pnl_Hist_CSV.ResumeLayout(false);
            this.pnl_Hist_CSV.PerformLayout();
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
        private System.Windows.Forms.Panel pnl_Hist_CSV;
        private System.Windows.Forms.RadioButton rb_Hist_CSV;
        private System.Windows.Forms.RadioButton rb_Hist_Tabla;
        private System.Windows.Forms.Label lbl_bd_microfocus;
        private System.Windows.Forms.RadioButton rbHistCsv;
    }
}

