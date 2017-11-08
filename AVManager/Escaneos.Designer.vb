<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Escaneos
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Escaneos))
        Me.dtgEquipos = New System.Windows.Forms.DataGridView()
        Me.Numero = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Disks = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.UltimoEscaneo = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.cmsEquipos = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.EscanearDiscoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.btnEmpiezaEscaneos = New System.Windows.Forms.Button()
        Me.dtgEscaneos = New System.Windows.Forms.DataGridView()
        Me.cmsEscaneos = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.CancelarEscaneoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReiniciarEscaneoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DataGridViewProgressColumn1 = New AVManager.DataGridViewProgressColumn()
        Me.btnAgregar = New System.Windows.Forms.Button()
        Me.lblEquipos = New System.Windows.Forms.Label()
        Me.lblValidos = New System.Windows.Forms.Label()
        Me.lblErrores = New System.Windows.Forms.Label()
        Me.lblEscaneos = New System.Windows.Forms.Label()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.Disco = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.EscaneoProgreso = New AVManager.DataGridViewProgressColumn()
        Me.Archivo = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.dtgEquipos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.cmsEquipos.SuspendLayout()
        CType(Me.dtgEscaneos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.cmsEscaneos.SuspendLayout()
        Me.SuspendLayout()
        '
        'dtgEquipos
        '
        Me.dtgEquipos.AllowUserToAddRows = False
        Me.dtgEquipos.AllowUserToDeleteRows = False
        Me.dtgEquipos.AllowUserToResizeColumns = False
        Me.dtgEquipos.AllowUserToResizeRows = False
        Me.dtgEquipos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dtgEquipos.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Numero, Me.Disks, Me.UltimoEscaneo})
        Me.dtgEquipos.Location = New System.Drawing.Point(40, 35)
        Me.dtgEquipos.Name = "dtgEquipos"
        Me.dtgEquipos.ReadOnly = True
        Me.dtgEquipos.RowHeadersVisible = False
        Me.dtgEquipos.RowTemplate.ContextMenuStrip = Me.cmsEquipos
        Me.dtgEquipos.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.dtgEquipos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dtgEquipos.Size = New System.Drawing.Size(350, 510)
        Me.dtgEquipos.TabIndex = 12
        '
        'Numero
        '
        Me.Numero.HeaderText = "# Equipo"
        Me.Numero.Name = "Numero"
        Me.Numero.ReadOnly = True
        Me.Numero.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Numero.Width = 75
        '
        'Disks
        '
        Me.Disks.HeaderText = "Discos"
        Me.Disks.Name = "Disks"
        Me.Disks.ReadOnly = True
        Me.Disks.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Disks.Width = 160
        '
        'UltimoEscaneo
        '
        Me.UltimoEscaneo.HeaderText = "Ultimo Escaneo"
        Me.UltimoEscaneo.Name = "UltimoEscaneo"
        Me.UltimoEscaneo.ReadOnly = True
        Me.UltimoEscaneo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.UltimoEscaneo.Width = 115
        '
        'cmsEquipos
        '
        Me.cmsEquipos.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.EscanearDiscoToolStripMenuItem})
        Me.cmsEquipos.Name = "cmsEquipos"
        Me.cmsEquipos.Size = New System.Drawing.Size(153, 26)
        '
        'EscanearDiscoToolStripMenuItem
        '
        Me.EscanearDiscoToolStripMenuItem.Name = "EscanearDiscoToolStripMenuItem"
        Me.EscanearDiscoToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.EscanearDiscoToolStripMenuItem.Text = "Escanear Disco"
        '
        'btnEmpiezaEscaneos
        '
        Me.btnEmpiezaEscaneos.Location = New System.Drawing.Point(659, 452)
        Me.btnEmpiezaEscaneos.Name = "btnEmpiezaEscaneos"
        Me.btnEmpiezaEscaneos.Size = New System.Drawing.Size(111, 38)
        Me.btnEmpiezaEscaneos.TabIndex = 11
        Me.btnEmpiezaEscaneos.Text = "Empezar Escaneos"
        Me.btnEmpiezaEscaneos.UseVisualStyleBackColor = True
        '
        'dtgEscaneos
        '
        Me.dtgEscaneos.AllowUserToAddRows = False
        Me.dtgEscaneos.AllowUserToDeleteRows = False
        Me.dtgEscaneos.AllowUserToResizeColumns = False
        Me.dtgEscaneos.AllowUserToResizeRows = False
        Me.dtgEscaneos.BackgroundColor = System.Drawing.Color.FloralWhite
        Me.dtgEscaneos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dtgEscaneos.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Disco, Me.EscaneoProgreso, Me.Archivo})
        Me.dtgEscaneos.ContextMenuStrip = Me.cmsEscaneos
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.White
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dtgEscaneos.DefaultCellStyle = DataGridViewCellStyle1
        Me.dtgEscaneos.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.dtgEscaneos.Location = New System.Drawing.Point(485, 35)
        Me.dtgEscaneos.MultiSelect = False
        Me.dtgEscaneos.Name = "dtgEscaneos"
        Me.dtgEscaneos.RowHeadersVisible = False
        Me.dtgEscaneos.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White
        Me.dtgEscaneos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dtgEscaneos.Size = New System.Drawing.Size(459, 384)
        Me.dtgEscaneos.TabIndex = 13
        '
        'cmsEscaneos
        '
        Me.cmsEscaneos.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CancelarEscaneoToolStripMenuItem, Me.ReiniciarEscaneoToolStripMenuItem})
        Me.cmsEscaneos.Name = "cmsEquipos"
        Me.cmsEscaneos.Size = New System.Drawing.Size(167, 48)
        '
        'CancelarEscaneoToolStripMenuItem
        '
        Me.CancelarEscaneoToolStripMenuItem.Name = "CancelarEscaneoToolStripMenuItem"
        Me.CancelarEscaneoToolStripMenuItem.Size = New System.Drawing.Size(166, 22)
        Me.CancelarEscaneoToolStripMenuItem.Text = "Cancelar Escaneo"
        '
        'ReiniciarEscaneoToolStripMenuItem
        '
        Me.ReiniciarEscaneoToolStripMenuItem.Name = "ReiniciarEscaneoToolStripMenuItem"
        Me.ReiniciarEscaneoToolStripMenuItem.Size = New System.Drawing.Size(166, 22)
        Me.ReiniciarEscaneoToolStripMenuItem.Text = "Reiniciar Escaneo"
        '
        'DataGridViewProgressColumn1
        '
        Me.DataGridViewProgressColumn1.HeaderText = "Escaneos"
        Me.DataGridViewProgressColumn1.Name = "DataGridViewProgressColumn1"
        Me.DataGridViewProgressColumn1.Width = 330
        '
        'btnAgregar
        '
        Me.btnAgregar.Location = New System.Drawing.Point(659, 507)
        Me.btnAgregar.Name = "btnAgregar"
        Me.btnAgregar.Size = New System.Drawing.Size(111, 38)
        Me.btnAgregar.TabIndex = 14
        Me.btnAgregar.Text = "Agregar Equipo"
        Me.btnAgregar.UseVisualStyleBackColor = True
        '
        'lblEquipos
        '
        Me.lblEquipos.AutoSize = True
        Me.lblEquipos.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEquipos.Location = New System.Drawing.Point(37, 548)
        Me.lblEquipos.Name = "lblEquipos"
        Me.lblEquipos.Size = New System.Drawing.Size(85, 15)
        Me.lblEquipos.TabIndex = 15
        Me.lblEquipos.Text = "# de Equipos: "
        '
        'lblValidos
        '
        Me.lblValidos.AutoSize = True
        Me.lblValidos.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblValidos.Location = New System.Drawing.Point(482, 422)
        Me.lblValidos.Name = "lblValidos"
        Me.lblValidos.Size = New System.Drawing.Size(60, 15)
        Me.lblValidos.TabIndex = 16
        Me.lblValidos.Text = "Válidos: 0"
        '
        'lblErrores
        '
        Me.lblErrores.AutoSize = True
        Me.lblErrores.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblErrores.Location = New System.Drawing.Point(656, 422)
        Me.lblErrores.Name = "lblErrores"
        Me.lblErrores.Size = New System.Drawing.Size(60, 15)
        Me.lblErrores.TabIndex = 17
        Me.lblErrores.Text = "Errores: 0"
        '
        'lblEscaneos
        '
        Me.lblEscaneos.AutoSize = True
        Me.lblEscaneos.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEscaneos.Location = New System.Drawing.Point(826, 422)
        Me.lblEscaneos.Name = "lblEscaneos"
        Me.lblEscaneos.Size = New System.Drawing.Size(97, 15)
        Me.lblEscaneos.TabIndex = 18
        Me.lblEscaneos.Text = "Total Escaneos: "
        '
        'lblVersion
        '
        Me.lblVersion.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblVersion.CausesValidation = False
        Me.lblVersion.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVersion.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.lblVersion.Location = New System.Drawing.Point(875, 563)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(97, 15)
        Me.lblVersion.TabIndex = 19
        Me.lblVersion.Text = "v2"
        Me.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Disco
        '
        Me.Disco.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.Disco.HeaderText = "Disco"
        Me.Disco.Name = "Disco"
        Me.Disco.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Disco.Width = 38
        '
        'EscaneoProgreso
        '
        Me.EscaneoProgreso.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.EscaneoProgreso.HeaderText = "Escaneos"
        Me.EscaneoProgreso.Name = "EscaneoProgreso"
        '
        'Archivo
        '
        Me.Archivo.HeaderText = "Archivos"
        Me.Archivo.Name = "Archivo"
        Me.Archivo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'Escaneos
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FloralWhite
        Me.ClientSize = New System.Drawing.Size(984, 587)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.lblEscaneos)
        Me.Controls.Add(Me.lblErrores)
        Me.Controls.Add(Me.lblValidos)
        Me.Controls.Add(Me.lblEquipos)
        Me.Controls.Add(Me.btnAgregar)
        Me.Controls.Add(Me.dtgEquipos)
        Me.Controls.Add(Me.btnEmpiezaEscaneos)
        Me.Controls.Add(Me.dtgEscaneos)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Escaneos"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Juguel Uniformes"
        CType(Me.dtgEquipos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.cmsEquipos.ResumeLayout(False)
        CType(Me.dtgEscaneos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.cmsEscaneos.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents dtgEquipos As DataGridView
    Friend WithEvents btnEmpiezaEscaneos As Button
    Friend WithEvents dtgEscaneos As DataGridView
    Friend WithEvents DataGridViewProgressColumn1 As DataGridViewProgressColumn
    Friend WithEvents btnAgregar As Button
    Friend WithEvents lblEquipos As Label
    Friend WithEvents lblValidos As Label
    Friend WithEvents lblErrores As Label
    Friend WithEvents lblEscaneos As Label
    Friend WithEvents lblVersion As Label
    Friend WithEvents cmsEquipos As ContextMenuStrip
    Friend WithEvents EscanearDiscoToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents cmsEscaneos As ContextMenuStrip
    Friend WithEvents CancelarEscaneoToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ReiniciarEscaneoToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Numero As DataGridViewTextBoxColumn
    Friend WithEvents Disks As DataGridViewTextBoxColumn
    Friend WithEvents UltimoEscaneo As DataGridViewTextBoxColumn
    Friend WithEvents Disco As DataGridViewTextBoxColumn
    Friend WithEvents EscaneoProgreso As DataGridViewProgressColumn
    Friend WithEvents Archivo As DataGridViewTextBoxColumn
End Class
