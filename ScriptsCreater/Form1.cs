using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;

namespace ScriptsCreater
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

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Generador Script v." + cr.version;
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
                txFile.Text = archivo;
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
                }
                else if (archivo.ToLower().Contains("ds"))
                {
                    rbDSDM.Checked = true;
                    gbDSDM.Visible = true;
                    gbHist.Visible = false;
                    gbAcciones.Visible = true;
                    cb_ChangeTrack.Text = "Change Tracking Comentado";
                    cb_ChangeTrack.Checked = true;
                    cb_ChangeTrack.Visible = true;
                    cb_IndexCS.Visible = false;
                }
                else if (archivo.ToLower().Contains("int"))
                {
                    rbIntegridad.Checked = true;
                    gbDSDM.Visible = false;
                    gbHist.Visible = false;
                    gbAcciones.Visible = false;
                    cb_ChangeTrack.Visible = false;
                    cb_IndexCS.Visible = false;
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
                //Genera Historificación
                else if (rbHist.Checked)
                {
                    string fichero = "";
                    string linegen = "OK";
                    //Para un archivo
                    if (rb_Archivo.Checked == true)
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
                            linegen = sh.hist(archivo, csv, ruta, ref arcScript, cb_ClaveAuto.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked);
                            fichero = fichero + "\n\r" + arcScript;
                        }
                    }
                    //Para todos los archivos de la Carpeta
                    else if (rb_Directorio.Checked == true)
                    {
                        txFile.Text = "*.csv";
                        string[] dirs = Directory.GetFiles(rutaorigen, "*.csv");
                        foreach (string dir in dirs)
                        {
                            csv = null;
                            csv = cr.leerCSV(archivo, rutaorigen);

                            archivo = dir.Replace(rutaorigen, "");

                            linegen = sh.hist(archivo, csv, ruta, ref arcScript, cb_ClaveAuto.Checked, cb_CreateTable.Checked, cb_ChangeTrack.Checked);
                            fichero = fichero + "\n\r" + arcScript;
                        }
                    }
                    //Si no seleccionas ningúna Opción
                    else
                    {
                        MessageBox.Show("Debe seleccionar una opción Historificación para generar los ficheros", "Generación Ficheros", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }

                    //Si todo es correcto
                    if (linegen == "OK")
                    {
                        MessageBox.Show("Ficheros generados en " + ruta + fichero, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

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
        }

        private void rbHist_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = false;
            gbHist.Visible = true;
            rb_Archivo.Checked = true;
            gbAcciones.Visible = true;
            cb_ChangeTrack.Text = "Change Tracking Comentado";
            cb_ChangeTrack.Checked = true;
            cb_ChangeTrack.Visible = true;
            cb_IndexCS.Visible = false;
        }

        private void rbDSDM_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = true;
            gbHist.Visible = false;
            gbAcciones.Visible = true;
            cb_ChangeTrack.Text = "Change Tracking Comentado";
            cb_ChangeTrack.Checked = true;
            cb_ChangeTrack.Visible = true;
            cb_IndexCS.Visible = false;
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

    }
}
