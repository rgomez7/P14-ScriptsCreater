﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;

namespace ScriptsCreator
{
    public partial class Form1 : Form
    {
        string archivoruta;
        string archivo;
        string rutaorigen;
        string ruta;
        string[] csv = new string[0];
        Acciones cr = new Acciones();
        ScriptMaestros sm = new ScriptMaestros();
        ScriptIntegridad sinteg = new ScriptIntegridad();
        ScriptDS ds = new ScriptDS();
        ScriptDM dm = new ScriptDM();
        ScriptHist sh = new ScriptHist();
        ScriptLectorCSV lec = new ScriptLectorCSV();
        ScriptGen_MF_STG mf_stg = new ScriptGen_MF_STG();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Generador Script v." + cr.version;
            //this.Text = "";
            txFile.Width = txSalida.Width;
            txBBDD.Visible = false;
            lbl_bd_microfocus.Visible = false;
            //Oculto por defecto todos los radiobuttons de opciones que van vía clojure
            gbDSDM.Visible = false;
            rbIntegridad.Visible = false;
            rbMaestro.Visible = false;
        }

        //Esto es para el boton de seleción de archivos
        private void button1_Click(object sender, EventArgs e)
        {
            csv = new string[0];

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Archivo CSV|*.csv";
            openFileDialog1.Title = "Seleccione un Archivo";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                archivoruta = openFileDialog1.FileName;
                archivo = openFileDialog1.SafeFileName;
                txFile.Text = archivoruta;
                rutaorigen = archivoruta.Replace(archivo, "");
                if (txSalida.Text == "")
                {
                    ruta = archivoruta.Replace(archivo, "");
                    if (ruta.ToLower().Contains("csv"))
                    {
                        ruta = ruta.ToLower().Replace("csv\\", "");
                    }
                    txSalida.Text = ruta;
                }
                
                //Pasamos valor CSV a variables
                csv = cr.leerCSV(archivo, rutaorigen);
                if (csv.Length == 0)
                {
                    txFile.Text = "";
                    txSalida.Text = "";
                }
                
                //Acciones a realizar dependiendo del check selecionado
                if (archivo.ToLower().Contains("mae"))
                {
                    rbMaestro.Checked = true;
                    gbDSDM.Visible = false;
                    gbHist.Visible = false;
                    gbAcciones.Visible = true;                    
                    cb_ChangeTrack.Text = "Activar Change Tracking";
                    cb_ChangeTrack.Checked = false;
                    cb_ChangeTrack.Visible = true;
                    cb_IndexCS.Visible = false;
                    txFile.ReadOnly = true;
                    btnBuscar.Enabled = true;
                    txFile.Width = txSalida.Width;
                    txBBDD.Visible = false;
                    lbl_bd_microfocus.Visible = false;
                }
                else if (archivo.ToLower().Contains("ds"))
                {
                    rbDSDM.Checked = true;
                    //gbDSDM.Visible = true;
                    gbHist.Visible = false;
                    gbAcciones.Visible = true;
                    cb_ChangeTrack.Text = "Change Tracking Comentado";
                    cb_ChangeTrack.Checked = true;
                    cb_ChangeTrack.Visible = true;
                    cb_IndexCS.Visible = false;
                    txFile.ReadOnly = true;
                    btnBuscar.Enabled = true;
                    txFile.Width = txSalida.Width;
                    txBBDD.Visible = false;
                    lbl_bd_microfocus.Visible = false;
                }
                else if (archivo.ToLower().Contains("int"))
                {
                    rbIntegridad.Checked = true;
                    gbDSDM.Visible = false;
                    gbHist.Visible = false;
                    gbAcciones.Visible = false;
                    cb_ChangeTrack.Visible = false;
                    cb_IndexCS.Visible = false;
                    txFile.ReadOnly = true;
                    btnBuscar.Enabled = true;
                    txFile.Width = txSalida.Width;
                    txBBDD.Visible = false;
                    lbl_bd_microfocus.Visible = false;
                }
                else if (archivo.ToLower().Contains("his"))
                {
                    rbHist.Checked = true;
                    gbDSDM.Visible = false;
                    gbHist.Visible = true;
                    rb_Archivo.Checked = true;
                    gbAcciones.Visible = true;
                    cb_ChangeTrack.Text = "Change Tracking Comentado";
                    cb_ChangeTrack.Checked = true;
                    cb_ChangeTrack.Visible = true;
                    cb_IndexCS.Visible = false;
                    txFile.ReadOnly = true;
                    btnBuscar.Enabled = true;
                    txFile.Width = txSalida.Width;
                    txBBDD.Visible = false;
                    lbl_bd_microfocus.Visible = false;
                }
            }
        }

        //Esto es para el botón donde se van a guardar los archivos
        private void btRuta_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

            folderBrowserDialog1.SelectedPath = ruta;

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                ruta = folderBrowserDialog1.SelectedPath + "\\";
                txSalida.Text = ruta;
            }
        }

        //Esto es para el botón de generación de script
        private void btnScript_Click(object sender, EventArgs e)
        {
            string arcScript = "";
            string[] lineas = new string[0];
            ruta = txSalida.Text;

            if (ruta.Length == 0)
            {
                MessageBox.Show("Debe seleccionar una ruta donde generar los ficheros", "Seleccionar Ruta", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            { 
                if (ruta.Substring(ruta.Length - 1, 1) != "\\" )
                {
                    ruta = ruta + "\\";
                    txSalida.Text = ruta;
                }

                if (txFile.Text != "")
                {
                    //Opción maestro
                    if (rbMaestro.Checked)
                    {
                        csv = cr.leerCSV(archivo, rutaorigen);
                        if (csv.Length == 0)
                        {
                            txFile.Text = "";
                            txSalida.Text = "";
                            MessageBox.Show("Debe seleccionar un CSV para generar el fichero", "Selección CSV", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        else
                        {
                            string linegen = sm.ScMaestro(archivo, csv, ruta, ref arcScript, cb_CreateTable.Checked, cb_ChangeTrack.Checked);

                            if (linegen == "OK")
                            {
                                MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    //Opción Integridad
                    else if (rbIntegridad.Checked)
                    {
                        csv = cr.leerCSV(archivo, rutaorigen);
                        if (csv.Length == 0)
                        {
                            txFile.Text = "";
                            txSalida.Text = "";
                            MessageBox.Show("Debe seleccionar un CSV para generar el fichero", "Selección CSV", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        else
                        {
                            string linegen = sinteg.ScIntegridad(archivo, csv, ruta, ref arcScript);

                            if (linegen == "OK")
                            {
                                MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    //Opción DS DM
                    else if (rbDSDM.Checked)
                    {
                        csv = cr.leerCSV(archivo, rutaorigen);
                        if (csv.Length == 0)
                        {
                            txFile.Text = "";
                            txSalida.Text = "";
                            MessageBox.Show("Debe seleccionar un CSV para generar el fichero", "Selección CSV", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        else
                        {
                            //Genera tabla
                            if (rb_DSDM_T.Checked == true)
                            {
                                string linegen = ds.table(archivo, csv, ruta, ref arcScript, cbIncremental.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked);

                                if (linegen == "OK")
                                {
                                    MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            //Genera DS
                            else if (rb_DSDM_DS.Checked == true)
                            {
                                string linegen = ds.ds(archivo, csv, ruta, ref arcScript, cbIncremental.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked);

                                if (linegen == "OK")
                                {
                                    MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            //Genera DM
                            else if (rb_DSDM_DM.Checked == true)
                            {
                                string linegen = dm.dm(archivo, csv, ruta, ref arcScript, cbIncremental.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked, cb_IndexCS.Checked);

                                if (linegen == "OK")
                                {
                                    MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            //Genera DS y DM
                            else if (rb_DSDM_All.Checked == true)
                            {
                                string fichero = "";
                                string linegen = "OK";

                                //DS
                                linegen = ds.ds(archivo, csv, ruta, ref arcScript, cbIncremental.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked);
                                fichero = fichero + "\n\r" + arcScript;

                                //DM
                                linegen = dm.dm(archivo, csv, ruta, ref arcScript, cbIncremental.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked, cb_IndexCS.Checked);
                                fichero = fichero + "\n\r" + arcScript;

                                if (linegen == "OK")
                                {
                                    MessageBox.Show("Ficheros generados en " + ruta + fichero, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            //Si no seleccionas ningúna Opción
                            else
                            {
                                MessageBox.Show("Debe seleccionar una opción DS-DM para generar los ficheros", "Generación Ficheros", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                    }

                    //Opción Historificación desde tabla
                    else if (rbHist.Checked)
                    {
                        string fichero = "";
                        string linegen = "OK";
                        DataTable dtSP = new DataTable("Object_SP");
                        dtSP.Columns.Add("SP", typeof(String));
                        dtSP.Columns.Add("SP_TL", typeof(String));
                        dtSP.Columns.Add("CT", typeof(String));
                        dtSP.Columns.Add("TL_gen", typeof(String));
                        
                        string[] tablas;

                        if (txBBDD.TextLength == 0)
                        {
                            MessageBox.Show("Debe indicar la BBDD donde se encuentran las Tablas origen para generar la Historificación", "Indicar BBDD de Historificación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (txFile.TextLength == 0)
                        {
                            MessageBox.Show("Debe indicar las Tablas Origen para generar la Historificación", "Indicar Tablas de Historificación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            if (txFile.Text.ToString().Contains(";"))
                            {
                                tablas = txFile.Text.ToString().Split(';');
                            }
                            else
                            {
                                tablas = new string[1];
                                tablas[0] = txFile.Text.ToString();
                            }

                            foreach (string tab in tablas)
                            {
                                linegen = sh.Hist_tabla(tab, txBBDD.Text.ToString(), ref arcScript, ref dtSP, ruta);
                                fichero = fichero + "\n\r" + arcScript;
                            }
                            //Generar archivo carga precondiciones
                            linegen = sh.csv_precondiciones(dtSP, txBBDD.Text.ToString(), ruta);

                            //Si todo es correcto
                            if (linegen == "OK")
                            {
                                MessageBox.Show("Ficheros generados en " + ruta + fichero, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }

                    //Opción Historificación desde CSV
                    //else if (rbHistCsv.Checked)
                    //{
                    //    string fichero = "";
                    //    string linegen = "OK";
                    //    DataTable dtSP = new DataTable("Object_SP");
                    //    dtSP.Columns.Add("SP", typeof(String));
                    //    dtSP.Columns.Add("SP_TL", typeof(String));
                    //    dtSP.Columns.Add("CT", typeof(String));
                    //    dtSP.Columns.Add("TL_gen", typeof(String));
                    //
                    //    //Para un archivo
                    //    if (txFile.Text.EndsWith(".csv") == true)
                    //    {
                    //        csv = cr.leerCSV(archivo, rutaorigen);
                    //        if (csv.Length == 0)
                    //        {
                    //            txFile.Text = "";
                    //            txSalida.Text = "";
                    //            MessageBox.Show("Debe seleccionar un CSV para generar el fichero", "Selección CSV", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //        }
                    //        else
                    //        {
                    //            linegen = sh.hist(archivo, csv, ruta, ref arcScript, cb_ClaveAuto.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked);
                    //            fichero = fichero + "\n\r" + arcScript;
                    //        }
                    //    }
                    //
                    //    //Para todos los archivos de la Carpeta
                    //    //else if (txFile.Text.EndsWith("\\") == true)
                    //    //{
                    //    //    txFile.Text = "*.csv";
                    //    //    string[] dirs = Directory.GetFiles(rutaorigen, "*.csv");
                    //    //    foreach (string dir in dirs)
                    //    //    {
                    //    //        csv = null;
                    //    //        csv = cr.leerCSV(archivo, rutaorigen);
                    //    //
                    //    //        archivo = dir.Replace(rutaorigen, "");
                    //    //
                    //    //        linegen = sh.hist(archivo, csv, ruta, ref arcScript, cb_ClaveAuto.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked);
                    //    //        fichero = fichero + "\n\r" + arcScript;
                    //    //    }
                    //    //}
                    //    
                    //    //Si no se proporciona ninguna opción correcta
                    //    else
                    //    {
                    //        MessageBox.Show("Solo se admiten ficheros con extensión .csv", "Formato de fichero incorrecto", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //        linegen = "Formato de fichero de entrada incorrecto";
                    //    }
                    //
                    //    //Si todo es correcto
                    //    if (linegen == "OK")
                    //    {
                    //        MessageBox.Show("Ficheros generados en " + ruta + fichero, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    }
                    //}








                    //Opción generación Script con Union ALL de los campos
                    else if (rbLectorCSV.Checked == true)
                    {
                        csv = cr.leerCSV(archivo, rutaorigen);
                        string linegen = lec.selectUnion(archivo, csv, ruta, ref arcScript);

                        if (linegen == "OK")
                        {
                            MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    // Generación de Scripts y CSV de extracción desde tabla de MicroFocus
                    else if (rb_tabMF_STG.Checked == true)
                    {
                        string linegen;
                        string datosScript = "";
                        string nombreCSV = "";
                        string camposPK = "";
                        string nombreBD = "";
                        string table = "";
                        string schema = "";
                        int activoCT = 0;

                        nombreBD = txBBDD.Text;

                        if (txFile.Text.Contains("."))
                        {
                            string[] datos = txFile.Text.Split('.');
                            schema = datos[0];
                            table = datos[1];
                        }
                        else
                        {
                            schema = "DB2PROD";
                            table = txFile.Text;
                            txFile.Text = "DB2PROD." + txFile.Text;
                        }
                        
                        int existetab = mf_stg.ExisteTabla(table, schema, ref nombreBD);

                        if (existetab == 1)
                        {
                            linegen = mf_stg.createtable_stgFinal(table, schema, ruta, ref arcScript, ref camposPK, nombreBD, ref activoCT);
                            datosScript = datosScript + "\n\r" + arcScript;

                           //Obsoleto. Ya no se usa el CT, sino el CDC
                            //if (activoCT == 0)
                            //{
                            //    linegen = mf_stg.activarCT_microfocus(table, schema, ruta, ref arcScript, camposPK.ToUpper(), nombreBD, "Pre");
                            //    datosScript = datosScript + "\n\r" + arcScript;
                            //    linegen = mf_stg.activarCT_microfocus(table, schema, ruta, ref arcScript, camposPK.ToUpper(), nombreBD, "Pro");
                            //    datosScript = datosScript + "\n\r" + arcScript;
                            //}

                            linegen = mf_stg.gencsv(table, schema, ruta, ref arcScript, camposPK, nombreBD);
                            datosScript = datosScript + "\n\r" + arcScript;
                            nombreCSV = arcScript;
                            linegen = mf_stg.createtable_extraccion(nombreCSV, ruta, ref arcScript);
                            datosScript = datosScript + "\n\r" + arcScript;
                            linegen = mf_stg.createSP_extraccion(nombreCSV, ruta, ref arcScript);
                            datosScript = datosScript + "\n\r" + arcScript;
                            linegen = mf_stg.crearVariableConsulta(nombreCSV, ruta, ref arcScript, nombreBD);
                            datosScript = datosScript + "\n\r" + arcScript;
                            linegen = mf_stg.crearVariableConsulta_CDC(nombreCSV, ruta, ref arcScript, nombreBD);
                            datosScript = datosScript + "\n\r" + arcScript;

                            if (linegen == "OK")
                            {
                                txBBDD.Text = nombreBD;
                                MessageBox.Show("Fichero generado en " + ruta + datosScript, "Fichero generado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No existe la tabla " + txFile.Text + " en la BBDD " + nombreBD, "Tabla no existe en BBDD", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    //Generación de Script extracción desde CSV
                    else if (rb_CSV_MF_STG.Checked == true)
                    {
                        string fichero = "";
                        string linegen;
                        string datosScript = "";

                        linegen = mf_stg.createtable_extraccion(archivo, rutaorigen, ref arcScript, ruta);
                        datosScript = datosScript + "\n\r" + arcScript;
                        linegen = mf_stg.createSP_extraccion(archivo, rutaorigen, ref arcScript, ruta);
                        datosScript = datosScript + "\n\r" + arcScript;

                        if (txFile.Text.EndsWith(".csv") == true)
                        {
                            csv = cr.leerCSV(archivo, rutaorigen);
                            if (csv.Length == 0)
                            {
                                txFile.Text = "";
                                txSalida.Text = "";
                                MessageBox.Show("Debe seleccionar un CSV para generar el fichero", "Selección CSV", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            else
                            {
                                linegen = mf_stg.createtable_stgFinal(archivo, rutaorigen, ref arcScript, ruta);
                                fichero = fichero + "\n\r" + arcScript;
                            }
                        }

                        if (linegen == "OK")
                        {
                            MessageBox.Show("Fichero generado en " + ruta + datosScript, "Fichero generado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                }


                //Si no seleccionas ningún Tipo Script
                else
                {
                    MessageBox.Show("Debe seleccionar una opción en Tipo Script para generar los ficheros", "Generación Tipo Script", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        #region "Modificación RadioButton"

        private void rbIntegridad_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = false;
            gbHist.Visible = false;
            gbAcciones.Visible = false;
            cb_ChangeTrack.Visible = false;
            cb_IndexCS.Visible = false;
            txFile.ReadOnly = true;
            lbl_file.Text = "CSV a generar";
            btnBuscar.Enabled = true;
            txFile.Width = txSalida.Width;
            txBBDD.Visible = false;
            lbl_bd_microfocus.Visible = false;
            gbAcciones.Visible = true;
        }

        private void rbMaestro_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = false;
            gbHist.Visible = false;
            gbAcciones.Visible = true;
            cb_ChangeTrack.Text = "Activar Change Tracking";
            cb_ChangeTrack.Checked = false;
            cb_ChangeTrack.Visible = true;
            cb_IndexCS.Visible = false;
            txFile.ReadOnly = true;
            lbl_file.Text = "CSV a generar";
            btnBuscar.Enabled = true;
            txFile.Width = txSalida.Width;
            txBBDD.Visible = false;
            lbl_bd_microfocus.Visible = false;
            gbAcciones.Visible = true;
        }

        private void rbHist_CheckedChanged(object sender, EventArgs e)
        {
            lbl_file.Visible = true;
            lbl_file.Text = "Tablas origen: ";
            lbl_file.Width = 76;

            txFile.Visible = true;
            txFile.Enabled = true;
            txFile.ReadOnly = false;
            txFile.Width = 389;

            lbl_bd_microfocus.Visible = true;
            lbl_bd_microfocus.Text = "BD origen: ";
            lbl_bd_microfocus.Width = 56;

            txBBDD.Visible = true;
            txBBDD.Enabled = true;
            txBBDD.Width = 155;

            btnBuscar.Visible = false;
            btnBuscar.Enabled = false;

            label2.Visible = true;

            txSalida.Visible = true;
            txSalida.Enabled = true;
            txSalida.ReadOnly = false;
            txSalida.Width = 529;

            btRuta.Enabled = true;

            gbHist.Visible = false;

            rb_Hist_Tabla.Visible = false;
            rb_Hist_Tabla.Enabled = false;

            rb_Hist_CSV.Visible = false;
            rb_Hist_CSV.Enabled = false;

            rb_Archivo.Visible = false;

            rb_Directorio.Visible = false;

            cb_ClaveAuto.Visible = false;

            gbDSDM.Visible = false;

            gbAcciones.Visible = false;

            btnScript.Visible = true;
            btnScript.Enabled = true;
            btnScript.Location = new System.Drawing.Point(18,200);

            //gbDSDM.Width = 360;
            //gbHist.Visible = true;
            //rb_Archivo.Checked = true;
            //gbAcciones.Visible = true;
            //cb_ChangeTrack.Text = "Change Tracking Comentado";
            //cb_ChangeTrack.Checked = true;
            //cb_ChangeTrack.Visible = true;
            //cb_IndexCS.Visible = false;
            //lbl_file.Text = "CSV a generar";
            //btnBuscar.Enabled = true;
            //lbl_bd_microfocus.Visible = true;
            //lbl_bd_microfocus.Text = "BD origen: ";
            //lbl_bd_microfocus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //txFile.ReadOnly = true;
            //txFile.Width = txFile.Width - txBBDD.Width - lbl_bd_microfocus.Width - 5;
            //txFile.Visible = true;
            //txBBDD.Visible = true;
            //gbAcciones.Visible = false;
        }

        private void rbDSDM_CheckedChanged(object sender, EventArgs e)
        {
            //gbDSDM.Visible = true;
            gbHist.Visible = false;
            gbAcciones.Visible = true;
            cb_ChangeTrack.Text = "Change Tracking Comentado";
            cb_ChangeTrack.Checked = true;
            cb_ChangeTrack.Visible = true;
            cb_IndexCS.Visible = false;
            txFile.ReadOnly = true;
            lbl_file.Text = "CSV a generar";
            btnBuscar.Enabled = true;
            txFile.Width = txSalida.Width;
            txBBDD.Visible = false;
            lbl_bd_microfocus.Visible = false;
            gbAcciones.Visible = true;
        }

        private void rbLectorCSV_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = false;
            gbHist.Visible = false;
            gbAcciones.Visible = false;
            cb_ChangeTrack.Text = "Change Tracking Comentado";
            cb_ChangeTrack.Checked = false;
            cb_ChangeTrack.Visible = false;
            cb_IndexCS.Visible = false;
            txFile.ReadOnly = true;
            lbl_file.Text = "CSV a generar";
            btnBuscar.Enabled = true;
            txFile.Width = txSalida.Width;
            txBBDD.Visible = false;
            lbl_bd_microfocus.Visible = false;
            gbAcciones.Visible = false;
        }

        private void rb_tabMF_STG_CheckedChanged(object sender, EventArgs e)
        {
            lbl_file.Visible = true;
            lbl_file.Text = "Tablas origen: ";
            lbl_file.Width = 76;

            txFile.Visible = true;
            txFile.Enabled = true;
            txFile.ReadOnly = false;
            txFile.Width = 389;

            lbl_bd_microfocus.Visible = true;
            lbl_bd_microfocus.Text = "BD origen: ";
            lbl_bd_microfocus.Width = 56;

            txBBDD.Visible = true;
            txBBDD.Enabled = true;
            txBBDD.Width = 155;

            btnBuscar.Visible = false;
            btnBuscar.Enabled = false;
            btnBuscar.Text = "Buscar";

            label2.Visible = true;

            txSalida.Visible = true;
            txSalida.Enabled = true;
            txSalida.ReadOnly = false;
            txSalida.Width = 529;

            btRuta.Enabled = true;

            gbHist.Visible = false;

            rb_Hist_Tabla.Visible = false;
            rb_Hist_Tabla.Enabled = false;

            rb_Hist_CSV.Visible = false;
            rb_Hist_CSV.Enabled = false;

            rb_Archivo.Visible = false;

            rb_Directorio.Visible = false;

            cb_ClaveAuto.Visible = false;

            gbDSDM.Visible = false;

            gbAcciones.Visible = false;

            btnScript.Visible = true;
            btnScript.Enabled = true;
            btnScript.Location = new System.Drawing.Point(18, 200);
        }

        private void rb_CSV_MF_STG_CheckedChanged(object sender, EventArgs e)
        {
            lbl_file.Visible = true;
            lbl_file.Text = "CSV origen: ";
            lbl_file.Width = 76;

            txFile.Visible = true;
            txFile.Enabled = true;
            txFile.ReadOnly = false;
            txFile.Width = 529;

            lbl_bd_microfocus.Visible = false;
            lbl_bd_microfocus.Text = "BD origen: ";
            lbl_bd_microfocus.Width = 56;

            txBBDD.Visible = false;
            txBBDD.Enabled = true;
            txBBDD.Width = 76;

            btnBuscar.Visible = true;
            btnBuscar.Enabled = true;

            label2.Visible = true;

            txSalida.Visible = true;
            txSalida.Enabled = true;
            txSalida.ReadOnly = false;
            txSalida.Width = 529;

            btRuta.Enabled = true;

            gbHist.Visible = false;

            rb_Hist_Tabla.Visible = false;
            rb_Hist_Tabla.Enabled = false;

            rb_Hist_CSV.Visible = false;
            rb_Hist_CSV.Enabled = false;

            rb_Archivo.Visible = false;

            rb_Directorio.Visible = false;

            cb_ClaveAuto.Visible = false;

            gbDSDM.Visible = false;

            gbAcciones.Visible = false;

            btnScript.Visible = true;
            btnScript.Enabled = true;
            btnScript.Location = new System.Drawing.Point(18, 200);
        }

        private void rb_Hist_Tabla_CheckedChanged(object sender, EventArgs e)
        {
            pnl_Hist_CSV.Visible = false;
            lbl_file.Text = "Tablas (sep. ;):";
            btnBuscar.Enabled = false;
            txFile.ReadOnly = false;
            txFile.Width = txFile.Width - txBBDD.Width - 2;
            txBBDD.Visible = true;
            lbl_bd_microfocus.Visible = false;
        }

        private void rb_Hist_CSV_CheckedChanged(object sender, EventArgs e)
        {
            pnl_Hist_CSV.Visible = true;
            lbl_file.Text = "CSV a generar";
            btnBuscar.Enabled = true;
            txFile.ReadOnly = true;
            txFile.Width = txSalida.Width;
            txBBDD.Visible = false;
            lbl_bd_microfocus.Visible = false;
        }

        private void rb_DSDM_T_CheckedChanged(object sender, EventArgs e)
        {
            cb_IndexCS.Visible = false;
        }

        private void rb_DSDM_DS_CheckedChanged(object sender, EventArgs e)
        {
            cb_IndexCS.Visible = false;
        }

        private void rb_DSDM_DM_CheckedChanged(object sender, EventArgs e)
        {
            cb_IndexCS.Visible = true;
        }

        private void rb_DSDM_All_CheckedChanged(object sender, EventArgs e)
        {
            cb_IndexCS.Visible = true;
        }

        #endregion

        private void lbl_file_Click(object sender, EventArgs e)
        {

        }

        private void txBBDD_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txFile_TextChanged(object sender, EventArgs e)
        {

        }

        private void cb_ClaveAuto_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void rbHistCsv_CheckedChanged(object sender, EventArgs e)
        {
            lbl_file.Visible = true;
            lbl_file.Text = "CSV origen: ";
            lbl_file.Width = 76;

            txFile.Visible = true;
            txFile.Enabled = true;
            txFile.ReadOnly = false;
            txFile.Width = 529;

            lbl_bd_microfocus.Visible = false;
            lbl_bd_microfocus.Text = "BD origen: ";
            lbl_bd_microfocus.Width = 56;

            txBBDD.Visible = false;
            txBBDD.Enabled = true;
            txBBDD.Width = 76;

            btnBuscar.Visible = true;
            btnBuscar.Enabled = true;

            label2.Visible = true;

            txSalida.Visible = true;
            txSalida.Enabled = true;
            txSalida.ReadOnly = false;
            txSalida.Width = 529;

            btRuta.Enabled = true;

            gbHist.Visible = false;

            rb_Hist_Tabla.Visible = false;
            rb_Hist_Tabla.Enabled = false;

            rb_Hist_CSV.Visible = false;
            rb_Hist_CSV.Enabled = false;

            rb_Archivo.Visible = false;

            rb_Directorio.Visible = false;

            cb_ClaveAuto.Visible = false;

            gbDSDM.Visible = false;

            gbAcciones.Visible = false;

            btnScript.Visible = true;
            btnScript.Enabled = true;
            btnScript.Location = new System.Drawing.Point(18, 200);
        }
    }
}
