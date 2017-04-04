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
        ScriptDSDM dsdm = new ScriptDSDM();
        ScriptHist sh = new ScriptHist();

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
                string line;
                int i = 0;
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
                    ruta = archivoruta.Replace(archivo, "");
                    if (ruta.ToLower().Contains("csv"))
                    {
                        ruta = ruta.Replace("csv\\","");
                    }
                    txSalida.Text = ruta;

                //Pasamos valor CSV a variables
                try
                {
                    StreamReader file = new StreamReader(archivoruta);
                    while ((line = file.ReadLine()) != null)
                    {
                        Array.Resize(ref csv, csv.Length + 1);
                        csv[i] = line;
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    txFile.Text = "";
                    txSalida.Text = "";
                    MessageBox.Show("Error al abrir el archivo " + archivo + "\n\r"  + ex.Message,  "Error abrir fichero", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                if (archivo.ToLower().Contains("mae"))
                {
                    rbMaestro.Checked = true;
                    gbDSDM.Visible = false;
                    gbHist.Visible = false;
                }
                else if (archivo.ToLower().Contains("ds"))
                {
                    rbDSDM.Checked = true;
                    gbDSDM.Visible = true;
                    gbHist.Visible = false;
                }
                else if (archivo.ToLower().Contains("int"))
                {
                    rbIntegridad.Checked = true;
                    gbDSDM.Visible = false;
                    gbHist.Visible = false;
                }
                else if (archivo.ToLower().Contains("his"))
                {
                    rbHist.Checked = true;
                    gbDSDM.Visible = false;
                    gbHist.Visible = true;
                    rb_Archivo.Checked = true;
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
            string dev;
            string arcScript;
            string[] lineas = new string[0];
            ruta = txSalida.Text;

            if (rbMaestro.Checked)
            {
                //Primer archivo
                arcScript = "Script " + archivo.Replace("csv", "") + "sql";

                string linegen = sm.ScMaestro(archivo, csv, ruta, ref arcScript);
                //if (dev == "OK")
                //{
                //    //dev = cr.borrarfichero(ruta, arcScript);
                //}

                if (linegen == "OK")
                {
                    MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (rbIntegridad.Checked)
            {
                arcScript = "Script " + archivo.Replace("csv", "") + "sql";
                
                string linegen = sinteg.ScIntegridad(archivo, csv, ruta, ref arcScript);

                if (linegen == "OK")
                {
                    MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (rbDSDM.Checked)
            {
                if (rb_DSDM_T.Checked == true)
                {
                    arcScript = "Script " + archivo.Replace("csv", "") + "sql";

                    string linegen = dsdm.table(archivo, csv, ruta, ref arcScript, cbIncremental.Checked);

                    if (linegen == "OK")
                    {
                        MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (rb_DSDM_DS.Checked == true)
                {
                    arcScript = "Script normalizado_" + archivo.Replace("csv", "") + "sql";

                    string linegen = dsdm.ds(archivo, csv, ruta, ref arcScript, cbIncremental.Checked);

                    if (linegen == "OK")
                    {
                        MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (rb_DSDM_DM.Checked == true)
                {
                    arcScript = "Script dimensional_" + archivo.Replace("csv", "") + "sql";
                    dev = cr.comprobarficheros(ref lineas, ruta, arcScript, 1);

                    //string linegen = dsdm.dm(archivo, csv, ruta, ref arcScript,dev,cbIncremental.Checked);

                    MessageBox.Show("Ficheros generados en " + ruta + "\n\r" + arcScript, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (rb_DSDM_All.Checked == true)
                {
                    string fichero = "";

                    arcScript = "Script " + archivo.Replace("csv", "") + "sql";
                    dev = cr.comprobarficheros(ref lineas, ruta, arcScript, 1);
                    fichero = fichero + "\n\r" + arcScript;

                    //string linegen = dsdm.ds(archivo, csv, ruta, arcScript);

                    //dev = cr.comprobarficheros(ref lineas, ruta, arcScript, 1);
                    //string linegen = dsdm.dm(archivo, csv, ruta, arcScript);

                    MessageBox.Show("Ficheros generados en " + ruta + fichero, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Debe seleccionar una opción DS-DM para generar los ficheros", "Generación Ficheros", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                //dev = cr.comprobarficheros("RMRS", ruta, archivo);
            }
            else if (rbHist.Checked)
            {
                string fichero = "";
                string linegen = "OK";
                if (rb_Archivo.Checked == true)
                {
                    arcScript = "Script TL xxx_" + archivo.Replace(".csv", "") + "_tracelog_TL.sql";
                    
                    linegen = sh.hist(archivo, csv, ruta, ref arcScript, cb_ClaveAuto.Checked);
                    fichero = fichero + "\n\r" + arcScript;
                }
                else if (rb_Directorio.Checked == true)
                {
                    txFile.Text = "*.csv";
                    string[] dirs = Directory.GetFiles(rutaorigen, "*.csv");
                    foreach (string dir in dirs)
                    {
                        archivo = dir.Replace(rutaorigen,"");

                        arcScript = "Script TL xxx_" + archivo.Replace(".csv", "") + "_tracelog_TL.sql";
                        
                        linegen = sh.hist(archivo, csv, ruta, ref arcScript, cb_ClaveAuto.Checked);
                        fichero = fichero + "\n\r" + arcScript;
                    }
                }
                else
                {
                    MessageBox.Show("Debe seleccionar una opción Historificación para generar los ficheros", "Generación Ficheros", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                if (linegen == "OK")
                {
                    MessageBox.Show("Ficheros generados en " + ruta + fichero, "Ficheros generados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }



        }


        #region "Modificación RadioButton"

        private void rbIntegridad_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = false;
            gbHist.Visible = false;
        }

        private void rbMaestro_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = false;
            gbHist.Visible = false;
        }

        private void rbHist_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = false;
            gbHist.Visible = true;
            rb_Archivo.Checked = true;
        }

        private void rbDSDM_CheckedChanged(object sender, EventArgs e)
        {
            gbDSDM.Visible = true;
            gbHist.Visible = false;
        }

        #endregion

    }
}
